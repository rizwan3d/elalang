using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Ela.CodeModel;
using Ela.Compilation;
using Ela.Debug;
using Ela.Linking;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
	public sealed class ElaMachine
	{
		#region Construction
		private ElaValue[][] modules;
		private Pervasive[][] pervasives;
		private CodeAssembly asm;
		private object syncRoot = new Object();

		public ElaMachine(CodeAssembly asm)
		{
			this.asm = asm;
			var frame = asm.GetRootModule();
			MainThread = new WorkerThread(asm);
			var lays = frame.Layouts[0];
			modules = new ElaValue[asm.ModuleCount][];
			pervasives = new Pervasive[asm.ModuleCount][];
			pervasives[0] = new Pervasive[frame.ReferencedPervasives.Count];
			var mem = new ElaValue[lays.Size];
			modules[0] = mem;
			MainThread.CallStack.Push(new CallPoint(0, new EvalStack(lays.StackSize), mem, FastList<ElaValue[]>.Empty));
		}
		#endregion


		#region Types
		internal const int UNI = (Int32)ObjectType.Unit;
		internal const int INT = (Int32)ObjectType.Integer;
		internal const int REA = (Int32)ObjectType.Single;
		internal const int BYT = (Int32)ObjectType.Boolean;
		internal const int CHR = (Int32)ObjectType.Char;
		internal const int LNG = (Int32)ObjectType.Long;
		internal const int DBL = (Int32)ObjectType.Double;
		internal const int STR = (Int32)ObjectType.String;
		internal const int LST = (Int32)ObjectType.List;
		internal const int ARR = (Int32)ObjectType.Array;
		internal const int TUP = (Int32)ObjectType.Tuple;
		internal const int REC = (Int32)ObjectType.Record;
		internal const int FUN = (Int32)ObjectType.Function;
		internal const int OBJ = (Int32)ObjectType.Object;
		internal const int MOD = (Int32)ObjectType.Module;
		internal const int LAZ = (Int32)ObjectType.Lazy;
		internal const int VAR = (Int32)ObjectType.Variant;
		#endregion


		#region Public Methods
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
			catch (Exception ex)
			{
				var op = MainThread.Module != null && MainThread.Offset > 0 &&
					MainThread.Offset - 1 < MainThread.Module.Ops.Count ?
					MainThread.Module.Ops[MainThread.Offset - 1].ToString() : String.Empty;

				throw Exception("CriticalError", ex, MainThread.Offset - 1, op);
			}

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

				if (pervasives.Length != asm.ModuleCount)
				{
					var perv = new Pervasive[asm.ModuleCount][];

					for (var i = 0; i < pervasives.Length; i++)
						perv[i] = pervasives[i];

					pervasives = perv;
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

				pervasives[0] = new Pervasive[frame.ReferencedPervasives.Count];

				for (var i = 0; i < asm.ModuleCount; i++)
					ReadPervasives(MainThread, asm.GetModule(i), i);
			}
		}


		public void ReloadModule(ModuleReference mod, CodeFrame frame)
		{
			var hdl = asm.GetModuleHandle(mod.ToString());
			var size = frame.Layouts[0].Size;
			var loc = new ElaValue[size];
			modules[hdl] = loc;
			asm.RefreshModule(hdl, frame);
			MainThread.CallStack.Peek().BreakAddress = 0;
			MainThread.CallStack.Push(new CallPoint(hdl, new EvalStack(frame.Layouts[0].StackSize), loc, FastList<ElaValue[]>.Empty));
			MainThread.SwitchModule(hdl);
			MainThread.Offset = 0;
			Execute(MainThread);
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
			var currentPervs = pervasives[thread.ModuleHandle];

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
							var tup = evalStack.Pop().Ref as ElaTuple;

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
						left = evalStack.PopFast();
						evalStack.Push(left.Id(ctx).Ref.GetValue(right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.PushelemI4:
						right = evalStack.Peek();
						evalStack.Replace(right.Id(ctx).Ref.GetValue(new ElaValue(opd), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Pushelemi:
						right = evalStack.PopFast();
						i4 = opd & Byte.MaxValue;
						evalStack.Push(right.Id(ctx).Ref.GetValue(i4 == 0 ? locals[opd >> 8] :
							captures[captures.Count - i4][opd >> 8], ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Pushfld:
						{
							var fld = frame.Strings[opd];
							right = evalStack.PopFast().Id(ctx);

							if (right.Type == MOD)
							{
								i4 = ((ElaModule)right.Ref).Handle;
								var fr = asm.GetModule(i4);
								ScopeVar sc;

								if (!fr.GlobalScope.Locals.TryGetValue(fld, out sc))
								{
									ExecuteFail(new ElaError(ElaRuntimeError.UndefinedVariable, fld, asm.GetModuleName(i4)), thread, evalStack);
									goto SWITCH_MEM;
								}

								if ((sc.Flags & ElaVariableFlags.Private) == ElaVariableFlags.Private)
								{
									ExecuteFail(new ElaError(ElaRuntimeError.PrivateVariable, fld), thread, evalStack);
									goto SWITCH_MEM;
								}

								evalStack.Push(modules[i4][sc.Address]);
								break;
							}

							evalStack.Push(right.Ref.GetField(fld, ctx));

							if (ctx.Failed)
							{
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
						right = evalStack.Pop();
						evalStack.Pop().Id(ctx).Ref.SetValue(right.Id(ctx), evalStack.Pop(), ctx);

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Popelemi:
						i4 = opd & Byte.MaxValue;
						evalStack.Pop().Id(ctx).Ref.SetValue(i4 == 0 ?
							locals[opd >> 8] : captures[captures.Count - i4][opd >> 8],
							evalStack.Pop(), ctx);

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Popfld:
						evalStack.Pop().Id(ctx).Ref.SetField(frame.Strings[opd], evalStack.Pop(), ctx);

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Dup:
						right = evalStack.Peek();
						evalStack.Push(right);
						break;
					case Op.Swap:
						right = evalStack.Pop();
						left = evalStack.Peek();
						evalStack.Replace(right);
						evalStack.Push(left);
						break;
					case Op.Pusharg:
						{
							var name = frame.Strings[opd];

							if (asm.TryGetArgument(name, out right))
								evalStack.Push(right);
							else
							{
								UndefinedArgument(name, thread, evalStack);
								goto SWITCH_MEM;
							}
						}
						break;
					case Op.Pushperv:
						{
							if (currentPervs == null)
							{
								UnknownName(thread, evalStack);
								goto SWITCH_MEM;
							}

							var perv = currentPervs[opd];

							if (perv.Module == 0)
							{
								UnknownName(thread, evalStack);
								goto SWITCH_MEM;
							}
							else
								evalStack.Push(modules[perv.Module][perv.Address >> 8]);
						}
						break;
					#endregion

					#region Binary Operations
					case Op.AndBw:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 & right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.BitwiseAnd(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.OrBw:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 | right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.BitwiseOr(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Xor:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 ^ right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.BitwiseXor(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Shl:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 << right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.ShiftLeft(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Shr:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 >> right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.ShiftRight(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Concat:
						right = evalStack.Pop();
						left = evalStack.Peek().Id(ctx);
						evalStack.Replace(left.Ref.Concatenate(left, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Add:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 + right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.Add(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Sub:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 - right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.Subtract(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Div:
						right = evalStack.Pop();
						left = evalStack.Peek().Id(ctx);
						evalStack.Replace(left.Ref.Divide(left, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Mul:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 * right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.Multiply(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Pow:
						right = evalStack.Pop();
						left = evalStack.Peek().Id(ctx);
						evalStack.Replace(left.Ref.Power(left, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Rem:
						right = evalStack.Pop();
						left = evalStack.Peek().Id(ctx);
						evalStack.Replace(left.Ref.Modulus(left, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					#endregion

					#region Comparison Operations
					case Op.Trait:
						evalStack.Replace(new ElaValue(((Int32)evalStack.Peek().Id(ctx).Ref.Traits & opd) == opd));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Cgt:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 > right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.Greater(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Clt:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 < right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.Lesser(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Ceq:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 == right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.Equals(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Cneq:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 != right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.NotEquals(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Cgteq:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 >= right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.GreaterEquals(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Clteq:
						right = evalStack.Pop();
						left = evalStack.Peek();

						if (left.Type == INT && right.Type == INT)
						{
							evalStack.Replace(left.I4 <= right.I4);
							break;
						}

						res = left.Id(ctx);
						evalStack.Replace(res.Ref.LesserEquals(res, right.Id(ctx), ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					#endregion

					#region Object Operations
					case Op.Reccons:
						{
							right = evalStack.Pop();
							left = evalStack.Pop();
							var mut = evalStack.Pop();
							var rec = evalStack.Peek();

							if (rec.Type == REC)
								((ElaRecord)rec.Ref).AddField(right.AsString(), mut.I4 == 1, left);
							else
							{
								InvalidType(left, thread, evalStack, REC);
								goto SWITCH_MEM;
							}
						}
						break;
					case Op.Cons:
						right = evalStack.Pop();
						evalStack.Replace(right.Ref.Cons(right.Ref, evalStack.Peek(), ctx));

						if (ctx.Failed)
						{
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
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Genfin:
						right = evalStack.Peek();
						evalStack.Replace(right.Ref.GenerateFinalize(ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Tail:
						right = evalStack.Pop().Id(ctx);

						if (!right.Ref.IsNil(ctx))
							evalStack.Push(right.Ref.Tail(ctx));
						else
							evalStack.Push(right);

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Head:
						evalStack.Replace(evalStack.Peek().Id(ctx).Ref.Head(ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Isnil:
						evalStack.Replace(new ElaValue(evalStack.Peek().Id(ctx).Ref.IsNil(ctx)));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Len:
						evalStack.Replace(evalStack.Peek().Id(ctx).Ref.GetLength(ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Hasfld:
						evalStack.Replace(evalStack.Peek().Id(ctx).Ref.HasField(frame.Strings[opd], ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Force:
						right = evalStack.Peek();

						if ((right.Ref.Traits & ElaTraits.Thunk) != ElaTraits.Thunk)
						{
							evalStack.PopVoid();
							NoTrait(ElaRuntimeError.TraitThunk, right, thread, evalStack);
							goto SWITCH_MEM;
						}

						evalStack.Replace(right.Id(ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Tagval:
						evalStack.Replace(((ElaVariant)evalStack.Peek().Ref).Value);
						break;
					#endregion

					#region Unary Operations
					case Op.Incr:
						right = locals[opd >> 8];

						if (right.Type == INT)
						{
							right = new ElaValue(right.I4 + 1);
							evalStack.Push(right);
							locals[opd >> 8] = right;
							break;
						}

						Increment(opd, right.Id(ctx), thread, evalStack);
						goto SWITCH_MEM;
					case Op.Decr:
						right = locals[opd >> 8];

						if (right.Type == INT)
						{
							right = new ElaValue(right.I4 - 1);
							evalStack.Push(right);
							locals[opd >> 8] = right;
							break;
						}

						Decrement(opd, right.Id(ctx), thread, evalStack);
						goto SWITCH_MEM;
					case Op.Not:
						right = evalStack.Peek();

						if (right.Type == BYT)
						{
							evalStack.Replace(right.I4 != 1);
							break;
						}

						right = right.Id(ctx);
						evalStack.Replace(right.Ref.Bool(right, ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Neg:
						right = evalStack.Peek();

						if (right.Type == INT)
						{
							evalStack.Replace(new ElaValue(-right.I4));
							break;
						}

						right = right.Id(ctx);
						evalStack.Replace(right.Ref.Negate(right, ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.NotBw:
						right = evalStack.Peek();

						if (right.Type == INT)
						{
							evalStack.Replace(new ElaValue(~right.I4));
							break;
						}

						right = right.Id(ctx);
						evalStack.Replace(right.Ref.BitwiseNot(right, ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Conv:
						right = evalStack.Peek().Id(ctx);
						evalStack.Replace(right.Ref.Convert(right, (ObjectType)opd, ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					#endregion

					#region Goto Operations
					case Op.Skiptl:
						right = evalStack.PopFast().Id(ctx);

						if (right.Ref.IsNil(ctx))
						{
							if (ctx.Failed)
							{
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
									ExecuteThrow(thread, evalStack);
									goto SWITCH_MEM;
								}

								break;
							}

							var obj = right.Ref.Tail(ctx).Id(ctx);

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
								thread.Offset--;
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}
							break;
						}
					case Op.Skiphtag:
						right = evalStack.Pop().Id(ctx);

						if (!String.IsNullOrEmpty(right.Ref.GetTag()))
						{
							if (ctx.Failed)
							{
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}

							thread.Offset++;
							break;
						}

						break;
					case Op.Skiptag:
						right = evalStack.Pop().Id(ctx);

						if (frame.Strings[opd] == right.Ref.GetTag())
						{
							if (ctx.Failed)
							{
								ExecuteThrow(thread, evalStack);
								goto SWITCH_MEM;
							}

							thread.Offset++;
							break;
						}

						break;
					case Op.Brtrue:
						right = evalStack.Pop();

						if (right.Type == BYT)
						{
							if (right.I4 == 1)
								thread.Offset = opd;

							break;
						}

						right = right.Id(ctx);

						if (right.Ref.Bool(right, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Brfalse:
						right = evalStack.Pop();

						if (right.Type == BYT)
						{
							if (right.I4 != 1)
								thread.Offset = opd;

							break;
						}

						right = right.Id(ctx);

						if (!right.Ref.Bool(right, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
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

						if (left.Type == INT && right.Type == INT)
						{
							if (left.I4 == right.I4)
								thread.Offset = opd;

							break;
						}

						left = left.Id(ctx);
						left = left.Ref.Equals(left, right.Id(ctx), ctx);

						if (left.Ref.Bool(left, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Br_neq:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.Type == INT && right.Type == INT)
						{
							if (left.I4 != right.I4)
								thread.Offset = opd;

							break;
						}

						left = left.Id(ctx);
						left = left.Ref.NotEquals(left, right.Id(ctx), ctx);

						if (left.Ref.Bool(left, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Br_lt:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.Type == INT && right.Type == INT)
						{
							if (left.I4 < right.I4)
								thread.Offset = opd;

							break;
						}

						left = left.Id(ctx);
						left = left.Ref.Lesser(left, right.Id(ctx), ctx);

						if (left.Ref.Bool(left, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Br_gt:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.Type == INT && right.Type == INT)
						{
							if (left.I4 > right.I4)
								thread.Offset = opd;

							break;
						}

						left = left.Id(ctx);
						left = left.Ref.Greater(left, right.Id(ctx), ctx);

						if (left.Ref.Bool(left, ctx) && !ctx.Failed)
						{
							thread.Offset = opd;
							break;
						}

						if (ctx.Failed)
						{
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
						evalStack.Push(new ElaValue(new ElaLazy((ElaFunction)evalStack.Pop().Ref)));
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
						evalStack.Push(new ElaValue(ElaList.Nil));
						break;
					case Op.Newrec:
						evalStack.Push(new ElaValue(new ElaRecord(opd)));
						break;
					case Op.Newarr:
						evalStack.Push(new ElaValue(new ElaArray(4)));
						break;
					case Op.Newtup:
						evalStack.Push(new ElaValue(new ElaTuple(opd)));
						break;
					case Op.Newtup_2:
						{
							right = evalStack.Pop();
							left = evalStack.Peek();
							var tup = default(ElaTuple);

							if (right.Type == TUP || left.Type == TUP)
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

					#region Function Operations
					case Op.Flip:
						{
							right = evalStack.Peek();

							if (right.Type != FUN)
							{
								evalStack.PopVoid();
								ExecuteFail(new ElaError(ElaRuntimeError.ExpectedFunction, right.DataType.GetShortForm()), thread, evalStack);
								goto SWITCH_MEM;
							}

							var fun = (ElaFunction)right.Ref;
							fun = fun.CloneFast();
							fun.Flip = !fun.Flip;
							evalStack.Replace(new ElaValue(fun));
						}
						break;
					case Op.Call:
						if (Call(evalStack.Pop().Id(ctx).Ref, thread, evalStack, null))
							goto SWITCH_MEM;
						break;
					case Op.LazyCall:
						{
							var fun = ((ElaFunction)evalStack.Pop().Ref).CloneFast();
							fun.LastParameter = evalStack.Pop();
							evalStack.Push(new ElaValue(new ElaLazy(fun)));
						}
						break;
					case Op.Callt:
						{
							var cp = callStack.Pop();

							if (Call(evalStack.Pop().Id(ctx).Ref, thread, evalStack, cp))
								goto SWITCH_MEM;
							else
								callStack.Push(cp);
						}
						break;
					case Op.Ret:
						{
							var nm = callStack.Pop();

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

						if (right.Type == INT)
						{
							evalStack.Replace(right.I4 + 1);
							break;
						}

						right = right.Id(ctx);
						evalStack.Replace(right.Ref.Successor(right, ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Pred:
						right = evalStack.Peek();

						if (right.Type == INT)
						{
							evalStack.Replace(right.I4 - 1);
							break;
						}

						right = right.Id(ctx);
						evalStack.Replace(right.Ref.Predecessor(right, ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Max:
						evalStack.Replace(evalStack.Peek().Id(ctx).Ref.GetMax(ctx));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						break;
					case Op.Min:
						evalStack.Replace(evalStack.Peek().Id(ctx).Ref.GetMin(ctx));

						if (ctx.Failed)
						{
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
							left = evalStack.Pop().Id(ctx);
							right = evalStack.Peek().Id(ctx);

							if (left.Type != STR)
							{
								InvalidType(left, thread, evalStack, STR);
								goto SWITCH_MEM;
							}

							evalStack.Replace(new ElaValue(right.Ref.Show(right, ctx, new ShowInfo(0, 0, left.AsString()))));

							if (ctx.Failed)
							{
								ExecuteFail(ctx.Error, thread, evalStack);
								goto SWITCH_MEM;
							}
						}
						break;
					case Op.Cout:
						right = evalStack.Pop().Id(ctx);
						Console.WriteLine(right.Ref.Show(right, ctx, ShowInfo.Debug));

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}
						break;
					case Op.Isfun:
						right = evalStack.Peek().Id(ctx);

						if (ctx.Failed)
						{
							ExecuteThrow(thread, evalStack);
							goto SWITCH_MEM;
						}

						evalStack.Replace(new ElaValue(right.Type == FUN &&
							((ElaFunction)right.Ref).Parameters.Length + 1 == opd));
						break;
					case Op.Type:
						evalStack.Replace(new ElaValue(evalStack.Peek().Ref.GetTypeInfo()));
						break;
					case Op.Typeid:
						right = evalStack.Peek();

						if (right.Type == LAZ)
						{
							var la = (ElaLazy)right.Ref;

							if (la.Value.Ref != null)
							{
								evalStack.Replace(new ElaValue(la.Value.Type));
								break;
							}
						}

						evalStack.Replace(new ElaValue(right.Type));
						break;
					case Op.Throw:
						{
							var code = evalStack.Pop().ToString();
							var msg = evalStack.Pop().ToString();
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
									modules[hdl] = ((IntrinsicFrame)frm).Memory;
								else
								{
									i4 = frm.Layouts[0].Size;
									var loc = new ElaValue[i4];
									modules[hdl] = loc;
									callStack.Peek().BreakAddress = thread.Offset;
									callStack.Push(new CallPoint(hdl, new EvalStack(frm.Layouts[0].StackSize), loc, FastList<ElaValue[]>.Empty));
									thread.SwitchModule(hdl);
									thread.Offset = 0;
									goto SWITCH_MEM;
								}
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
				currentPervs = pervasives[thread.ModuleHandle];
				evalStack = mem.Stack;
			}
			goto CYCLE;

		}
		#endregion


		#region Operations
		private void Increment(int addr, ElaValue val, WorkerThread thread, EvalStack stack)
		{
			var nval = val.Ref.Successor(val, thread.Context);
			stack.Push(nval);
			thread.CallStack.Peek().Locals[addr >> 8] = nval;

			if (thread.Context.Failed)
				ExecuteThrow(thread, stack);
		}


		private void Decrement(int addr, ElaValue val, WorkerThread thread, EvalStack stack)
		{
			var nval = val.Ref.Predecessor(val, thread.Context);
			stack.Push(nval);
			thread.CallStack.Peek().Locals[addr >> 8] = nval;

			if (thread.Context.Failed)
				ExecuteThrow(thread, stack);
		}


		private void ReadPervasives(WorkerThread thread, CodeFrame frame, int handle)
		{
			var refs = thread.Module.ReferencedPervasives;
			var pervs = pervasives[thread.ModuleHandle];
			var hdl = 0;

			foreach (var kv in frame.DeclaredPervasives)
			{
				if (refs.TryGetValue(kv.Key, out hdl))
					pervs[hdl] = new Pervasive(handle, kv.Value);
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
				stack.Push(args[i]);

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


		private bool Call(ElaObject fun, WorkerThread thread, EvalStack stack, CallPoint cp)
		{
			if (fun.TypeId != FUN)
			{
				if ((fun.Traits & ElaTraits.Call) == ElaTraits.Call)
				{
					var res = fun.Call(stack.Pop(), thread.Context);
					stack.Push(res);

					if (thread.Context.Failed)
					{
						ExecuteThrow(thread, stack);
						return true;
					}

					return false;
				}
				else
					ExecuteFail(new ElaError(ElaRuntimeError.ExpectedFunction, ((ObjectType)fun.TypeId).GetShortForm()), thread, stack);

				return true;
			}

			var pop = true;
			var natFun = (ElaFunction)fun;

			if (natFun.Captures != null)
			{
				if (natFun.AppliedParameters == natFun.Parameters.Length)
				{
					if (natFun.ModuleHandle != thread.ModuleHandle)
						thread.SwitchModule(natFun.ModuleHandle);

					var mod = thread.Module;
					var layout = mod.Layouts[natFun.Handle];

					var newLoc = new ElaValue[layout.Size];
					var newStack = new EvalStack(layout.StackSize);
					var newMem = new CallPoint(natFun.ModuleHandle, newStack, newLoc, natFun.Captures);

					if (cp == null)
						thread.CallStack.Peek().BreakAddress = thread.Offset;

					newStack.Push(stack.Pop());

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
				}
				else if (natFun.AppliedParameters < natFun.Parameters.Length)
				{
					var newFun = natFun.CloneFast();
					newFun.Parameters[newFun.AppliedParameters] = stack.Pop();
					newFun.AppliedParameters++;
					stack.Push(new ElaValue(newFun));
					pop = false;
				}
				else
					InvalidParameterNumber(natFun.Parameters.Length + 1, natFun.Parameters.Length + 2, thread, stack);
			}
			else
			{
				pop = false;

				if (natFun.AppliedParameters == natFun.Parameters.Length)
					CallExternal(thread, stack, natFun);
				else if (natFun.AppliedParameters < natFun.Parameters.Length)
				{
					var newFun = natFun.Clone();
					newFun.Parameters[newFun.AppliedParameters] = stack.Pop();
					newFun.AppliedParameters++;
					stack.Push(new ElaValue(newFun));
				}
				else
					InvalidParameterNumber(natFun.Parameters.Length + 1, natFun.Parameters.Length + 2, thread, stack);
			}

			return pop;
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
			var cs = deb.BuildCallStack(err.CodeOffset, mod.Symbols, mod.File, err.Stack);
			return new ElaCodeException(err.FullMessage, err.Code, cs.File, cs.Line, cs.Column, cs, err);
		}


		private ElaMachineException Exception(string message, Exception ex, params object[] args)
		{
			return new ElaMachineException(Strings.GetMessage(message, args), ex);
		}


		private ElaMachineException Exception(string message)
		{
			return Exception(message, null);
		}


		private string FormatTypes(params int[] types)
		{
			var sb = new StringBuilder();

			for (var i = 0; i < types.Length; i++)
			{
				if (i > 0)
					sb.Append(',');

				sb.Append(((ObjectType)types[i]).GetShortForm());
			}

			return sb.ToString();
		}


		private void NoTrait(ElaRuntimeError code, ElaValue value, WorkerThread thread, EvalStack evalStack)
		{
			var str = value.Ref.Show(value, thread.Context, new ShowInfo(10, 10));

			if (str.Length > 40)
				str = str.Substring(0, 40) + "...";

			ExecuteFail(new ElaError(code, str, value.DataType.GetShortForm()), thread, evalStack);
		}


		private void InvalidType(ElaValue val, WorkerThread thread, EvalStack evalStack, params int[] exps)
		{
			ExecuteFail(new ElaError(ElaRuntimeError.InvalidType, FormatTypes(exps), FormatTypes(val.Type)), thread, evalStack);
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
			ExecuteFail(new ElaError(ElaRuntimeError.ConversionFailed, FormatTypes(val.Type),
				FormatTypes(target), err), thread, evalStack);
		}


		private void UnknownName(WorkerThread thread, EvalStack evalStack)
		{
			ExecuteFail(new ElaError(ElaRuntimeError.UnknownPervasive), thread, evalStack);
		}


		private void UndefinedArgument(string name, WorkerThread thread, EvalStack evalStack)
		{
			ExecuteFail(new ElaError(ElaRuntimeError.UndefinedArgument, name), thread, evalStack);
		}
		#endregion


		#region Properties
		public CodeAssembly Assembly { get { return asm; } }

		internal WorkerThread MainThread { get; private set; }
		#endregion
	}
}