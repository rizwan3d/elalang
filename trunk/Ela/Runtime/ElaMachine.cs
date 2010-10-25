using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Ela.Compilation;
using Ela.Debug;
using Ela.Linking;
using Ela.Runtime.ObjectModel;
using Ela.CodeModel;
using System.Threading;

namespace Ela.Runtime
{
	public sealed class ElaMachine
	{
		#region Construction
		private RuntimeValue[][] modules;
		private FastStack<Dictionary<String, Pervasive>> pervasives;
		private CodeAssembly asm;
		private object syncRoot = new Object();

		public ElaMachine(CodeFrame frame) : this(new CodeAssembly(frame))
		{

		}

		public ElaMachine(CodeAssembly asm)
		{
			this.asm = asm;
			Threads = new FastList<WorkerThread>();
			var frame = asm.GetRootModule();
			MainThread = new WorkerThread(asm);
			var lays = frame.Layouts[0];
			modules = new RuntimeValue[asm.ModuleCount][];
			var mem = new RuntimeValue[lays.Size];
			modules[0] = mem;
			pervasives = new FastStack<Dictionary<String, Pervasive>>();
			pervasives.Push(new Dictionary<String, Pervasive>());
			ReadPervasives(frame, 0);
			MainThread.CallStack.Push(
				new CallPoint(WorkerThread.EndAddress, 0, 0, null, mem,
					FastList<RuntimeValue[]>.Empty));
		}
		#endregion


		#region Affinity
		internal const int PTR = -100;

		internal const int ___ = (Int32)ObjectType.None;
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
		internal const int LAZ = (Int32)ObjectType.Lazy;
		internal const int SEQ = (Int32)ObjectType.Sequence;
		internal const int MOD = (Int32)ObjectType.Module;
		
		private static int[,] opAffinity = 
		{
			//ERR  UNI  INT  REA  BYT  CHR  LNG  DBL  STR  LST  ARR  TUP  REC  FUN  OBJ  LAZ  ENU  MOD
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //ERR
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //UNI
			{ ___, ___, INT, REA, ___, ___, LNG, DBL, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //INT
			{ ___, ___, REA, REA, ___, ___, ___, DBL, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //REA
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //BYT
			{ ___, ___, ___, ___, ___, CHR, ___, ___, STR, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //CHR
			{ ___, ___, LNG, ___, ___, ___, LNG, DBL, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //LNG
			{ ___, ___, DBL, DBL, ___, ___, DBL, DBL, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //DBL
			{ ___, ___, ___, ___, ___, STR, ___, ___, STR, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //STR
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, LST, ___, ___, ___, ___, ___, ___, ___, ___ }, //LST
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ARR, ___, ___, ___, ___, ___, ___, ___ }, //ARR
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //TUP
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //REC
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //UNI
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //PRD
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //FUN
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //OBJ
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //LAZ
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //ENU
			{ ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___ }, //MOD
		};
		#endregion


		#region Public Methods
		public ExecutionResult Run()
		{
			MainThread.Offset = MainThread.Offset == 0 ? 0 : MainThread.Offset;
			Execute(MainThread);

			if (MainThread.EvalStack.Count > 1)
				throw new ElaFatalException(Strings.GetMessage("StackCorrupted"));

			return new ExecutionResult(
				MainThread.EvalStack.Count > 0 ? MainThread.EvalStack.Pop() : new RuntimeValue(ElaObject.Unit));
		}


		public ExecutionResult Run(int offset)
		{
			MainThread.Offset = offset;
			return Run();
		}


		public void RefreshState()
		{
			if (modules.Length > 0)
			{
				var mem = modules[0];
				var frame = asm.GetRootModule();
				var arr = new RuntimeValue[frame.Layouts[0].Size];
				Array.Copy(mem, 0, arr, 0, mem.Length);
				modules[0] = arr;
				MainThread.SwitchModule(0);
				var cp = MainThread.CallStack.Pop();
				MainThread.CallStack.Push(new CallPoint(cp.ReturnAddress, cp.ModuleHandle,
					cp.StackOffset, cp.ReturnValue, arr, cp.Captures));
			}
		}
		
		
		public RuntimeValue GetVariableByHandle(int moduleHandle, int varHandle)
		{
			return modules[moduleHandle][varHandle];
		}
		#endregion

		
		#region Execute
		private void Execute(WorkerThread thread)
		{
			if (thread.Busy)
				throw BusyException();

			thread.Busy = true;

			var evalStack = thread.EvalStack;
			var callStack = thread.CallStack;
			var frame = thread.Module;
			var ops = thread.Module.Ops.GetRawArray();
			var opData = thread.Module.OpData.GetRawArray();
			var locals = callStack.Peek().Locals;
			var captures = callStack.Peek().Captures;

			var left = default(RuntimeValue);
			var right = default(RuntimeValue);
			var res = default(RuntimeValue);
			var i4 = 0;
			var i4_2 = 0;

			for (; ; )
			{
				#region Body
				var op = ops[thread.Offset];
				var opd = opData[thread.Offset];
				thread.Offset++;

				switch (op)
				{
					#region Stack Operations
					case Op.Pushvar:
						i4 = opd & Byte.MaxValue;

						if (i4 == 0)
							evalStack.Push(locals[opd >> 8]);
						else
							evalStack.Push(captures[captures.Count - i4][opd >> 8]);
						break;
					case Op.Pushstr:
						evalStack.Push(new RuntimeValue(frame.Strings[opd]));
						break;
					case Op.Pushstr_0:
						evalStack.Push(new RuntimeValue(String.Empty));
						break;
					case Op.PushI4:
						evalStack.Push(opd);
						break;
					case Op.PushI4_0:
						evalStack.Push(0);
						break;
					case Op.PushI1_0:
						evalStack.Push(new RuntimeValue(false));
						break;
					case Op.PushI1_1:
						evalStack.Push(new RuntimeValue(true));
						break;
					case Op.PushR4:
						evalStack.Push(new RuntimeValue(opd, ElaObject.Single));
						break;
					case Op.PushCh:
						evalStack.Push(new RuntimeValue((Char)opd));
						break;
					case Op.Pushptr:
						evalStack.Push(new RuntimeValue(ElaObject.Pointer));
						break;
					case Op.Pushunit:
						evalStack.Push(new RuntimeValue(ElaObject.Unit));
						break;
					case Op.Pushelem:
						{
							right = evalStack.Pop();
							left = evalStack.Pop();
							var coll = left.Ref as ElaIndexedObject;

							if (coll == null)
							{
								ExecuteThrow(ElaRuntimeError.OperationNotSupported, thread, left.DataType.GetShortForm());
								goto default;
							}
							else
							{
								res = coll.GetValue(right);

								if (res.Type == ___)
								{
									ExecuteThrow(ElaRuntimeError.IndexOutOfRange, thread);
									goto default;
								}
								else
									evalStack.Push(res);
							}
						}
						break;
					case Op.Pushfld:
						{
							var fld = frame.Strings[opd];
							right = evalStack.Pop();

							if (right.Type == MOD)
							{
								i4 = ((ElaModule)right.Ref).Handle;
								var fr = asm.GetModule(i4);
								ScopeVar sc;

								if (!fr.GlobalScope.Locals.TryGetValue(fld, out sc))
								{
									ExecuteThrow(ElaRuntimeError.UndefinedVariable, thread, fld);
									goto default;
								}
								else if ((sc.Flags & ElaVariableFlags.Private) == ElaVariableFlags.Private)
								{
									ExecuteThrow(ElaRuntimeError.PrivateVariable, thread, fld);
									goto default;
								}
								else
								{
									right = modules[i4][sc.Address];
									evalStack.Push(right);
								}
							}
							else
							{
								if (right.Type == REC)
								{
									res = ((ElaRecord)right.Ref).GetField(fld);

									if (res.Type == ___)
										res = right.Ref.GetAttribute(fld);
								}
								else
									res = right.Ref.GetAttribute(fld);

								if (res.Type == ___)
								{
									ExecuteThrow(ElaRuntimeError.UnknownField, thread, fld);
									goto default;
								}
								else
									evalStack.Push(res);
							}							
						}
						break;
					case Op.Pushseq:
						{
							var seq = (ElaSequence)evalStack.Pop().Ref;

							if (seq.Function == null)
								evalStack.Push(seq.GetNext());
							else
							{
								Call(seq.Function, thread, 0);
								goto default;
							}
						}
						break;
					case Op.Pop:
						evalStack.Pop();
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
							var coll = left.Ref as ElaIndexedObject;

							if (coll != null && !coll.ReadOnly)
							{
								if (!coll.SetValue(right, evalStack.Pop()))
								{
									ExecuteThrow(ElaRuntimeError.IndexOutOfRange, thread);
									goto default;
								}
							}
							else
							{
								ExecuteThrow(ElaRuntimeError.OperationNotSupported, thread, left.DataType.GetShortForm());
								goto default;
							}
						}
						break;
					case Op.Popfld:
						{
							right = evalStack.Pop();
							left = evalStack.Pop();
							var fld = frame.Strings[opd];

							if (right.Type == REC)
							{
								if (!((ElaRecord)right.Ref).SetField(fld, left))
								{
									ExecuteThrow(ElaRuntimeError.UnknownField, thread, fld);
									goto default;
								}
							}
							else
							{
								InvalidType(right, thread, REC);
								goto default;
							}
						}
						break;
					case Op.Dup:
						right = evalStack.Peek();
						evalStack.Push(right);
						break;
					case Op.Pusharg:
						{
							var name = frame.Strings[opd];

							if (asm.TryGetArgument(name, out right))
								evalStack.Push(right);
							else
							{
								UndefinedArgument(name, thread);
								goto default;
							}
						}
						break;
					case Op.Pushperv:
						{
							var name = frame.Strings[opd];
							var perv = default(Pervasive);

							if (!pervasives.Peek().TryGetValue(name, out perv))
							{
								UnknownName(name, thread);
								goto default;
							}
							else
							{
								if (perv.Module == 0)
									evalStack.Push(locals[perv.Name >> 8]);
								else
									evalStack.Push(modules[perv.Module][perv.Name >> 8]);
							}
						}
						break;
					#endregion

					#region Math Operations
					case Op.Add:
						{
							right = evalStack.Pop();
							left = evalStack.Peek();
							var aff = opAffinity[left.Type, right.Type];

							if (aff == INT)
								res = new RuntimeValue(left.I4 + right.I4);
							else if (aff == REA)
								res = new RuntimeValue(left.GetReal() + right.GetReal());
							else if (aff == STR)
								res = new RuntimeValue(left.ToString() + right.ToString());
							else if (aff == CHR)
								res = new RuntimeValue(new String(new char[] { (Char)left.I4, (Char)right.I4 }));
							else if (aff == LNG)
								res = new RuntimeValue(left.GetLong() + right.GetLong());
							else if (aff == DBL)
								res = new RuntimeValue(left.GetDouble() + right.GetDouble());
							else if (aff == ARR)
								res = ConcatArrays(left, right);
							else if (aff == LST)
								res = ConcatLists(left, right);
							else
							{
								InvalidBinaryOperation("+", left, right, thread);
								goto default;
							}

							evalStack.Replace(res);
						}
						break;
					case Op.Sub:
						right = evalStack.Pop();
						left = evalStack.Peek();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT)
							res = new RuntimeValue(left.I4 - right.I4);
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() - right.GetReal());
						else if (i4 == LNG)
							res = new RuntimeValue(left.GetLong() - right.GetLong());
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() - right.GetDouble());
						else
						{
							InvalidBinaryOperation("-", left, right, thread);
							goto default;
						}

						evalStack.Replace(res);
						break;
					case Op.Div:
						right = evalStack.Pop();
						left = evalStack.Peek();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT)
						{
							if (right.I4 == 0)
							{
								ExecuteThrow(ElaRuntimeError.DivideByZero, thread);
								goto default;
							}
							else
								res = new RuntimeValue(left.I4 / right.I4);
						}
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() / right.GetReal());
						else if (i4 == LNG)
						{
							var lng = left.GetLong();

							if (lng == 0)
							{
								ExecuteThrow(ElaRuntimeError.DivideByZero, thread);
								goto default;
							}
							else
								res = new RuntimeValue(lng / right.GetLong());
						}
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() / right.GetDouble());
						else
						{
							InvalidBinaryOperation("/", left, right, thread);
							goto default;
						}

