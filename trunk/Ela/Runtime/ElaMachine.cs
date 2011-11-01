using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Compilation;
using Ela.Debug;
using Ela.Linking;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
    public sealed class ElaMachine : IDisposable
	{
		#region Construction
		internal ElaValue[][] modules;
		private readonly CodeAssembly asm;
        private readonly IntrinsicFrame argMod;
        private readonly int argModHandle;
      	
		public ElaMachine(CodeAssembly asm)
		{
			this.asm = asm;
      		var frame = asm.GetRootModule();
			MainThread = new WorkerThread(asm);
			var lays = frame.Layouts[0];
			modules = new ElaValue[asm.ModuleCount][];
			var mem = new ElaValue[lays.Size];
			modules[0] = mem;

            argModHandle = asm.TryGetModuleHandle("$Args");

            if (argModHandle != -1)
            {
                argMod = (IntrinsicFrame)asm.GetModule(argModHandle);
                modules[argModHandle] = argMod.Memory;
                ReadPervasives(MainThread, argMod, argModHandle);
            }

			MainThread.CallStack.Push(new CallPoint(0, new EvalStack(lays.StackSize), mem, FastList<ElaValue[]>.Empty));
		}
		#endregion


        #region Types
        internal const int UNI = (Int32)ElaTypeCode.Unit;
		internal const int INT = (Int32)ElaTypeCode.Integer;
		internal const int REA = (Int32)ElaTypeCode.Single;
		internal const int BYT = (Int32)ElaTypeCode.Boolean;
		internal const int CHR = (Int32)ElaTypeCode.Char;
		internal const int LNG = (Int32)ElaTypeCode.Long;
		internal const int DBL = (Int32)ElaTypeCode.Double;
		internal const int STR = (Int32)ElaTypeCode.String;
		internal const int LST = (Int32)ElaTypeCode.List;
		internal const int TUP = (Int32)ElaTypeCode.Tuple;
		internal const int REC = (Int32)ElaTypeCode.Record;
		internal const int FUN = (Int32)ElaTypeCode.Function;
		internal const int OBJ = (Int32)ElaTypeCode.Object;
		internal const int MOD = (Int32)ElaTypeCode.Module;
		internal const int LAZ = (Int32)ElaTypeCode.Lazy;
		internal const int VAR = (Int32)ElaTypeCode.Variant;
		#endregion


		#region Public Methods
		public void Dispose()
		{
			var ex = default(Exception);

			foreach (var m in asm.EnumerateForeignModules())
			{
				try
				{
					m.Close();
				}
				catch (Exception e)
				{
					ex = e;
				}
			}

			if (ex != null)
				throw ex;
		}


		public ExecutionResult Run()
		{
			MainThread.Offset = MainThread.Offset == 0 ? 0 : MainThread.Offset;
			var ret = default(ElaValue);

			try
			{
				ret = Execute(MainThread);
			}
			catch (ElaException)
			{
				throw;
			}
            //catch (Exception ex)
            //{
            //    var op = MainThread.Module != null && MainThread.Offset > 0 &&
            //        MainThread.Offset - 1 < MainThread.Module.Ops.Count ?
            //        MainThread.Module.Ops[MainThread.Offset - 1].ToString() : String.Empty;
            //    throw Exception("CriticalError", ex, MainThread.Offset - 1, op);
            //}
			
			var evalStack = MainThread.CallStack[0].Stack;

			if (evalStack.Count > 0)
				throw Exception("StackCorrupted");

			return new ExecutionResult(ret);
		}


		public ExecutionResult Run(int offset)
		{
			MainThread.Offset = offset;
			return Run();
		}


		public void Recover()
		{
			if (modules.Length > 0)
			{
				var m = modules[0];

				for (var i = 0; i < m.Length; i++)
				{
					var v = m[i];

					if (v.Ref == null)
						m[i] = new ElaValue(ElaUnit.Instance);
				}
			}
		}


		public void RefreshState()
		{
			if (modules.Length > 0)
			{
				if (modules.Length != asm.ModuleCount)
				{
					var mods = new ElaValue[asm.ModuleCount][];

					for (var i = 0; i < modules.Length; i++)
						mods[i] = modules[i];

					modules = mods;
				}

				var mem = modules[0];
				var frame = asm.GetRootModule();
				var arr = new ElaValue[frame.Layouts[0].Size];
				Array.Copy(mem, 0, arr, 0, mem.Length);
				modules[0] = arr;
				MainThread.SwitchModule(0);
				MainThread.CallStack.Clear();
				var cp = new CallPoint(0, new EvalStack(frame.Layouts[0].StackSize), arr, FastList<ElaValue[]>.Empty);
				MainThread.CallStack.Push(cp);

				for (var i = 0; i < asm.ModuleCount; i++)
					ReadPervasives(MainThread, asm.GetModule(i), i);
			}
		}


		public ElaValue GetVariableByHandle(int moduleHandle, int varHandle)
		{
			var mod = default(ElaValue[]);

			try
			{
				mod = modules[moduleHandle];
			}
			catch (IndexOutOfRangeException)
			{
				throw Exception("InvalidModuleHandle");
			}

			try
			{
				return mod[varHandle];
			}
			catch (IndexOutOfRangeException)
			{
				throw Exception("InvalidVariableAddress");
			}
		}
		#endregion


		#region Execute
		private ElaValue Execute(WorkerThread thread)
		{
			var callStack = thread.CallStack;
			var evalStack = callStack.Peek().Stack;
			var frame = thread.Module;
			var ops = thread.Module.Ops.GetRawArray();
			var opData = thread.Module.OpData.GetRawArray();
			var locals = callStack.Peek().Locals;
			var captures = callStack.Peek().Captures;
			
			var ctx = thread.Context;
			var left = default(ElaValue);
			var right = default(ElaValue);
			var res = default(ElaValue);
			var i4 = 0;

		CYCLE:
			{
				#region Body
				var op = ops[thread.Offset];
				var opd = opData[thread.Offset];
				thread.Offset++;

				switch (op)
				{
					#region Stack Operations
					case Op.Tupex:
						{
                            right = evalStack.Pop();
                            var fv = right.Force(ctx);

                            if (ctx.Failed)
                            {
                                evalStack.Push(right);
                                ExecuteThrow(thread, evalStack);
                                goto SWITCH_MEM;
                            }

							var tup = fv.Ref as ElaTuple;

							if (tup == null || tup.Length != opd)
							{
								ExecuteFail(ElaRuntimeError.MatchFailed, thread, evalStack);
								goto SWITCH_MEM;
							}
							else
							{
								for (var i = 0; i < tup.Length; i++)
									locals[i] = tup.FastGet(i);
							}
						}
						break;
					case Op.Pushvar:
						i4 = opd & Byte.MaxValue;

						if (i4 == 0)
							evalStack.Push(locals[opd >> 8]);
						else
							evalStack.Push(captures[captures.Count - i4][opd >> 8]);
						break;
					case Op.Pushstr:
						evalStack.Push(new ElaValue(frame.Strings[opd]));
						break;
					case Op.Pushstr_0:
						evalStack.Push(new ElaValue(String.Empty));
						break;
					case Op.PushI4:
						evalStack.Push(opd);
						break;
					case Op.PushI4_0:
						evalStack.Push(0);
						break;
					case Op.PushI1_0:
						evalStack.Push(new ElaValue(false));
						break;
					case Op.PushI1_1:
						evalStack.Push(new ElaValue(true));
						break;
					case Op.PushR4:
						evalStack.Push(new ElaValue(opd, ElaSingle.Instance));
						break;
					case Op.PushCh:
						evalStack.Push(new ElaValue((Char)opd));
						break;
					case Op.Pushunit:
						evalStack.Push(new ElaValue(ElaUnit.Instance));
						break;
					case Op.Pushelem:
						right = evalStack.Pop();
						left = evalStack.Peek();
						evalStack.Replace(left.Ref.GetValue(right.Id(ctx), ctx)); //use of ElaValue.Id

						if (ctx.Failed)
						{
							evalStack.Replace(left);
							evalStack.Push(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Pushfld:
						{
							var fld = frame.Strings[opd];
							right = evalStack.PopFast();
							evalStack.Push(right.Ref.GetField(fld, ctx));

							if (ctx.Failed)
							{
								evalStack.Replace(right);
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}

							break;
						}
					case Op.Pop:
						evalStack.PopVoid();
						break;
					case Op.Popvar:
						right = evalStack.Pop();
						i4 = opd & Byte.MaxValue;

						if (i4 == 0)
							locals[opd >> 8] = right;
						else
							captures[captures.Count - i4][opd >> 8] = right;
						break;
					case Op.Popelem:
						{
							right = evalStack.Pop();
							left = evalStack.Pop();
							var val = evalStack.Pop();
							left.Ref.SetValue(right.Id(ctx), val, ctx); //Use of ElaValue.Id

							if (ctx.Failed)
							{
								evalStack.Push(val);
								evalStack.Push(left);
								evalStack.Push(right);
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}
						}
						break;
					case Op.Popfld:
						right = evalStack.Pop();
						left = evalStack.Pop();
						right.Ref.SetField(frame.Strings[opd], left, ctx);

						if (ctx.Failed)
						{
							evalStack.Push(left);
							evalStack.Push(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Dup:
						evalStack.Push(evalStack.Peek());
						break;
					case Op.Swap:
						right = evalStack.Pop();
						left = evalStack.Peek();
						evalStack.Replace(right);
						evalStack.Push(left);
						break;
					#endregion

					#region Binary Operations
					case Op.AndBw:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(left.Ref.BitwiseAnd(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.OrBw:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(left.Ref.BitwiseOr(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Xor:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(left.Ref.BitwiseXor(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Shl:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(left.Ref.ShiftLeft(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Shr:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(left.Ref.ShiftRight(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Concat:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(left.Concatenate(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Add:
						left = evalStack.Pop();
						right = evalStack.Peek();

                        if (left.TypeId == INT && right.TypeId == INT)
                        {
                            evalStack.Replace(left.I4 + right.I4);
                            break;
                        }

                        evalStack.Replace(left.Ref.Add(left, right, ctx));
                        
                        if (ctx.Failed)
                        {
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
						break;
					case Op.Sub:
						left = evalStack.Pop();
						right = evalStack.Peek();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							evalStack.Replace(left.I4 - right.I4);
							break;
						}

						evalStack.Replace(left.Ref.Subtract(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Div:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(left.Divide(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Mul:
						left = evalStack.Pop();
						right = evalStack.Peek();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							evalStack.Replace(left.I4 * right.I4);
							break;
						}

						evalStack.Replace(left.Ref.Multiply(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Pow:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(left.Ref.Power(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Rem:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(left.Ref.Remainder(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					#endregion

					#region Comparison Operations
                    case Op.Pat:
						right = evalStack.Peek();
						evalStack.Replace(new ElaValue(((Int32)right.Id(ctx).Ref.GetSupportedPatterns() & opd) == opd));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Cgt:
						left = evalStack.Pop();
						right = evalStack.Peek();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							evalStack.Replace(left.I4 > right.I4);
							break;
						}

						evalStack.Replace(left.Ref.Greater(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Clt:
						left = evalStack.Pop();
						right = evalStack.Peek();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							evalStack.Replace(left.I4 < right.I4);
							break;
						}

						evalStack.Replace(left.Ref.Lesser(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Ceq:
						left = evalStack.Pop();
						right = evalStack.Peek();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							evalStack.Replace(left.I4 == right.I4);
							break;
						}

						evalStack.Replace(left.Ref.Equal(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Cneq:
						left = evalStack.Pop();
						right = evalStack.Peek();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							evalStack.Replace(left.I4 != right.I4);
							break;
						}

						evalStack.Replace(left.Ref.NotEqual(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Cgteq:
						left = evalStack.Pop();
						right = evalStack.Peek();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							evalStack.Replace(left.I4 >= right.I4);
							break;
						}

						evalStack.Replace(left.Ref.GreaterEqual(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Clteq:
						left = evalStack.Pop();
						right = evalStack.Peek();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							evalStack.Replace(left.I4 <= right.I4);
							break;
						}

						evalStack.Replace(left.Ref.LesserEqual(left, right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					#endregion

					#region Object Operations
					case Op.Clone:
						right = evalStack.Peek();
						evalStack.Replace(right.Ref.Clone(ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Elem:
                        right = evalStack.Peek();
                        res = right.Ref.GetLength(ctx);

                        if (!ctx.Failed)
                        {
                            if (!res.Equal(res, new ElaValue(opd & Byte.MaxValue), ctx).Bool(ctx))
                            {
                                if (ctx.Failed)
                                {
                                    evalStack.Replace(right);
                                    ExecuteThrow(thread, evalStack);
                                }
                                else
                                    ExecuteFail(ElaRuntimeError.MatchFailed, thread, evalStack);
                                
                                goto SWITCH_MEM;
                            }

                            evalStack.Replace(right.Ref.GetValue(new ElaValue(opd >> 8), ctx));

                            if (!ctx.Failed)
                                break;
                        }
                        
                        evalStack.Replace(right);
						ExecuteThrow(thread, evalStack);
                        goto SWITCH_MEM;
					case Op.Reccons:
						{
							right = evalStack.Pop();
							left = evalStack.Pop();
							var mut = evalStack.Pop();
							var rec = evalStack.Peek();

							if (rec.TypeId == REC)
								((ElaRecord)rec.Ref).AddField(right.DirectGetString(), mut.I4 == 1, left);
							else
							{
								InvalidType(left, thread, evalStack, TypeCodeFormat.GetShortForm(ElaTypeCode.Record));
								goto SWITCH_MEM;
							}
						}
						break;
					case Op.Cons:
						left = evalStack.Pop();
						right = evalStack.Peek();
						evalStack.Replace(right.Ref.Cons(right.Ref, left, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Gen:
						right = evalStack.Pop();
						left = evalStack.Peek();
						evalStack.Replace(left.Ref.Generate(right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							evalStack.Push(left);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Genfin:
						right = evalStack.Peek();
						evalStack.Replace(right.Ref.GenerateFinalize(ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Tail:
						evalStack.Replace((right = evalStack.Peek()).Ref.Tail(ctx));
						
						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Head:
						evalStack.Replace((right = evalStack.Peek()).Ref.Head(ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Nil:
						evalStack.Replace((right = evalStack.Peek()).Nil(ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Isnil:
						evalStack.Replace(new ElaValue((right = evalStack.Peek()).Ref.IsNil(ctx)));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Len:
						evalStack.Replace((right = evalStack.Peek()).Ref.GetLength(ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Hasfld:
						evalStack.Replace((right = evalStack.Peek()).Ref.HasField(frame.Strings[opd], ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Force:
						right = evalStack.Peek();
						evalStack.Replace(right.Ref.Force(right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Untag:
						evalStack.Replace((right = evalStack.Peek()).Ref.Untag(ctx));

                        if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
                    case Op.Gettag:
                        evalStack.Replace(new ElaValue((right = evalStack.Peek()).Ref.GetTag(ctx)));

                        if (ctx.Failed)
                        {
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
					#endregion

					#region Unary Operations
					case Op.Not:
						right = evalStack.Peek();

						if (right.TypeId == BYT)
						{
							evalStack.Replace(right.I4 != 1);
							break;
						}

						evalStack.Replace(!right.Ref.Bool(right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Neg:
						right = evalStack.Peek();
						evalStack.Replace(right.Ref.Negate(right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.NotBw:
						right = evalStack.Peek();
						evalStack.Replace(right.Ref.BitwiseNot(right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
                    case Op.Conv:
                        {
                            left = evalStack.Pop();
                            right = evalStack.Pop();

                            if (left.TypeId == INT)
                            {
                                evalStack.Push(Convert(right, (ElaTypeCode)left.I4, ctx));

                                if (ctx.Failed)
                                    evalStack.PopVoid();
                            }
                            else
                            {
                                var fv = left.Force(ctx);

                                if (!ctx.Failed)
                                {
                                    evalStack.Push(right.Ref.Convert(right, (ElaTypeInfo)fv.Ref, ctx));

                                    if (ctx.Failed)
                                        evalStack.PopVoid();
                                }
                            }

                            if (ctx.Failed)
                            {
                                evalStack.Push(right);
                                evalStack.Push(left);
                                ExecuteThrow(thread, evalStack);
                                goto SWITCH_MEM;
                            }
                        }
                        break;
					#endregion

					#region Goto Operations
					case Op.Skiptl:
						right = evalStack.PopFast();

						if (right.Ref.IsNil(ctx))
						{
							if (ctx.Failed)
							{
								evalStack.Push(right);
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}

							break;
						}

						locals[opd] = right.Ref.Head(ctx);
						locals[opd + 1] = right.Ref.Tail(ctx);
						thread.Offset++;

						if (ctx.Failed)
						{
							evalStack.Push(right);
							thread.Offset--;
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Skiptn:
						{
							right = evalStack.PopFast();

							if (right.Ref.IsNil(ctx))
							{
								if (ctx.Failed)
								{
									evalStack.Push(right);
									ExecuteThrow(thread, evalStack);
									goto SWITCH_MEM;
								}

								break;
							}

							var obj = right.Ref.Tail(ctx);

							if (!obj.Ref.IsNil(ctx))
							{
								if (ctx.Failed)
								{
									ExecuteThrow(thread, evalStack);
									goto SWITCH_MEM;
								}

								break;
							}

							locals[opd] = right.Ref.Head(ctx);
							thread.Offset++;

							if (ctx.Failed)
							{
								evalStack.Push(right);
								thread.Offset--;
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}
							break;
						}
					case Op.Skiphtag:
						right = evalStack.Pop();

						if (!String.IsNullOrEmpty(right.Ref.GetTag(ctx)))
						{
							if (ctx.Failed)
							{
								evalStack.Push(right);
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}

							thread.Offset++;
							break;
						}

                        if (ctx.Failed)
                        {
                            evalStack.Push(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
						break;
					case Op.Skiptag:
						right = evalStack.Pop();

						if (frame.Strings[opd] == right.Ref.GetTag(ctx))
						{
							if (ctx.Failed)
							{
								evalStack.Push(right);
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}

							thread.Offset++;
							break;
						}

                        if (ctx.Failed)
                        {
                            evalStack.Push(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
						break;
					case Op.Brtrue:
						right = evalStack.Pop();

						if (right.TypeId == BYT)
						{
							if (right.I4 == 1)
								thread.Offset = opd;

							break;
						}

						if (right.Ref.Bool(right, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							evalStack.Push(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Brfalse:
						right = evalStack.Pop();

						if (right.TypeId == BYT)
						{
							if (right.I4 != 1)
								thread.Offset = opd;

							break;
						}

						if (!right.Ref.Bool(right, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							evalStack.Push(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Br:
						thread.Offset = opd;
						break;
					case Op.Br_eq:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							if (left.I4 == right.I4)
								thread.Offset = opd;

							break;
						}

						res = left.Ref.Equal(left, right, ctx);

						if (res.Ref.Bool(res, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							evalStack.Push(left);
							evalStack.Push(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Br_neq:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							if (left.I4 != right.I4)
								thread.Offset = opd;

							break;
						}

						res = left.Ref.NotEqual(left, right, ctx);

						if (res.Ref.Bool(res, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							evalStack.Push(left);
							evalStack.Push(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Br_lt:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							if (left.I4 < right.I4)
								thread.Offset = opd;

							break;
						}

						res = left.Ref.Lesser(res, right, ctx);

						if (res.Ref.Bool(res, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							evalStack.Push(left);
							evalStack.Push(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Br_gt:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.TypeId == INT && right.TypeId == INT)
						{
							if (left.I4 > right.I4)
								thread.Offset = opd;

							break;
						}

						res = left.Ref.Greater(left, right, ctx);

						if (res.Ref.Bool(res, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							evalStack.Push(left);
							evalStack.Push(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Brnil:
						right = evalStack.PopFast();

						if (right.IsNil(ctx))
							thread.Offset = opd;
						
						if (ctx.Failed)
						{
							evalStack.Push(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					#endregion

					#region CreateNew Operations
                    case Op.Newvar:
						right = evalStack.Peek();
						evalStack.Replace(new ElaValue(new ElaVariant(frame.Strings[opd], right)));
						break;
					case Op.Newlazy:
						evalStack.Push(new ElaValue(new ElaLazy((ElaFunction)evalStack.Pop().Ref, thread.Module)));
						break;
					case Op.Newfun:
						{
							var lst = new FastList<ElaValue[]>(captures);
							lst.Add(locals);
							var fun = new ElaFunction(opd, thread.ModuleHandle, evalStack.Pop().I4, lst, this);
							evalStack.Push(new ElaValue(fun));
						}
						break;
                    case Op.Newlist:
						evalStack.Push(new ElaValue(ElaList.Empty));
						break;
					case Op.Newrec:
						evalStack.Push(new ElaValue(new ElaRecord(opd)));
						break;
					case Op.Newtup:
						evalStack.Push(new ElaValue(new ElaTuple(opd)));
						break;
					case Op.Newtup_2:
						{
							right = evalStack.Pop();
							left = evalStack.Peek();
							var tup = default(ElaTuple);

							if (right.TypeId == TUP || left.TypeId == TUP)
							{
								tup = new ElaTuple(2);
								tup.InternalSetValue(left);
								tup.InternalSetValue(right);
							}
							else
							{
								tup = new ElaTuple(2);
								tup.InternalSetValue(0, left);
								tup.InternalSetValue(1, right);
							}

							evalStack.Replace(new ElaValue(tup));
						}
						break;
					case Op.NewI8:
						{
							right = evalStack.Pop();
							left = evalStack.Peek();
							var conv = new Conv();
							conv.I4_1 = left.I4;
							conv.I4_2 = right.I4;
							evalStack.Replace(new ElaValue(conv.I8));
						}
						break;
					case Op.NewR8:
						{
							right = evalStack.Pop();
							left = evalStack.Peek();
							var conv = new Conv();
							conv.I4_1 = left.I4;
							conv.I4_2 = right.I4;
							evalStack.Replace(new ElaValue(conv.R8));
						}
						break;
					case Op.Newmod:
						{
							var str = frame.Strings[opd];
							i4 = asm.GetModuleHandle(str);
							evalStack.Push(new ElaValue(new ElaModule(i4, this)));
						}
						break;
					#endregion

					#region Thunk Operations
					case Op.Flip:
						{
							right = evalStack.Peek().Id(ctx);

							if (right.TypeId != FUN)
							{
								evalStack.PopVoid();
								ExecuteFail(new ElaError(ElaRuntimeError.ExpectedFunction, TypeCodeFormat.GetShortForm(right.TypeCode)), thread, evalStack);
								goto SWITCH_MEM;
							}

							var fun = (ElaFunction)right.Ref;
							fun = fun.Captures != null ? fun.CloneFast() : fun.Clone();
							fun.Flip = !fun.Flip;
							evalStack.Replace(new ElaValue(fun));

							if (ctx.Failed)
							{
								evalStack.Push(right);
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}
						}
						break;
                    case Op.Call:
						if (Call(evalStack.Pop().Ref, thread, evalStack, CallFlag.None))
						    goto SWITCH_MEM;
						break;
					case Op.LazyCall:
						{
							var fun = ((ElaFunction)evalStack.Pop().Ref).CloneFast();
							fun.LastParameter = evalStack.Pop();
							evalStack.Push(new  ElaValue(new ElaLazy(fun, thread.Module)));
						}
						break;
					case Op.Callt:
						{
							if (callStack.Peek().Thunk != null || evalStack.Peek().TypeId == LAZ)
							{
                                if (Call(evalStack.Pop().Ref, thread, evalStack, CallFlag.None))
									goto SWITCH_MEM;
								break;
							}

                            var cp = callStack.Pop();

                            if (Call(evalStack.Pop().Ref, thread, evalStack, CallFlag.NoReturn))
                                goto SWITCH_MEM;
                            else
                                callStack.Push(cp);
						}
						break;
					case Op.Ret:
						{
							var cc = callStack.Pop();

							if (cc.Thunk != null)
							{
								cc.Thunk.Value = evalStack.Pop();
								thread.Offset = callStack.Peek().BreakAddress;
								goto SWITCH_MEM;
							}

							if (callStack.Count > 0)
							{
								var om = callStack.Peek();
								om.Stack.Push(evalStack.Pop());

								if (om.BreakAddress == 0)
									return default(ElaValue);
								else
								{
									thread.Offset = om.BreakAddress;
									goto SWITCH_MEM;
								}
							}
							else
								return evalStack.PopFast();
						}
					#endregion

					#region Builtins
                    case Op.Succ:
						right = evalStack.Peek();

						if (right.TypeId == INT)
						{
							evalStack.Replace(right.I4 + 1);
							break;
						}

						evalStack.Replace(right.Ref.Successor(right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Pred:
						right = evalStack.Peek();

						if (right.TypeId == INT)
						{
							evalStack.Replace(right.I4 - 1);
							break;
						}

						evalStack.Replace(right.Ref.Predecessor(right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Max:
						right = evalStack.Peek();
						evalStack.Replace(right.Ref.GetMax(right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Min:
						right = evalStack.Peek();
						evalStack.Replace(right.Ref.GetMin(right, ctx));

						if (ctx.Failed)
						{
							evalStack.Replace(right);
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					#endregion

					#region Misc
					case Op.Nop:
						break;
					case Op.Show:
						{
							left = evalStack.Pop();
							right = evalStack.Peek();

							if (left.TypeId != STR)
							{
                                InvalidType(left, thread, evalStack, TypeCodeFormat.GetShortForm(ElaTypeCode.String));
								goto SWITCH_MEM;
							}

                            evalStack.Replace(new ElaValue(right.Ref.Show(right, new ShowInfo(0, 0, left.DirectGetString()), ctx)));

							if (ctx.Failed)
							{
								evalStack.Replace(right);
								evalStack.Push(left);
								ExecuteFail(ctx.Error, thread, evalStack);
								goto SWITCH_MEM;
							}
						}
						break;
					case Op.Type:
						evalStack.Replace(new ElaValue(evalStack.Peek().Ref.GetTypeInfo()));
						break;
					case Op.Typeid:
						right = evalStack.Peek();

						if (right.TypeId == LAZ)
						{
							var la = (ElaLazy)right.Ref;

							if (la.Value.Ref != null)
							{
								evalStack.Replace(new ElaValue(la.Value.TypeId));
								break;
							}
						}

						evalStack.Replace(new ElaValue(right.TypeId));
						break;
					case Op.Throw:
						{
							left = evalStack.Pop();
							right = evalStack.Pop();
							var code = left.Show(ShowInfo.Default, ctx);
							var msg = right.Show(ShowInfo.Default, ctx);
						
							if (ctx.Failed)
							{
								evalStack.Push(right);
								evalStack.Push(left);
								ExecuteThrow(thread, evalStack);
							}
							else
								ExecuteFail(new ElaError(code, msg), thread, evalStack);
							
							goto SWITCH_MEM;
						}
					case Op.Rethrow:
						{
							var err = (ElaError)evalStack.Pop().Ref;
							ExecuteFail(err, thread, evalStack);
							goto SWITCH_MEM;
						}
					case Op.Failwith:
						{
							ExecuteFail((ElaRuntimeError)opd, thread, evalStack);
							goto SWITCH_MEM;
						}
					case Op.Stop:
						if (callStack.Count > 1)
						{
							callStack.Pop();
							var modMem = callStack.Peek();
							thread.Offset = modMem.BreakAddress;
							var om = thread.Module;
							var omh = thread.ModuleHandle;
							thread.SwitchModule(modMem.ModuleHandle);

                            if (!asm.RequireQuailified(omh))
							    ReadPervasives(thread, om, omh);
							
                            right = evalStack.Pop();

							if (modMem.BreakAddress == 0)
								return right;
							else
								goto SWITCH_MEM;
						}
						return evalStack.Count > 0 ? evalStack.Pop() : new ElaValue(ElaUnit.Instance);
					case Op.Runmod:
						{
							var mn = frame.Strings[opd];
							var hdl = asm.GetModuleHandle(mn);

							if (modules[hdl] == null)
							{
								var frm = asm.GetModule(hdl);

								if (frm is IntrinsicFrame)
								{
									modules[hdl] = ((IntrinsicFrame)frm).Memory;

                                    if (!asm.RequireQuailified(hdl))
									    ReadPervasives(thread, frm, hdl);
								}
								else
								{
									i4 = frm.Layouts[0].Size;
									var loc = new ElaValue[i4];
									modules[hdl] = loc;
									callStack.Peek().BreakAddress = thread.Offset;
									callStack.Push(new CallPoint(hdl, new EvalStack(frm.Layouts[0].StackSize), loc, FastList<ElaValue[]>.Empty));
									thread.SwitchModule(hdl);
									thread.Offset = 0;
                                    
                                    if (argMod != null)
                                        ReadPervasives(thread, argMod, argModHandle);

									goto SWITCH_MEM;
								}
							}
							else
							{
								var frm = asm.GetModule(hdl);

                                if (!asm.RequireQuailified(hdl))
                                    ReadPervasives(thread, frm, hdl);
							}
						}
						break;
					case Op.Start:
						callStack.Peek().CatchMark = opd;
						callStack.Peek().StackOffset = evalStack.Count;
						break;
					case Op.Leave:
						callStack.Peek().CatchMark = null;
						break;
					#endregion
				}
				#endregion
			}
			goto CYCLE;

		SWITCH_MEM:
			{
				var mem = callStack.Peek();
				thread.SwitchModule(mem.ModuleHandle);
				locals = mem.Locals;
				captures = mem.Captures;
				ops = thread.Module.Ops.GetRawArray();
				opData = thread.Module.OpData.GetRawArray();
				frame = thread.Module;
				evalStack = mem.Stack;
			}
			goto CYCLE;

		}
		#endregion


		#region Operations
        private ElaValue Convert(ElaValue value, ElaTypeCode typeCode, ExecutionContext ctx)
        {
            var typeInfo = default(ElaTypeInfo);

            switch (typeCode)
            {
                case ElaTypeCode.Boolean: 
                    typeInfo = ElaBoolean.TypeInfo;
                    break;
                case ElaTypeCode.Char:
                    typeInfo = ElaChar.TypeInfo;
                    break;
                case ElaTypeCode.Double:
                    typeInfo = ElaDouble.TypeInfo;
                    break;
                case ElaTypeCode.Function:
                    typeInfo = ElaFunction.TypeInfo;
                    break;
                case ElaTypeCode.Integer:
                    typeInfo = ElaInteger.TypeInfo;
                    break;
                case ElaTypeCode.Lazy:
                    typeInfo = ElaLazy.TypeInfo;
                    break;
                case ElaTypeCode.List:
                    typeInfo = ElaList.TypeInfo;
                    break;
                case ElaTypeCode.Long:
                    typeInfo = ElaLong.TypeInfo;
                    break;
                case ElaTypeCode.Module:
                    typeInfo = ElaModule.TypeInfo;
                    break;
                case ElaTypeCode.Record:
                    typeInfo = ElaRecord.TypeInfo;
                    break;
                case ElaTypeCode.Single:
                    typeInfo = ElaSingle.TypeInfo;
                    break;
                case ElaTypeCode.String:
                    typeInfo = ElaString.TypeInfo;
                    break;
                case ElaTypeCode.Tuple:
                    typeInfo = ElaTuple.TypeInfo;
                    break;
                case ElaTypeCode.Unit:
                    typeInfo = ElaUnit.TypeInfo;
                    break;
                case ElaTypeCode.Variant:
                    typeInfo = ElaVariant.TypeInfo;
                    break;
            }

            return value.Ref.Convert(value, typeInfo, ctx);
        }

		private void ReadPervasives(WorkerThread thread, CodeFrame frame, int handle)
		{
			var mod = thread.Module;
			var locals = modules[thread.ModuleHandle];
			var externs = frame.GlobalScope.Locals;
			var extMem = modules[handle];
			ScopeVar sv;

			foreach (var s in mod.LateBounds)
			{
				if (externs.TryGetValue(s.Name, out sv))
					locals[s.Address] = extMem[sv.Address];
			}
		}


		internal ElaValue CallPartial(ElaFunction fun, ElaValue arg)
		{
			if (fun.AppliedParameters < fun.Parameters.Length)
			{
				fun = fun.Clone();
				fun.Parameters[fun.AppliedParameters++] = arg;
				return new ElaValue(fun);
			}

			var t = MainThread.Clone();

			if (fun.ModuleHandle != t.ModuleHandle)
				t.SwitchModule(fun.ModuleHandle);

			var layout = t.Module.Layouts[fun.Handle];
			var stack = new EvalStack(layout.StackSize);

			stack.Push(arg);

			if (fun.AppliedParameters > 0)
				for (var i = 0; i < fun.AppliedParameters; i++)
					stack.Push(fun.Parameters[fun.AppliedParameters - i - 1]);

			var cp = new CallPoint(fun.ModuleHandle, stack, new ElaValue[layout.Size], fun.Captures);
			t.CallStack.Push(cp);
			t.Offset = layout.Address;
			return Execute(t);
		}


		internal ElaValue Call(ElaFunction fun, ElaValue[] args)
		{
			var t = MainThread.Clone();

			if (fun.ModuleHandle != t.ModuleHandle)
				t.SwitchModule(fun.ModuleHandle);

			var layout = t.Module.Layouts[fun.Handle];
			var stack = new EvalStack(layout.StackSize);
			var len = args.Length;

			if (fun.AppliedParameters > 0)
			{
				for (var i = 0; i < fun.AppliedParameters; i++)
					stack.Push(fun.Parameters[fun.AppliedParameters - i - 1]);

				len += fun.AppliedParameters;
			}

			for (var i = 0; i < len; i++)
				stack.Push(args[len - i - 1]);

			if (len != fun.Parameters.Length + 1)
			{
				InvalidParameterNumber(fun.Parameters.Length + 1, len, t, stack);
				return default(ElaValue);
			}

			var cp = new CallPoint(fun.ModuleHandle, stack, new ElaValue[layout.Size], fun.Captures);
			t.CallStack.Push(cp);
			t.Offset = layout.Address;
			return Execute(t);

		}
		

		private bool Call(ElaObject fun, WorkerThread thread, EvalStack stack, CallFlag cf)
		{
			if (fun.TypeId != FUN)
			{
				var arg = stack.Peek();
				var res = fun.Call(arg, thread.Context);
				stack.Replace(res);

				if (thread.Context.Failed)
				{
					stack.Replace(arg);
					stack.Push(new ElaValue(fun));
					ExecuteThrow(thread, stack);
					return true;
				}

				return false;
			}

			var natFun = (ElaFunction)fun;

			if (natFun.Captures != null)
			{
				if (natFun.AppliedParameters < natFun.Parameters.Length)
				{
					var newFun = natFun.CloneFast();
					newFun.Parameters[natFun.AppliedParameters] = stack.Peek();
					newFun.AppliedParameters++;
					stack.Replace(new ElaValue(newFun));
					return false;
				}
				
				if (natFun.AppliedParameters == natFun.Parameters.Length)
				{
					if (natFun.ModuleHandle != thread.ModuleHandle)
						thread.SwitchModule(natFun.ModuleHandle);

                    var p = cf != CallFlag.AllParams ? stack.PopFast() : natFun.LastParameter;
                        
					var mod = thread.Module;
					var layout = mod.Layouts[natFun.Handle];

					var newLoc = new ElaValue[layout.Size];
					var newStack = new EvalStack(layout.StackSize);
					var newMem = new CallPoint(natFun.ModuleHandle, newStack, newLoc, natFun.Captures);

					if (cf != CallFlag.NoReturn)
						thread.CallStack.Peek().BreakAddress = thread.Offset;

                    newStack.Push(p);

					for (var i = 0; i < natFun.Parameters.Length; i++)
						newStack.Push(natFun.Parameters[natFun.Parameters.Length - i - 1]);
					
					if (natFun.Flip)
					{
						var right = newStack.Pop();
						var left = newStack.Peek();
						newStack.Replace(right);
						newStack.Push(left);
					}

					thread.CallStack.Push(newMem);
					thread.Offset = layout.Address;
					return true;
				}
				
				InvalidParameterNumber(natFun.Parameters.Length + 1, natFun.Parameters.Length + 2, thread, stack);
				return true;
			}

			if (natFun.AppliedParameters == natFun.Parameters.Length)
			{
				CallExternal(thread, stack, natFun);
				return false;
			}
			else if (natFun.AppliedParameters < natFun.Parameters.Length)
			{
				var newFun = natFun.Clone();
				newFun.Parameters[newFun.AppliedParameters] = stack.Peek();
				newFun.AppliedParameters++;
				stack.Replace(new ElaValue(newFun));
				return true;
			}
			
			InvalidParameterNumber(natFun.Parameters.Length + 1, natFun.Parameters.Length + 2, thread, stack);		
			return true;
		}


		private void CallExternal(WorkerThread thread, EvalStack stack, ElaFunction funObj)
		{
			try
			{
				var arr = new ElaValue[funObj.Parameters.Length + 1];

				if (funObj.AppliedParameters > 0)
					Array.Copy(funObj.Parameters, arr, funObj.Parameters.Length);

				arr[funObj.Parameters.Length] = stack.Pop();

				if (funObj.Flip)
				{
					var x = arr[0];
					arr[0] = arr[1];
					arr[1] = x;
				}

				stack.Push(funObj.Call(arr));
			}
			catch (ElaRuntimeException ex)
			{
				ExecuteFail(new ElaError(ex.Category, ex.Message), thread, stack);
			}
			catch (Exception ex)
			{
				ExecuteFail(new ElaError(ElaRuntimeError.CallFailed, ex.Message), thread, stack);
			}
		}
		#endregion


		#region Exceptions
		private void ExecuteFail(ElaRuntimeError err, WorkerThread thread, EvalStack stack)
		{
			thread.Context.Fail(err);
			ExecuteThrow(thread, stack);
		}


		private void ExecuteFail(ElaError err, WorkerThread thread, EvalStack stack)
		{
			thread.Context.Fail(err);
			ExecuteThrow(thread, stack);
		}


		private void ExecuteThrow(WorkerThread thread, EvalStack stack)
		{
			if (thread.Context.Thunk != null)
			{
				thread.Context.Failed = false;
				var t = thread.Context.Thunk;
				thread.Context.Thunk = null;
				thread.Offset--;
                Call(t.Function, thread, stack, CallFlag.AllParams);
				thread.CallStack.Peek().Thunk = t;
				return;
			}

			var err = thread.Context.Error;
			thread.Context.Reset();
			var callStack = thread.CallStack;
			var cm = default(int?);
			var i = 0;

			if (callStack.Count > 0)
			{
				do
					cm = callStack[callStack.Count - (++i)].CatchMark;
				while (cm == null && i < callStack.Count);
			}

			if (cm == null)
				throw CreateException(Dump(err, thread));
			else
			{
				var c = 1;

				while (c++ < i)
					callStack.Pop();

				var curStack = callStack.Peek().Stack;
				curStack.Clear(callStack.Peek().StackOffset);
				curStack.Push(new ElaValue(Dump(err, thread)));
				thread.Offset = cm.Value;
			}
		}


		private ElaError Dump(ElaError err, WorkerThread thread)
		{
			if (err.Stack != null)
				return err;

			err.Module = thread.ModuleHandle;
			err.CodeOffset = thread.Offset;
			var st = new Stack<StackPoint>();

			for (var i = 0; i < thread.CallStack.Count; i++)
			{
				var cm = thread.CallStack[i];
				st.Push(new StackPoint(cm.BreakAddress, cm.ModuleHandle));
			}

			err.Stack = st;
			return err;
		}


		private ElaCodeException CreateException(ElaError err)
		{
			var deb = new ElaDebugger(asm);
			var mod = asm.GetModule(err.Module);
			var cs = deb.BuildCallStack(err.CodeOffset, mod, mod.File, err.Stack);
			return new ElaCodeException(err.FullMessage.Replace("\0",""), err.Code, cs.File, cs.Line, cs.Column, cs, err);
		}


		private ElaMachineException Exception(string message, Exception ex, params object[] args)
		{
			return new ElaMachineException(Strings.GetMessage(message, args), ex);
		}


		private ElaMachineException Exception(string message)
		{
			return Exception(message, null);
		}


		private void NoOperation(string op, ElaValue value, WorkerThread thread, EvalStack evalStack)
		{
			var str = value.Ref.Show(value, new ShowInfo(10, 10), thread.Context);

			if (str.Length > 40)
				str = str.Substring(0, 40) + "...";

			ExecuteFail(new ElaError(ElaRuntimeError.InvalidOp, str, value.GetTypeName(), op), thread, evalStack);
		}


		private void InvalidType(ElaValue val, WorkerThread thread, EvalStack evalStack, string type)
		{
			ExecuteFail(new ElaError(ElaRuntimeError.InvalidType, type, val.GetTypeName()), thread, evalStack);
		}


		private void InvalidParameterNumber(int pars, int passed, WorkerThread thread, EvalStack evalStack)
		{
			if (passed == 0)
				ExecuteFail(new ElaError(ElaRuntimeError.CallWithNoParams, pars), thread, evalStack);
			else if (passed > pars)
				ExecuteFail(new ElaError(ElaRuntimeError.TooManyParams), thread, evalStack);
			else if (passed < pars)
				ExecuteFail(new ElaError(ElaRuntimeError.TooFewParams), thread, evalStack);
		}


		private void ConversionFailed(ElaValue val, int target, string err, WorkerThread thread, EvalStack evalStack)
		{
			ExecuteFail(new ElaError(ElaRuntimeError.ConversionFailed, val.GetTypeName(),
				TypeCodeFormat.GetShortForm((ElaTypeCode)target), err), thread, evalStack);
		}
		#endregion


		#region Properties
		public CodeAssembly Assembly { get { return asm; } }

		internal WorkerThread MainThread { get; private set; }
		#endregion
	}
}