						evalStack.Replace(res);
						break;
					case Op.Mul:
						right = evalStack.Pop();
						left = evalStack.Peek();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT)
							res = new RuntimeValue(left.I4 * right.I4);
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() * right.GetReal());
						else if (i4 == LNG)
							res = new RuntimeValue(left.GetLong() * right.GetLong());
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() * right.GetDouble());
						else
						{
							InvalidBinaryOperation("*", left, right, thread);
							goto default;
						}

						evalStack.Replace(res);
						break;
					case Op.Pow:
						right = evalStack.Pop();
						left = evalStack.Peek();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT)
							res = new RuntimeValue((Int32)Math.Pow(left.I4, right.I4));
						else if (i4 == REA)
							res = new RuntimeValue(Math.Pow(left.GetReal(), right.GetReal()));
						else
						{
							InvalidBinaryOperation("**", left, right, thread);
							goto default;
						}

						evalStack.Replace(res);
						break;
					case Op.Rem:
						right = evalStack.Pop();
						left = evalStack.Peek();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT)
						{
							if (right.I4 == 0)
							{
								ExecuteThrow(ElaRuntimeError.DivideByZero, thread);
								goto default;
							}
							else
								res = new RuntimeValue(left.I4 % right.I4);
						}
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() % right.GetReal());
						else if (i4 == LNG)
						{
							var lng = left.GetLong();

							if (lng == 0)
							{
								ExecuteThrow(ElaRuntimeError.DivideByZero, thread);
								goto default;
							}
							else
								res = new RuntimeValue(lng % right.GetLong());
						}
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() % right.GetDouble());
						else
						{
							InvalidBinaryOperation("%", left, right, thread);
							goto default;
						}

						evalStack.Replace(res);
						break;
					#endregion

					#region Comparison Operations
					case Op.Ctag:
						{
							right = evalStack.Pop();
							left = evalStack.Pop();
							var tag = String.Empty;

							if (right.Type == TUP)
								tag = ((ElaTuple)right.Ref).Tag;
							else if (right.Type == REC)
								tag = ((ElaRecord)right.Ref).Tag;
							else
							{
								InvalidType(right, thread, TUP, REC);
								goto default;
							}

							var cons = left.Ref.ToString();
							evalStack.Push(new RuntimeValue(left.ToString() == tag));
						}
						break;
					case Op.Htag:
						right = evalStack.Pop();

						if (right.Type == TUP)
							evalStack.Push(new RuntimeValue(((ElaTuple)right.Ref).Tag != null));
						else if (right.Type == REC)
							evalStack.Push(new RuntimeValue(((ElaRecord)right.Ref).Tag != null));
						else
						{
							InvalidType(right, thread, TUP, REC);
							goto default;
						}
						break;
					case Op.Cgt:
						right = evalStack.Pop();
						left = evalStack.Pop();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
							res = new RuntimeValue(left.I4 > right.I4);
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() > right.GetReal());
						else if (i4 == LNG)
							res = new RuntimeValue(left.GetLong() > right.GetLong());
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() > right.GetDouble());
						else
						{
							InvalidBinaryOperation(">", left, right, thread);
							goto default;
						}

						evalStack.Push(res);
						break;
					case Op.Clt:
						right = evalStack.Pop();
						left = evalStack.Pop();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
							res = new RuntimeValue(left.I4 < right.I4);
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() < right.GetReal());
						else if (i4 == LNG)
							res = new RuntimeValue(left.GetLong() < right.GetLong());
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() < right.GetDouble());
						else
						{
							InvalidBinaryOperation("<", left, right, thread);
							goto default;
						}

						evalStack.Push(res);
						break;
					case Op.Ceq:
						right = evalStack.Pop();
						left = evalStack.Pop();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
							res = new RuntimeValue(left.I4 == right.I4);
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() == right.GetReal());
						else if (i4 == STR)
							res = new RuntimeValue(left.Ref.ToString() == right.Ref.ToString());
						else if (i4 == LNG)
							res = new RuntimeValue(left.GetLong() == right.GetLong());
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() == right.GetDouble());
						else if (i4 == ___)
						{
							InvalidBinaryOperation("==", left, right, thread);
							goto default;
						}
						else
							res = new RuntimeValue(left.Ref == right.Ref);

						evalStack.Push(res);
						break;
					case Op.Cneq:
						right = evalStack.Pop();
						left = evalStack.Pop();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
							res = new RuntimeValue(left.I4 != right.I4);
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() != right.GetReal());
						else if (i4 == STR)
							res = new RuntimeValue(left.Ref.ToString() != right.Ref.ToString());
						else if (i4 == LNG)
							res = new RuntimeValue(left.GetLong() != right.GetLong());
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() != right.GetDouble());
						else if (i4 == ___)
						{
							InvalidBinaryOperation("!=", left, right, thread);
							goto default;
						}
						else
							res = new RuntimeValue(left.Ref != right.Ref);

						evalStack.Push(res);
						break;
					case Op.Cgteq:
						right = evalStack.Pop();
						left = evalStack.Pop();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
							res = new RuntimeValue(left.I4 >= right.I4);
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() >= right.GetReal());
						else if (i4 == LNG)
							res = new RuntimeValue(left.GetLong() >= right.GetLong());
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() >= right.GetDouble());
						else
						{
							InvalidBinaryOperation(">=", left, right, thread);
							goto default;
						}

						evalStack.Push(res);
						break;
					case Op.Clteq:
						right = evalStack.Pop();
						left = evalStack.Pop();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
							res = new RuntimeValue(left.I4 <= right.I4);
						else if (i4 == REA)
							res = new RuntimeValue(left.GetReal() <= right.GetReal());
						else if (i4 == LNG)
							res = new RuntimeValue(left.GetLong() <= right.GetLong());
						else if (i4 == DBL)
							res = new RuntimeValue(left.GetDouble() <= right.GetDouble());
						else
						{
							InvalidBinaryOperation("<=", left, right, thread);
							goto default;
						}

						evalStack.Push(res);
						break;
					#endregion

					#region List Operations
					case Op.Listadd:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (right.Type == LST)
						{
							res = new RuntimeValue(new ElaList((ElaList)right.Ref, left));
							evalStack.Push(res);
						}
						else
						{
							InvalidType(right, thread, LST);
							goto default;
						}
						break;
					case Op.Listgen:
						ConstructList(opd, thread);
						break;
					case Op.Listtail:
						right = evalStack.Pop();

						if (right.Type != LST)
						{
							InvalidType(right, thread, LST);
							goto default;
						}
						else if (right.Ref != ElaList.Nil)
							evalStack.Push(new RuntimeValue(((ElaList)right.Ref).Next));
						else
							evalStack.Push(new RuntimeValue(ElaList.Nil));
						break;
					case Op.Listelem:
						right = evalStack.Pop();

						if (right.Type != LST)
						{
							InvalidType(right, thread, LST);
							goto default;
						}
						else
							evalStack.Push(((ElaList)right.Ref).Value);
						break;
					#endregion

					#region Binary Operations
					case Op.AndBw:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.Type != INT || right.Type != INT)
						{
							InvalidType(left, thread, INT);
							goto default;
						}
						else
						{
							res = new RuntimeValue(left.I4 & right.I4);
							evalStack.Push(res);
						}
						break;
					case Op.OrBw:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.Type != INT || right.Type != INT)
						{
							InvalidType(left, thread, INT);
							goto default;
						}
						else
						{
							res = new RuntimeValue(left.I4 | right.I4);
							evalStack.Push(res);
						}
						break;
					case Op.Xor:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (left.Type != INT || right.Type != INT)
						{
							InvalidType(left, thread, INT);
							goto default;
						}
						else
						{
							res = new RuntimeValue(left.I4 ^ right.I4);
							evalStack.Push(res);
						}
						break;
					case Op.Shl:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (right.Type != INT)
						{
							InvalidType(right, thread, INT);
							goto default;
						}
						else
						{
							if (left.Type == INT)
								evalStack.Push(left.I4 << right.I4);
							else if (left.Type == LNG)
								evalStack.Push(new RuntimeValue(left.GetLong() << right.I4));
							else
							{
								InvalidType(left, thread, INT, LNG);
								goto default;
							}
						}
						break;
					case Op.Shr:
						right = evalStack.Pop();
						left = evalStack.Pop();

						if (right.Type != INT)
						{
							InvalidType(right, thread, INT);
							goto default;
						}
						else
						{
							if (left.Type == INT)
								evalStack.Push(left.I4 >> right.I4);
							else if (left.Type == LNG)
								evalStack.Push(new RuntimeValue(left.GetLong() >> right.I4));
							else
							{
								InvalidType(left, thread, INT, LNG);
								goto default;
							}
						}
						break;
					#endregion

					#region Unary Operations
					case Op.Incr:
						{
							right = locals[opd >> 8];

							if (right.Type != INT)
							{
								InvalidUnaryOperation("++", right, thread);
								goto default;
							}
							else
							{
								right = new RuntimeValue(right.I4 + 1);
								evalStack.Push(right);
								locals[opd >> 8] = right;
							}
						}
						break;
					case Op.Decr:
						{
							right = locals[opd >> 8];

							if (right.Type != INT)
							{
								InvalidUnaryOperation("--", right, thread);
								goto default;
							}
							else
							{
								right = new RuntimeValue(right.I4 - 1);
								evalStack.Push(right);
								locals[opd >> 8] = right;
							}
						}
						break;
					case Op.Not:
						right = evalStack.Pop();

						if (right.Type != BYT)
						{
							InvalidUnaryOperation("!", right, thread);
							goto default;
						}
						else
						{
							res = new RuntimeValue(!(right.I4 == 1));
							evalStack.Push(res);
						}
						break;
					case Op.Neg:
						right = evalStack.Pop();

						if (right.Type == INT)
							res = new RuntimeValue(-right.I4);
						else if (right.Type == REA)
							res = new RuntimeValue(-right.GetReal());
						else if (right.Type == DBL)
							res = new RuntimeValue(-right.GetDouble());
						else if (right.Type == LNG)
							res = new RuntimeValue(-right.GetLong());
						else
						{
							InvalidUnaryOperation("-", right, thread);
							goto default;
						}

						evalStack.Push(res);
						break;
					case Op.Pos:
						right = evalStack.Pop();

						if (right.Type == INT)
							res = new RuntimeValue(+right.I4);
						else if (right.Type == REA)
							res = new RuntimeValue(+right.GetReal());
						else if (right.Type == DBL)
							res = new RuntimeValue(+right.GetDouble());
						else if (right.Type == LNG)
							res = new RuntimeValue(+right.GetLong());
						else
						{
							InvalidUnaryOperation("+", right, thread);
							goto default;
						}

						evalStack.Push(res);
						break;
					case Op.NotBw:
						right = evalStack.Pop();

						if (right.Type != INT)
						{
							InvalidUnaryOperation("~", right, thread);
							goto default;
						}
						else
						{
							res = new RuntimeValue(~right.I4);
							evalStack.Push(res);
						}
						break;
					case Op.Valueof:
						if (ValueOf(thread))
							goto default;
						break;
					#endregion

					#region Conversion Operations
					case Op.ConvI4:
						ConvertI4(evalStack.Peek(), thread);
						goto default;
					case Op.ConvR4:
						ConvertR4(evalStack.Peek(), thread);
						goto default;
					case Op.ConvI1:
						ConvertI1(evalStack.Peek(), thread);
						goto default;
					case Op.ConvStr:
						ConvertStr(evalStack.Peek(), thread);
						goto default;
					case Op.ConvI8:
						ConvertI8(evalStack.Peek(), thread);
						goto default;
					case Op.ConvCh:
						ConvertCh(evalStack.Peek(), thread);
						goto default;
					case Op.ConvR8:
						ConvertR8(evalStack.Peek(), thread);
						goto default;
					case Op.ConvSeq:
						ConvertSeq(evalStack.Peek(), thread);
						goto default;
					#endregion

					#region Goto Operations
					case Op.Brtrue:
						right = evalStack.Pop();

						if (right.Type != BYT)
							InvalidType(right, thread, BYT);
						else if (right.I4 == 1)
							thread.Offset = opd;
						break;
					case Op.Brfalse:
						right = evalStack.Pop();

						if (right.Type != BYT)
							InvalidType(right, thread, BYT);
						else if (right.I4 == 0)
							thread.Offset = opd;
						break;
					case Op.Brptr:
						right = evalStack.Peek();

						if (right.Type == PTR)
							thread.Offset = opd;
						break;
					case Op.Br:
						thread.Offset = opd;
						break;
					case Op.Br_eq:
						right = evalStack.Pop();
						left = evalStack.Pop();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
						{
							if (left.I4 == right.I4)
								thread.Offset = opd;
						}
						else if (i4 == REA)
						{
							if (left.GetReal() == right.GetReal())
								thread.Offset = opd;
						}
						else if (i4 == STR)
						{
							if (left.Ref.ToString() == right.Ref.ToString())
								thread.Offset = opd;
						}
						else if (i4 == LNG)
						{
							if (left.GetLong() == right.GetLong())
								thread.Offset = opd;
						}
						else if (i4 == DBL)
						{
							if (left.GetDouble() == right.GetDouble())
								thread.Offset = opd;
						}
						else if (i4 == ___)
							InvalidBinaryOperation("==", left, right, thread);
						else
						{
							if (left.Ref == right.Ref)
								thread.Offset = opd;
						}
						break;
					case Op.Br_neq:
						right = evalStack.Pop();
						left = evalStack.Pop();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
						{
							if (left.I4 != right.I4)
								thread.Offset = opd;
						}
						else if (i4 == REA)
						{
							if (left.GetReal() != right.GetReal())
								thread.Offset = opd;
						}
						else if (i4 == STR)
						{
							if (left.Ref.ToString() != right.Ref.ToString())
								thread.Offset = opd;
						}
						else if (i4 == LNG)
						{
							if (left.GetLong() != right.GetLong())
								thread.Offset = opd;
						}
						else if (i4 == DBL)
						{
							if (left.GetDouble() != right.GetDouble())
								thread.Offset = opd;
						}
						else if (i4 == ___)
							InvalidBinaryOperation("!=", left, right, thread);
						else
						{
							if (left.Ref != right.Ref)
								thread.Offset = opd;
						}
						break;
					case Op.Br_lt:
						right = evalStack.Pop();
						left = evalStack.Pop();
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
						{
							if (left.I4 < right.I4)
								thread.Offset = opd;
						}
						else if (i4 == REA)
						{
							if (left.GetReal() < right.GetReal())
								thread.Offset = opd;
						}
						else if (i4 == LNG)
						{
							if (left.GetLong() < right.GetLong())
								thread.Offset = opd;
						}
						else if (i4 == DBL)
						{
							if (left.GetDouble() < right.GetDouble())
								thread.Offset = opd;
						}
						else
							InvalidBinaryOperation("<", left, right, thread);
						break;
					case Op.Br_gt:
						right = evalStack.Pop();
						left = evalStack.Pop();
						
						i4 = opAffinity[left.Type, right.Type];

						if (i4 == INT || i4 == CHR)
						{
							if (left.I4 > right.I4)
								thread.Offset = opd;
						}
						else if (i4 == REA)
						{
							if (left.GetReal() > right.GetReal())
								thread.Offset = opd;
						}
						else if (i4 == LNG)
						{
							if (left.GetLong() > right.GetLong())
								thread.Offset = opd;
						}
						else if (i4 == DBL)
						{
							if (left.GetDouble() > right.GetDouble())
								thread.Offset = opd;
						}
						else
							InvalidBinaryOperation(">", left, right, thread);
						break;
					case Op.Brdyn:
						i4 = opd;
						i4_2 = i4 & Byte.MaxValue;

						if (i4_2 == 0)
							right = locals[i4 >> 8];
						else
							right = captures[captures.Count - i4_2][i4 >> 8];

						if (right.I4 > 0)
							thread.Offset = right.I4;
						break;
					#endregion

					#region CreateNew Operations
					case Op.Newlazy:
						{
							right = evalStack.Pop();
							evalStack.Push(new RuntimeValue(new ElaLazy((ElaFunction)right.Ref)));
						}
						break;
					case Op.Newfun:
						{
							var lst = new FastList<RuntimeValue[]>(captures);
							lst.Add(locals);
							evalStack.Push(new RuntimeValue(new ElaNativeFunction(opd,
								thread.ModuleHandle, evalStack.Pop().I4, lst, this)));
						}
						break;
					case Op.Newfund:
						{
							right = evalStack.Pop();

							if (right.Type != FUN)
							{
								InvalidType(right, thread, FUN);
								goto default;
							}
							else
							{
								var parCount = ((ElaFunction)right.Ref).ParameterCount;
								var lst = new FastList<RuntimeValue[]>(captures);
								lst.Add(locals);
								evalStack.Push(new RuntimeValue(new ElaNativeFunction(opd,
									thread.ModuleHandle, parCount, lst, this)));
							}
						}
						break;
					case Op.Newfuns:
						{
							var layout = thread.Module.Layouts[opd];
							var lst = new FastList<RuntimeValue[]>(captures);
							lst.Add(locals);
							var newMem = new RuntimeValue[layout.Size];
							evalStack.Push(new RuntimeValue(new ElaNativeFunction(opd,
								thread.ModuleHandle, 0, lst, this) { Memory = newMem }));
						}
						break;
					case Op.Newlist:
						evalStack.Push(new RuntimeValue(ElaList.Nil));
						break;
					case Op.Newrec:
						ConstructRecord(thread, opd);
						break;
					case Op.Newarr:
						ConstructArray(opd, thread);
						break;
					case Op.Newarr_0:
						evalStack.Push(new RuntimeValue(new ElaArray(4)));
						break;
					case Op.Newtup:
						{
							var tup = new ElaTuple(opd);
							
							for (var j = opd - 1; j > -1; j--)
								tup.InternalSetValue(j, evalStack.Pop());

							evalStack.Push(new RuntimeValue(tup));
						}
						break;
					case Op.Newtup_2:
						{
							var tup = new ElaTuple(2);
							tup.InternalSetValue(1, evalStack.Pop());
							tup.InternalSetValue(0, evalStack.Pop());
							evalStack.Push(new RuntimeValue(tup));
						}
						break;
					case Op.Newseq:
						{
							right = evalStack.Pop();

							if (right.Type == SEQ)
								evalStack.Push(right);
							else if (right.Type != ARR && right.Type != LST && right.Type != TUP &&
								right.Type != STR && right.Type != FUN)
							{
								InvalidType(right, thread, ARR, LST, TUP, STR);
								goto default;
							}
							else if (right.Type == FUN)
							{
								var sf = right.Ref as ElaNativeFunction;

								if (sf == null || sf.Memory == null)
								{
									InvalidType(right, thread, ARR, LST, TUP, STR);
									goto default;
								}

								evalStack.Push(new RuntimeValue(new ElaSequence(right.Ref)));
							}
							else
								evalStack.Push(new RuntimeValue(new ElaSequence(right.Ref)));
						}
						break;
					case Op.NewI8:
						{
							right = evalStack.Pop();
							left = evalStack.Pop();
							var conv = new Conv();
							conv.I4_1 = left.I4;
							conv.I4_2 = right.I4;
							evalStack.Push(new RuntimeValue(conv.I8));
						}
						break;
					case Op.NewR8:
						{
							right = evalStack.Pop();
							left = evalStack.Pop();
							var conv = new Conv();
							conv.I4_1 = left.I4;
							conv.I4_2 = right.I4;
							evalStack.Push(new RuntimeValue(conv.R8));
						}
						break;
					case Op.Newmod:
						{
							var str = frame.Strings[opd];
							i4 = asm.GetModuleHandle(str);
							evalStack.Push(new RuntimeValue(new ElaModule(i4, this)));
						}
						break;
					case Op.Hasfld:
						{
							var fld = frame.Strings[opd];
							right = evalStack.Pop();

							if (right.Type == REC)
								evalStack.Push(new RuntimeValue(((ElaRecord)right.Ref).HasField(fld)));
							else
								evalStack.Push(new RuntimeValue(false));
						}
						break;
					#endregion

					#region Function Operations
					case Op.Call:
						if (Call(evalStack.Pop().Ref, thread, opd))
							goto default;
						break;
					case Op.Calld:
						if (Call(evalStack.Pop().Ref, thread, evalStack.Count - callStack.Peek().StackOffset))
							goto default;
						break;
					case Op.Callt:
						if (Call(evalStack.Pop().Ref, thread, opd))
							goto default;
						break;
					case Op.Calla:
						CallAsync(thread);
						break;
					case Op.Epilog:
						i4 = opd;
						i4_2 = i4 & Byte.MaxValue;

						if (i4_2 == 0)
							locals[i4 >> 8] = new RuntimeValue(0);
						else
							captures[captures.Count - i4_2][i4 >> 8] = new RuntimeValue(0);

						evalStack.Pop();
						evalStack.Push(new RuntimeValue(ElaObject.Pointer));
						break;
					case Op.Yield:
						{
							i4 = opd;
							i4_2 = i4 & Byte.MaxValue;

							if (i4_2 == 0)
								locals[i4 >> 8] = new RuntimeValue(thread.Offset);
							else
								captures[captures.Count - i4_2][i4 >> 8] = new RuntimeValue(thread.Offset);

							var om = callStack.Pop();

							if (om.ReturnAddress == 0)
								return;

							thread.Offset = om.ReturnAddress;
							goto default;
						}
					case Op.Ret:
						{
							var om = callStack.Pop();

							if (om.ReturnValue != null)
								om.ReturnValue.SetValue(evalStack.Peek());

							if (callStack.Count > 0)
							{
								if (om.ReturnAddress == 0)
									return;
								else
								{
									thread.Offset = om.ReturnAddress;
									goto default;
								}
							}
							else
							{
								Monitor.Enter(syncRoot);
								Threads.Remove(thread);
								Monitor.Exit(syncRoot);

								if (thread.ReturnValue != null)
									thread.ReturnValue.SetValue(thread.EvalStack.Pop());

								return;
							}
						}
					#endregion

					#region Complex and Debug
					case Op.Nop:
						break;
					case Op.Cout:
						right = evalStack.Pop();
						Console.WriteLine(right);
						break;
					case Op.Typeof:
						evalStack.Push(new RuntimeValue(evalStack.Pop().Type));
						break;
					case Op.Throw:
						ExecuteThrow((ElaRuntimeError)opd, thread);
						goto default;
					case Op.Term:
						{
							if (callStack.Count > 1)
							{
								var modMem = callStack.Pop();
								thread.Offset = modMem.ReturnAddress;
								
								if (pervasives.Count > 0)
									pervasives.Pop();
								
								if (pervasives.Count > 0)
									ReadPervasives(thread.Module, thread.ModuleHandle);
								
								thread.SwitchModule(callStack.Peek().ModuleHandle);
								evalStack.Pop();
								goto default;
							}
							else
							{
								thread.Busy = false;
								return;
							}
						}
					case Op.Runmod:
						{
							var mn = frame.Strings[opd];
							var hdl = asm.GetModuleHandle(mn);
								
							if (modules[hdl] == null)
							{
								var frm = asm.GetModule(hdl);
								pervasives.Push(new Dictionary<String,Pervasive>());

								if (frm is IntrinsicFrame)
									modules[hdl] = ((IntrinsicFrame)frm).Memory;
								else
								{
									i4 = frm.Layouts[0].Size;
									var loc = new RuntimeValue[i4];
									modules[hdl] = loc;
									var modMem = new CallPoint(thread.Offset, hdl, evalStack.Count,
										null, loc, FastList<RuntimeValue[]>.Empty);
									callStack.Push(modMem);
									thread.SwitchModule(hdl);
									thread.Offset = 0;
									goto default;
								}
							}
						}
						break;
					case Op.Start:
						callStack.Peek().CatchMark = opd;
						break;
					case Op.Leave:
						callStack.Peek().CatchMark = null;
						break;
					case Op.Settag:
						{
							right = evalStack.Peek();

							if (right.Type == TUP || right.Type == REC)
								((ElaTuple)right.Ref).Tag = frame.Strings[opd];
							else
							{
								InvalidType(right, thread, TUP, REC);
								goto default;
							}
						}
						break;
					case Op.Len:
						{
							right = evalStack.Pop();
							var obj = right.Ref as ElaIndexedObject;

							if (obj != null)
								evalStack.Push(obj.Length);
							else if (right.Type == LST)
								evalStack.Push(((ElaList)right.Ref).GetLength());
							else
								InvalidType(right, thread, TUP, REC, ARR, LST, STR);
						}
						break;
					#endregion

					#region Control Structures
					default:
						{
							var mem = callStack.Peek();
							thread.SwitchModule(mem.ModuleHandle);
							locals = mem.Locals;
							captures = mem.Captures;
							ops = thread.Module.Ops.GetRawArray();
							opData = thread.Module.OpData.GetRawArray();
							frame = thread.Module;
						}
						break;
					#endregion
				}
				#endregion
			}
		}
		#endregion


		#region Helper Methods
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


		private void InvalidBinaryOperation(string op, RuntimeValue left, RuntimeValue right, WorkerThread thread)
		{
			ExecuteThrow(ElaRuntimeError.InvalidBinaryOperation, thread, op,
				left.DataType.GetShortForm(), right.DataType.GetShortForm());
		}


		private void InvalidUnaryOperation(string op, RuntimeValue val, WorkerThread thread)
		{
			ExecuteThrow(ElaRuntimeError.InvalidUnaryOperation, thread, op, val.DataType.GetShortForm());
		}


		private void InvalidType(RuntimeValue val, WorkerThread thread, params int[] exps)
		{
			ExecuteThrow(ElaRuntimeError.InvalidType, thread, FormatTypes(exps), FormatTypes(val.Type));
		}


		private void InvalidParameterNumber(int pars, int passed, WorkerThread thread)
		{
			if (passed == 0)
				ExecuteThrow(ElaRuntimeError.CallWithNoParams, thread, pars);
			else if (passed > pars)
				ExecuteThrow(ElaRuntimeError.TooManyParams, thread);
			else if (passed < pars)
				ExecuteThrow(ElaRuntimeError.TooFewParams, thread);
		}


		private void ConversionFailed(RuntimeValue val, int target, string err, WorkerThread thread)
		{
			ExecuteThrow(ElaRuntimeError.ConversionFailed, thread, FormatTypes(val.Type),
				FormatTypes(target), err);
		}


		private void UnableToConvert(RuntimeValue val, int target, WorkerThread thread)
		{
			ExecuteThrow(ElaRuntimeError.UnableConvert, thread, FormatTypes(val.Type),
				FormatTypes(target));
		}


		private void UnknownName(string name, WorkerThread thread)
		{
			ExecuteThrow(ElaRuntimeError.UnknownPervasive, thread, name);
		}


		private void UndefinedArgument(string name, WorkerThread thread)
		{
			ExecuteThrow(ElaRuntimeError.UndefinedArgument, thread, name);
		}


		internal static ObjectType GetOperationAffinity(ElaObject left, ElaObject right)
		{
			return (ObjectType)opAffinity[left.TypeId, right.TypeId];
		}


		private ElaFatalException BusyException()
		{
			return new ElaFatalException(Strings.GetMessage("ThreadBusy"));
		}
		#endregion


		#region Conversions
		private void ConvertI4(RuntimeValue val, WorkerThread thread)
		{
			var evalStack = thread.EvalStack;

			if (val.Type == INT)
			{
				//idle
			}
			else if (val.Type == CHR)
				evalStack.Replace(new RuntimeValue(val.I4));
			else if (val.Type == REA)
				evalStack.Replace(new RuntimeValue((Int32)val.GetReal()));
			else if (val.Type == BYT)
				evalStack.Replace(new RuntimeValue(val.I4));
			else if (val.Type == LNG)
				evalStack.Replace(new RuntimeValue((Int32)val.GetLong()));
			else if (val.Type == DBL)
				evalStack.Replace(new RuntimeValue((Int32)val.GetDouble()));
			else if (val.Type == STR)
			{
				try
				{
					evalStack.Replace(new RuntimeValue(Int32.Parse(val.Ref.ToString())));
				}
				catch (Exception ex)
				{
					ConversionFailed(val, INT, ex.Message, thread);
				}
			}
			else
				UnableToConvert(val, INT, thread);
		}


		private void ConvertI8(RuntimeValue val, WorkerThread thread)
		{
			var evalStack = thread.EvalStack;

			if (val.Type == INT || val.Type == CHR)
				evalStack.Replace(new RuntimeValue((Int64)val.I4));
			else if (val.Type == REA)
				evalStack.Replace(new RuntimeValue((Int64)val.GetReal()));
			else if (val.Type == BYT)
				evalStack.Replace(new RuntimeValue((Int64)val.I4));
			else if (val.Type == LNG)
			{
				//idle
			}
			else if (val.Type == DBL)
				evalStack.Replace(new RuntimeValue((Int64)val.GetDouble()));
			else if (val.Type == STR)
			{
				try
				{
					evalStack.Replace(new RuntimeValue(Int64.Parse(val.Ref.ToString())));
				}
				catch (Exception ex)
				{
					ConversionFailed(val, LNG, ex.Message, thread);
				}
			}
			else
				UnableToConvert(val, LNG, thread);
		}


		private void ConvertCh(RuntimeValue val, WorkerThread thread)
		{
			var evalStack = thread.EvalStack;

			if (val.Type == INT)
				evalStack.Replace(new RuntimeValue((Char)val.I4));
			else if (val.Type == CHR)
			{
				//idle
			}
			else if (val.Type == REA)
				evalStack.Replace(new RuntimeValue((Char)val.GetReal()));
			else if (val.Type == BYT)
				evalStack.Replace(new RuntimeValue((Char)val.I4));
			else if (val.Type == LNG)
				evalStack.Replace(new RuntimeValue((Char)val.GetLong()));
			else if (val.Type == DBL)
				evalStack.Replace(new RuntimeValue((Char)val.GetDouble()));
			else if (val.Type == STR)
			{
				var str = val.Ref.ToString();
				evalStack.Replace(new RuntimeValue(str.Length > 0 ? str[0] : '\0'));
			}
			else
				UnableToConvert(val, CHR, thread);
		}


		private void ConvertR4(RuntimeValue val, WorkerThread thread)
		{
			var evalStack = thread.EvalStack;

			if (val.Type == INT || val.Type == CHR)
				evalStack.Replace(new RuntimeValue((Single)val.I4));
			else if (val.Type == REA)
			{
				//idle
			}
			else if (val.Type == BYT)
				evalStack.Replace(new RuntimeValue((Single)val.I4));
			else if (val.Type == DBL)
				evalStack.Replace(new RuntimeValue((Single)val.GetDouble()));
			else if (val.Type == LNG)
				evalStack.Replace(new RuntimeValue((Single)val.GetLong()));
			else if (val.Type == STR)
			{
				try
				{
					evalStack.Replace(new RuntimeValue(Single.Parse(val.Ref.ToString())));
				}
				catch (Exception ex)
				{
					ConversionFailed(val, REA, ex.Message, thread);
				}
			}
			else
				UnableToConvert(val, REA, thread);
		}


		private void ConvertR8(RuntimeValue val, WorkerThread thread)
		{
			var evalStack = thread.EvalStack;

			if (val.Type == INT || val.Type == CHR)
				evalStack.Replace(new RuntimeValue((Double)val.I4));
			else if (val.Type == REA)
				evalStack.Replace(new RuntimeValue((Double)val.GetReal()));
			else if (val.Type == BYT)
				evalStack.Replace(new RuntimeValue((Double)val.I4));
			else if (val.Type == DBL)
			{
				//idle
			}
			else if (val.Type == LNG)
				evalStack.Replace(new RuntimeValue((Double)val.GetLong()));
			else if (val.Type == STR)
			{
				try
				{
					evalStack.Replace(new RuntimeValue(Double.Parse(val.Ref.ToString())));
				}
				catch (Exception ex)
				{
					ConversionFailed(val, DBL, ex.Message, thread);
				}
			}
			else
				UnableToConvert(val, DBL, thread);
		}


		private void ConvertI1(RuntimeValue val, WorkerThread thread)
		{
			var evalStack = thread.EvalStack;

			if (val.Type == INT || val.Type == CHR)
				evalStack.Replace(new RuntimeValue(val.I4 > 0));
			else if (val.Type == REA)
				evalStack.Replace(new RuntimeValue((Int32)val.GetReal() > 0));
			else if (val.Type == BYT)
			{
				//idle
			}
			else if (val.Type == DBL)
				evalStack.Replace(new RuntimeValue((Int32)val.GetDouble() > 0));
			else if (val.Type == LNG)
				evalStack.Replace(new RuntimeValue((Int32)val.GetLong() > 0));
			else if (val.Type == STR)
			{
				try
				{
					evalStack.Replace(new RuntimeValue(Boolean.Parse(val.Ref.ToString())));
				}
				catch (Exception ex)
				{
					ConversionFailed(val, BYT, ex.Message, thread);
				}
			}
			else
				UnableToConvert(val, BYT, thread);
		}


		private void ConvertSeq(RuntimeValue val, WorkerThread thread)
		{
			if (val.Type == SEQ)
			{
				//idle
			}
			else if (val.Type == ARR || val.Type == STR || val.Type == TUP || val.Type == LST)
				thread.EvalStack.Replace(new RuntimeValue(new ElaSequence(val.Ref)));
			else
				UnableToConvert(val, BYT, thread);
		}


		private void ConvertStr(RuntimeValue val, WorkerThread thread)
		{
			if (val.Type != STR)
				thread.EvalStack.Replace(new RuntimeValue(val.ToString()));
		}
		#endregion


		#region Operations
		private void ReadPervasives(CodeFrame frame, int handle)
		{
			var dict = pervasives.Peek();

			foreach (var kv in frame.Pervasives)
			{
				if (dict.ContainsKey(kv.Key))
					dict.Remove(kv.Key);

				dict.Add(kv.Key, new Pervasive(handle, kv.Value));
			}
		}


		private void ConstructArray(int size, WorkerThread thread)
		{
			var arr = new ElaArray(size > 0 ? size : 4);
			var evalStack = thread.EvalStack;

			if (size == -1)
			{
				var lt = new List<RuntimeValue>();

				for (; ; )
				{
					var right = evalStack.Pop();

					if (right.Type == PTR)
						break;

					lt.Add(right);
				}

				for (var j = lt.Count - 1; j > -1; j--)
					arr.Add(lt[j]);
			}
			else
			{
				arr.Stretch(size);

				for (var j = size - 1; j > -1; j--)
					arr.InternalSetValue(j, evalStack.Pop());
			}

			evalStack.Push(new RuntimeValue(arr));
		}


		private void ConstructList(int size, WorkerThread thread)
		{
			var evalStack = thread.EvalStack;
			var list = ElaList.Nil;
			size = size == -1 ? Int32.MaxValue : size;

			for (var j = 0; j < size; j++)
			{
				var val = evalStack.Pop();

				if (val.Type == PTR)
					break;

				list = new ElaList(list, val);
			}

			evalStack.Push(new RuntimeValue(list));
		}


		private void ConstructRecord(WorkerThread thread, int size)
		{
			var rec = new ElaRecord(size);
			var evalStack = thread.EvalStack;

			for (var j = 0; j < size; j++)
			{
				var fld = evalStack.Pop().ToString();
				var val = evalStack.Pop();
				rec.AddField(j, fld, val);
			}

			evalStack.Push(new RuntimeValue(rec));
		}


		internal RuntimeValue Call(ElaNativeFunction fun, RuntimeValue[] args)
		{
			var t = MainThread;
			var len = t.EvalStack.Count;

			for (var i = 0; i < args.Length; i++)
				t.EvalStack.Push(args[args.Length - i - 1]);
			
			if (fun.ModuleHandle != t.ModuleHandle)
				t.SwitchModule(fun.ModuleHandle);

			if (args.Length != fun.ParameterCount)
				InvalidParameterNumber(fun.ParameterCount, args.Length, t);
			else
			{
				t.Busy = false;
				var layout = t.Module.Layouts[fun.Handle];
				var newMem = default(RuntimeValue[]);

				if (fun.Memory != null)
					newMem = fun.Memory;
				else
					newMem = new RuntimeValue[layout.Size];

				var os = t.Offset;
				var cp = new CallPoint(0, fun.ModuleHandle, len, null,
					newMem, fun.Captures);
				t.CallStack.Push(cp);
				t.Offset = layout.Address;
				Execute(t);
				t.Offset = os;
			}

			return t.EvalStack.Pop();
		}


		private void CurryFunction(ElaFunction fun, int parCount, WorkerThread thread)
		{
			var si = 0;
			var arr = default(RuntimeValue[]);

			if (fun.AppliedParameters != null)
			{
				si = fun.AppliedParameters.Length;
				arr = new RuntimeValue[fun.ParameterCount - parCount + si];
				Array.Copy(fun.AppliedParameters, arr, fun.AppliedParameters.Length);
			}
			else
				arr = new RuntimeValue[parCount];

			for (var i = 0; i < parCount; i++)
				arr[si + i] = thread.EvalStack.Pop();

			var newFun = fun.Clone();
			newFun.AppliedParameters = arr;
			newFun.ParameterCount -= parCount;
			thread.EvalStack.Push(new RuntimeValue(newFun));
		}


		private bool Call(ElaObject fun, WorkerThread thread, int parPassed)
		{
			var pop = true;
			
			if (fun.TypeId != FUN)
				ExecuteThrow(ElaRuntimeError.NotFunction, thread);
			else 
			{
				var natFun = fun as ElaNativeFunction;

				if (natFun != null)
				{
					if (natFun.ParameterCount == parPassed)
					{
						if (natFun.ModuleHandle != thread.ModuleHandle)
							thread.SwitchModule(natFun.ModuleHandle);

						var mod = thread.Module;
						var retAddr = thread.Offset;
						var layout = mod.Layouts[natFun.Handle];
						var newLoc = natFun.Memory != null ? natFun.Memory : new RuntimeValue[layout.Size];
						var newMem = new CallPoint(retAddr, natFun.ModuleHandle,
							thread.EvalStack.Count - parPassed, null, newLoc, natFun.Captures);

						if (natFun.AppliedParameters != null)
						{
							for (var i = 0; i < natFun.AppliedParameters.Length; i++)
								thread.EvalStack.Push(natFun.AppliedParameters[natFun.AppliedParameters.Length - i - 1]);
						}

						thread.CallStack.Push(newMem);
						thread.Offset = layout.Address;
					}
					else if (natFun.ParameterCount > parPassed && parPassed != 0)
					{
						CurryFunction(natFun, parPassed, thread);
						pop = false;
					}
					else
						InvalidParameterNumber(natFun.ParameterCount, parPassed, thread);					
				}
				else
				{
					var extFun = (ElaFunction)fun;
					pop = false;

					if (extFun.ParameterCount == parPassed || extFun.ParameterCount == -1)
						CallExternal(thread, extFun, parPassed);
					else if (extFun.ParameterCount > parPassed)
						CurryFunction(extFun, parPassed, thread);
					else
						InvalidParameterNumber(extFun.ParameterCount, parPassed, thread);					
				}
			}			

			return pop;
		}


		private void CallAsync(WorkerThread thread)
		{
			var newThread = thread.Clone();
			var lazy = default(ElaLazy);
			newThread.ReturnValue = lazy;
			var rv = thread.EvalStack.Pop();
			var fun = rv.Ref as ElaNativeFunction;
				
			if (fun != null)
			{
				Call(fun, newThread, 0);
				var t = new System.Threading.Thread(
					new System.Threading.ThreadStart(() => Execute(newThread)));
				lazy = new ElaLazy(t);
				t.Start();
			}
			else
			{
				var t = new System.Threading.Thread(
					new System.Threading.ThreadStart(() => Call(rv.Ref, newThread, 0)));
				lazy = new ElaLazy(t); 
				t.Start();
			}

			thread.EvalStack.Push(new RuntimeValue(lazy));
		}


		private void CallExternal(WorkerThread thread, ElaFunction funObj, int parPass)
		{
			thread.Busy = true;

			try
			{
				var arr = default(RuntimeValue[]);
				var si = 0;

				if (funObj.AppliedParameters != null)
				{
					si = funObj.AppliedParameters.Length;
					arr = new RuntimeValue[parPass + si];
					Array.Copy(funObj.AppliedParameters, arr, si);
				}
				else
					arr = new RuntimeValue[parPass];

				for (var i = 0; i < parPass; i++)
					arr[i + si] = thread.EvalStack.Pop();

				var res = funObj.Call(arr);
				thread.EvalStack.Push(res);
			}
			catch (ElaCallException ex)
			{
				ExecuteThrow(ex.Error, thread, ex.Message);
			}
			catch (Exception ex)
			{
				ExecuteThrow(ElaRuntimeError.ExternalCallFailed, thread, ex.Message);
			}
			finally
			{
				thread.Busy = false;
			}
		}


		private bool ValueOf(WorkerThread thread)
		{
			var val = thread.EvalStack.Pop();

			if (val.Type != LAZ)
				thread.EvalStack.Push(val);
			else
			{
				var lazy = (ElaLazy)val.Ref;

				if (!lazy.HasValue())
				{
					if (lazy.Thread != null)
						lazy.Thread.Join();
					else
					{
						if (Call(lazy.Function, thread, 0))
						{
							var mem = thread.CallStack.Peek();
							mem.ReturnAddress--;
							mem.ReturnValue = lazy;
							return true;
						}
					}
				}

				thread.EvalStack.Push(lazy.InternalValue);
			}

			return false;
		}


		private RuntimeValue ConcatArrays(RuntimeValue left, RuntimeValue right)
		{
			var arr = default(ElaArray);
			var lseq = (ElaArray)left.Ref;
			var rseq = (ElaArray)right.Ref;
			arr = new ElaArray(lseq.Length + rseq.Length);
			arr.Copy(0, lseq);
			arr.Copy(lseq.Length, rseq);
			return new RuntimeValue(arr);
		}


		private RuntimeValue ConcatLists(RuntimeValue left, RuntimeValue right)
		{
			var lseq = (ElaList)left.Ref;
			var rseq = (ElaList)right.Ref;
			var list = rseq;

			foreach (var e in lseq.Reverse())
				list = new ElaList(list, e);

			return new RuntimeValue(list);
		}
		#endregion


		#region Exceptions
		private void ExecuteThrow(ElaRuntimeError errorType, WorkerThread thread, params object[] args)
		{
			var errObj = default(RuntimeValue);
			var err = String.Empty;

			if (errorType == ElaRuntimeError.None && thread.EvalStack.Count > 0)
			{
				errObj = thread.EvalStack.Pop();

				if (errObj.Ref is ElaError)
				{
					var ee = (ElaError)errObj.Ref;
					errorType = ee.Code;
					err = ee.Message;
				}
				else
					err = errObj.ToString();
			}
			else
				err = Strings.GetError(errorType, args);

			var callStack = thread.CallStack;
			var cm = default(int?);
			var i = 0;

			do
				cm = callStack[callStack.Count - (++i)].CatchMark;
			while (cm == null && i < callStack.Count);

			if (cm == null)
			{
				thread.EvalStack.Clear(0);
				throw CreateException(err, errorType, thread, args);
			}
			else
			{
				var c = 1;

				while (c++ < i)
					callStack.Pop();

				thread.EvalStack.Clear(callStack.Peek().StackOffset);

				if (errorType == ElaRuntimeError.None)
					thread.EvalStack.Push(errObj);
				else
				{
					var tab = new ElaError(errorType, err);
					thread.EvalStack.Push(new RuntimeValue(tab));
				}

				thread.Offset = cm.Value;
			}
		}


		private ElaCodeException CreateException(string err, ElaRuntimeError errorType, WorkerThread thread, object[] args)
		{
			thread.Busy = false;
			var deb = new ElaDebugger(asm);
			var cs = deb.BuildCallStack(thread);

			while (thread.CallStack.Count > 1)
				thread.CallStack.Pop();

			return new ElaCodeException(err, errorType, cs.File, cs.Line, cs.Column, cs);
		}
		#endregion


		#region Properties
		public CodeAssembly Assembly { get { return asm; } }
		
		internal FastList<WorkerThread> Threads { get; private set; }

		internal WorkerThread MainThread { get; private set; }
		#endregion
	}
}