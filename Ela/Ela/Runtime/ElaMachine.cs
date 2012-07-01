using System;
using System.Collections.Generic;
using System.IO;
using Ela.Compilation;
using Ela.Debug;
using Ela.Linking;
using Ela.Runtime.ObjectModel;
using Ela.Runtime.Classes;
using Ela.CodeModel;

namespace Ela.Runtime
{
    public sealed class ElaMachine : IDisposable
    {
        #region Construction
        internal ElaValue[][] modules;
        private readonly CodeAssembly asm;
        private int lastOffset;

        public ElaMachine(CodeAssembly asm)
        {
            this.asm = asm;
            var frame = asm.GetRootModule();
            MainThread = new WorkerThread(asm);
            var lays = frame.Layouts[0];
            modules = new ElaValue[asm.ModuleCount][];
            var mem = new ElaValue[lays.Size];
            modules[0] = mem;
            MainThread.CallStack.Push(new CallPoint(0, new EvalStack(lays.StackSize), mem, FastList<ElaValue[]>.Empty));
            BuildFunTable();
        }
        #endregion


        #region InternalTypes
        internal const int UNI = (Int32)ElaTypeCode.Unit;
        internal const int INT = (Int32)ElaTypeCode.Integer;
        internal const int REA = (Int32)ElaTypeCode.Single;
        internal const int BYT = (Int32)ElaTypeCode.Boolean;
        internal const int CHR = (Int32)ElaTypeCode.Char;
        internal const int LNG = (Int32)ElaTypeCode.Long;
        internal const int DBL = (Int32)ElaTypeCode.Double;
        internal const int STR = (Int32)ElaTypeCode.String;
        internal const int LST = (Int32)ElaTypeCode.List;
        internal const int RES = (Int32)ElaTypeCode.__Reserved;
        internal const int TUP = (Int32)ElaTypeCode.Tuple;
        internal const int REC = (Int32)ElaTypeCode.Record;
        internal const int FUN = (Int32)ElaTypeCode.Function;
        internal const int OBJ = (Int32)ElaTypeCode.Object;
        internal const int MOD = (Int32)ElaTypeCode.Module;
        internal const int LAZ = (Int32)ElaTypeCode.Lazy;
        internal const int VAR = (Int32)ElaTypeCode.Variant;
        internal const int TYP = (Int32)ElaTypeCode.TypeInfo;

        private Class[] cls;
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


        public string PrintValue(ElaValue val)
        {
            if (val.Ref == null)
                return "_|_";

            if (val.TypeCode == ElaTypeCode.Lazy)
                val = ((ElaLazy)val.Ref).Force();

            var ctx = new ExecutionContext();
            var ret = cls[val.TypeId].Showf(String.Empty, val, ctx);

            if (ctx.Failed && ctx.Fun == null)
                return val.ToString();
            else if (ctx.Failed)
            {
                try
                {
                    var rval = ctx.Fun.Call(new ElaValue(String.Empty), val);
                    return rval.DirectGetString();
                }
                catch
                {
                    return val.ToString();
                }
            }
            else
                return ret;
        }


        public ExecutionResult Run()
        {
            MainThread.Offset = MainThread.Offset == 0 ? 0 : MainThread.Offset;
            lastOffset = MainThread.Module.Ops.Count;
            var ret = default(ElaValue);

            try
            {
                ret = Execute(MainThread);
            }
            catch (ElaException)
            {
                throw;
            }
#if !DEBUG
            catch (NullReferenceException ex)
            {
                var err = new ElaError(ElaRuntimeError.BottomReached);
                throw CreateException(Dump(err, MainThread), ex);
            }
#endif
            catch (OutOfMemoryException)
            {
                throw Exception("OutOfMemory");
            }
#if !DEBUG
            catch (Exception ex)
            {
                var op = MainThread.Module != null && MainThread.Offset > 0 &&
                    MainThread.Offset - 1 < MainThread.Module.Ops.Count ?
                    MainThread.Module.Ops[MainThread.Offset - 1].ToString() : String.Empty;
                throw Exception("CriticalError", ex, MainThread.Offset - 1, op);
            }
#endif

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


        public ExecutionResult Resume()
        {
            RefreshState();
            return Run(lastOffset);
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
            }

            if (asm != null)
                BuildFunTable();
        }


        private void BuildFunTable()
        {
            cls = asm.Cls.ToArray();
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
                    case Op.Pushext:
                        evalStack.Push(modules[frame.HandleMap[opd & Byte.MaxValue]][opd >> 8]);
                        break;
                    case Op.Pushloc:
                        evalStack.Push(locals[opd]);
                        break;
                    case Op.Pushvar:
                        evalStack.Push(captures[captures.Count - (opd & Byte.MaxValue)][opd >> 8]);
                        break;
                    case Op.Pushstr:
                        evalStack.Push(new ElaValue(frame.Strings[opd]));
                        break;
                    case Op.Pushstr_0:
                        evalStack.Push(new ElaValue(ElaString.Empty));
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
                        left = evalStack.Pop();
                        right = evalStack.Peek();

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].GetValue(left, right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            evalStack.Push(left);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Pushfld:
                        {
                            var fld = frame.Strings[opd];
                            right = evalStack.PopFast();

                            if (right.TypeId == LAZ)
                                right = right.Ref.Force(right, ctx);

                            evalStack.Push(cls[right.TypeId].GetValue(right, new ElaValue(fld), ctx));

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
                    case Op.Poploc:
                        locals[opd] = evalStack.Pop();
                        break;
                    case Op.Popvar:
                        captures[captures.Count - (opd & Byte.MaxValue)][opd >> 8] = evalStack.Pop();
                        break;
                    case Op.Dup:
                        evalStack.Dup();
                        break;
                    case Op.Swap:
                        right = evalStack.PopFast();
                        left = evalStack.Peek();
                        evalStack.Replace(right);
                        evalStack.Push(left);
                        break;
                    #endregion

                    #region Object Operations
                    case Op.Show:
                        {
                            left = evalStack.Pop();
                            right = evalStack.Pop();
                            left = left.Ref.Force(left, ctx);

                            if (left.TypeId != STR)
                            {
                                InvalidType(left, thread, evalStack, TypeCodeFormat.GetShortForm(ElaTypeCode.String));
                                goto SWITCH_MEM;
                            }

                            if (right.TypeId == LAZ)
                                right = right.Ref.Force(right, ctx);

                            if (ctx.Failed)
                            {
                                evalStack.Push(right);
                                evalStack.Push(left);
                                ExecuteFail(ctx.Error, thread, evalStack);
                                goto SWITCH_MEM;
                            }

                            evalStack.Push(new ElaValue(cls[right.TypeId].Showf(left.DirectGetString(), right, ctx)));

                            if (ctx.Failed)
                            {
                                evalStack.Replace(right);
                                evalStack.Push(left);
                                ExecuteFail(ctx.Error, thread, evalStack);
                                goto SWITCH_MEM;
                            }
                        }
                        break;
                    case Op.Read:
                        {
                            var fmt = evalStack.Pop();
                            right = evalStack.Pop();
                            left = evalStack.Pop();

                            if (left.TypeId == LAZ || right.TypeId == LAZ || fmt.TypeId == LAZ)
                            {
                                left = left.Ref.Force(left, ctx);
                                right = right.Ref.Force(right, ctx);
                                fmt = fmt.Ref.Force(fmt, ctx);
                            }

                            if (fmt.TypeId != STR)
                            {
                                InvalidType(fmt, thread, evalStack, TypeCodeFormat.GetShortForm(ElaTypeCode.String));
                                goto SWITCH_MEM;
                            }

                            if (left.TypeId != STR)
                            {
                                InvalidType(left, thread, evalStack, TypeCodeFormat.GetShortForm(ElaTypeCode.String));
                                goto SWITCH_MEM;
                            }

                            if (right.TypeId != TYP)
                            {
                                InvalidType(left, thread, evalStack, TypeCodeFormat.GetShortForm(ElaTypeCode.TypeInfo));
                                goto SWITCH_MEM;
                            }

                            evalStack.Push(cls[((ElaTypeInfo)right.Ref).ReflectedTypeCode].Parse(right, fmt.DirectGetString(), left.DirectGetString(), ctx));

                            if (ctx.Failed)
                            {
                                evalStack.PopVoid();
                                evalStack.Push(left);

                                if (ctx.Fun == null)
                                    evalStack.Push(right);

                                evalStack.Push(fmt);
                                ExecuteFail(ctx.Error, thread, evalStack);
                                goto SWITCH_MEM;
                            }
                        }
                        break;
                    case Op.Reccons:
                        {
                            right = evalStack.Pop();
                            left = evalStack.Pop();
                            var rec = (ElaRecord)evalStack.Peek().Ref;
                            rec.SetField(opd, right.DirectGetString(), left);
                        }
                        break;
                    case Op.Recfld:
                        {
                            left = evalStack.Pop();
                            right = evalStack.Pop();

                            if (left.TypeId == LAZ || right.TypeId == LAZ)
                            {
                                left = left.Ref.Force(left, ctx);
                                right = right.Ref.Force(right, ctx);
                            }
                            if (left.TypeId != REC)
                                ctx.InvalidType(TypeCodeFormat.GetShortForm(ElaTypeCode.Record), left);

                            if (!ctx.Failed)
                                evalStack.Push(((ElaRecord)left.Ref).GetKey(right, ctx));

                            if (ctx.Failed)
                            {
                                evalStack.Push(right);
                                evalStack.Push(left);
                                ExecuteFail(ctx.Error, thread, evalStack);
                                goto SWITCH_MEM;
                            }
                        }
                        break;
                    case Op.Tupcons:
                        right = evalStack.Pop();
                        left = evalStack.Peek();
                        ((ElaTuple)left.Ref).InternalSetValue(opd, right);
                        break;
                    case Op.Cons:
                        left = evalStack.Pop();
                        right = evalStack.Peek();
                        evalStack.Replace(right.Ref.Cons(left, ctx));

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
                        right = evalStack.Peek();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(cls[right.TypeId].Tail(right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Head:
                        right = evalStack.Peek();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(cls[right.TypeId].Head(right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Isnil:
                        right = evalStack.Peek();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(new ElaValue(cls[right.TypeId].IsNil(right, ctx)));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Len:
                        right = evalStack.Peek();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(cls[right.TypeId].GetLength(right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }

                        break;
                    case Op.Force:
                        if (evalStack.Peek().TypeId == LAZ)
                        {
                            right = evalStack.Peek();
                            evalStack.Replace(right.Ref.Force(right, ctx));

                            if (ctx.Failed)
                            {
                                evalStack.Replace(right);
                                ExecuteThrow(thread, evalStack);
                                goto SWITCH_MEM;
                            }
                        }
                        break;
                    case Op.Untag:
                        right = evalStack.Peek();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(right.Ref.Untag(ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Gettag:
                        right = evalStack.Peek();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(new ElaValue(right.Ref.GetTag(ctx)));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    #endregion

                    #region Goto Operations
                    case Op.Skiphtag:
                        right = evalStack.Pop();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

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

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        if (frame.Strings[opd] == right.Ref.GetTag(ctx))
                        {
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
                        right = evalStack.PopFast();

                        if (right.Ref.True(right, ctx))
                            thread.Offset = opd;

                        if (ctx.Failed)
                        {
                            evalStack.Push(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Brfalse:
                        right = evalStack.PopFast();

                        if (right.Ref.False(right, ctx))
                            thread.Offset = opd;

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
                    #endregion

                    #region Binary Operations
                    case Op.AndBw:
                        left = evalStack.Pop();
                        right = evalStack.Peek();

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].BitwiseAnd(left, right, ctx));

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

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].BitwiseOr(left, right, ctx));

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

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].BitwiseXor(left, right, ctx));

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

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].ShiftLeft(left, right, ctx));

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

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].ShiftRight(left, right, ctx));

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

                        if (left.TypeId == LAZ)
                            left = left.Ref.Force(left, ctx);

                        evalStack.Replace(cls[left.TypeId].Concatenate(left, right, ctx));

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

                        if (left.Ref.TypeId == LAZ || right.Ref.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].Add(left, right, ctx));

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

                        if (left.Ref.TypeId == LAZ || right.Ref.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].Subtract(left, right, ctx));

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

                        if (left.Ref.TypeId == LAZ || right.Ref.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].Divide(left, right, ctx));

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

                        if (left.Ref.TypeId == LAZ || right.Ref.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].Multiply(left, right, ctx));

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

                        if (left.Ref.TypeId == LAZ || right.Ref.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].Power(left, right, ctx));

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

                        if (left.Ref.TypeId == LAZ || right.Ref.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].Remainder(left, right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            evalStack.Push(left);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Cgt:
                        left = evalStack.Pop();
                        right = evalStack.Peek();

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].Greater(left, right, ctx));

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

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].Lesser(left, right, ctx));

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

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].Equal(left, right, ctx));

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

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].NotEqual(left, right, ctx));

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

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].GreaterEqual(left, right, ctx));

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

                        if (left.TypeId == LAZ || right.TypeId == LAZ)
                        {
                            left = left.Ref.Force(left, ctx);
                            right = right.Ref.Force(right, ctx);
                        }

                        evalStack.Replace(cls[left.TypeId].LesserEqual(left, right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            evalStack.Push(left);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    #endregion

                    #region Unary Operations
                    case Op.Succ:
                        right = evalStack.Peek();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(cls[right.TypeId].Successor(right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Pred:
                        right = evalStack.Peek();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(cls[right.TypeId].Predecessor(right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Cast:
                        {
                            left = evalStack.Pop();
                            right = evalStack.Pop();

                            if (left.TypeId == LAZ || right.TypeId == LAZ)
                            {
                                left = left.Ref.Force(left, ctx);
                                right = right.Ref.Force(right, ctx);
                            }

                            if (!ctx.Failed)
                            {
                                evalStack.Push(Cast(left, right, thread, evalStack, ctx));

                                if (!ctx.Failed)
                                    break;

                                evalStack.PopVoid();
                            }

                            evalStack.Push(right);
                            evalStack.Push(left);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                    case Op.Not:
                        right = evalStack.Peek();
                        right = right.Ref.Force(right, ctx);

                        if (right.TypeId == BYT)
                        {
                            evalStack.Replace(right.I4 != 1);
                            break;
                        }
                        else if (right.TypeId != LAZ)
                        {
                            InvalidType(right, thread, evalStack, "bool");
                            goto SWITCH_MEM;
                        }

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.Neg:
                        right = evalStack.Peek();

                        if (right.Ref.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(cls[right.TypeId].Negate(right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
                            ExecuteThrow(thread, evalStack);
                            goto SWITCH_MEM;
                        }
                        break;
                    case Op.NotBw:
                        right = evalStack.Peek();

                        if (right.TypeId == LAZ)
                            right = right.Ref.Force(right, ctx);

                        evalStack.Replace(cls[right.TypeId].BitwiseNot(right, ctx));

                        if (ctx.Failed)
                        {
                            evalStack.Replace(right);
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
                    case Op.Newfunc:
                        {
                            var fn = new ElaFunTable(frame.Strings[opd], evalStack.Pop().I4, evalStack.Pop().I4, 0);
                            evalStack.Push(new ElaValue(fn));
                        }
                        break;
                    case Op.Addmbr:
                        {
                            var typ = evalStack.Pop().I4;
                            left = evalStack.Pop();
                            right = evalStack.Pop();

                            if (right.TypeId != FUN)
                            {
                                InvalidType(right, thread, evalStack, TypeCodeFormat.GetShortForm(ElaTypeCode.Function));
                                goto SWITCH_MEM;
                            }

                            if (left.TypeId == INT)
                                cls[typ].AddFunction((ElaBuiltinKind)left.I4, (ElaFunction)right.Ref);
                            else
                                ((ElaFunTable)left.Ref).AddFunction(typ, (ElaFunction)right.Ref);
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
                            var tup = new ElaTuple(2);
                            tup.InternalSetValue(0, left);
                            tup.InternalSetValue(1, right);
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
                            right = evalStack.Peek();

                            if (right.TypeId == LAZ)
                                right = right.Ref.Force(right, ctx);

                            if (ctx.Failed)
                            {
                                ExecuteThrow(thread, evalStack);
                                goto SWITCH_MEM;
                            }

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
                            evalStack.Push(new ElaValue(-1, new ElaLazy(fun)));
                        }
                        break;
                    case Op.Callt:
                        {
                            var funObj = evalStack.Peek().Ref;

                            if (callStack.Peek().Thunk != null || funObj.TypeId == LAZ)// || funObj.IsExternFun())
                            {
                                if (Call(evalStack.Pop().Ref, thread, evalStack, CallFlag.None))
                                    goto SWITCH_MEM;
                                break;
                            }

                            var cp = callStack.Pop();
                            evalStack.PopVoid();

                            if (Call(funObj, thread, evalStack, CallFlag.NoReturn))
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

                                if (cc.Thunk.Value.TypeId == LAZ)
                                {
                                    cc.Thunk.Value = cc.Thunk.Value.Ref.Force(cc.Thunk.Value, ctx);

                                    if (ctx.Failed)
                                    {
                                        thread.Offset++;
                                        ExecuteThrow(thread, evalStack);
                                        goto SWITCH_MEM;
                                    }
                                }

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

                    #region Misc
                    case Op.Nop:
                        break;
                    case Op.Newtype:
                        {
                            var tid = asm.Types[thread.Module.InternalTypes[frame.Strings[opd]]];
                            evalStack.Replace(new ElaValue(new ElaUserType(tid.TypeName, tid.TypeCode, evalStack.Peek())));
                        }
                        break;
                    case Op.Traitch:
                        {
                            left = evalStack.Pop();
                            right = evalStack.Pop();
                            long cc = (Int64)left.I4;
                            long tc = (Int64)right.TypeId << 32;
                            evalStack.Push(Assembly.Instances.ContainsKey(cc | tc));
                        }
                        break;
                    case Op.Typeid:
                        evalStack.Push(thread.Module.InternalTypes[frame.Strings[opd]]);
                        break;
                    case Op.Classid:
                        evalStack.Push(thread.Module.InternalClasses[frame.Strings[opd]].Code);
                        break;
                    case Op.Type:
                        {
                            right = evalStack.Peek();

                            if (right.TypeId == LAZ && ((ElaLazy)right.Ref).Value.Ref != null)
                                right = right.Ref.Force(right, ctx);

                            evalStack.Replace(new ElaValue(new ElaTypeInfo(asm.Types[right.Ref.TypeId])));
                        }
                        break;
                    case Op.Throw:
                        {
                            left = evalStack.Pop();
                            right = evalStack.Pop();
                            var code = left.ToString();
                            var msg = right.ToString();

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
                evalStack = mem.Stack;
            }
            goto CYCLE;

        }
        #endregion


        #region Operations
        private ElaValue Cast(ElaValue codeValue, ElaValue value, WorkerThread thread, EvalStack stack, ExecutionContext ctx)
        {
            if (codeValue.TypeId != INT)
            {
                InvalidType(codeValue, thread, stack, TypeCodeFormat.GetShortForm(ElaTypeCode.Integer));
                return new ElaValue(ElaObject.ElaInvalidObject.Instance);
            }

            var code = codeValue.I4;

            if (code == value.TypeId)
                return value;

            if (value.Ref is ElaUserType)
            {
                var ut = (ElaUserType)value.Ref;

                if (ut.Value.TypeId == code)
                    return ut.Value;
            }

            if (code == CHR && value.TypeId == INT)
                return new ElaValue((Char)value.I4);

            else if (code == INT && value.TypeId == LNG)
                return new ElaValue((Int32)value.Ref.AsLong());
            else if (code == INT && value.TypeId == REA)
                return new ElaValue((Int32)value.DirectGetReal());
            else if (code == INT && value.TypeId == DBL)
                return new ElaValue((Int32)value.Ref.AsDouble());
            else if (code == INT && value.TypeId == CHR)
                return new ElaValue(value.I4);
            else if (code == INT && value.TypeId == BYT)
                return new ElaValue(value.I4);

            else if (code == REA && value.TypeId == INT)
                return new ElaValue((Single)value.I4);
            else if (code == REA && value.TypeId == LNG)
                return new ElaValue((Single)value.Ref.AsLong());
            else if (code == REA && value.TypeId == DBL)
                return new ElaValue((Single)value.Ref.AsDouble());

            else if (code == LNG && value.TypeId == INT)
                return new ElaValue((Int64)value.I4);
            else if (code == LNG && value.TypeId == REA)
                return new ElaValue((Int64)value.DirectGetReal());
            else if (code == LNG && value.TypeId == DBL)
                return new ElaValue((Int64)value.Ref.AsDouble());
            else if (code == LNG && value.TypeId == CHR)
                return new ElaValue((Int64)value.I4);
            else if (code == INT && value.TypeId == BYT)
                return new ElaValue((Int64)value.I4);

            else if (code == DBL && value.TypeId == INT)
                return new ElaValue((Double)value.I4);
            else if (code == DBL && value.TypeId == LNG)
                return new ElaValue((Double)value.Ref.AsLong());
            else if (code == DBL && value.TypeId == REA)
                return new ElaValue((Double)value.DirectGetReal());

            else if (code == STR && value.TypeId == CHR)
                return new ElaValue(((Char)value.I4).ToString());

            else if (code == BYT && value.TypeId == INT)
                return new ElaValue(value.I4 == 1);
            else if (code == BYT && value.TypeId == LNG)
                return new ElaValue(value.Ref.AsLong() == 1);

            ctx.ConversionFailed(value, TypeCodeFormat.GetShortForm((ElaTypeCode)code));
            return new ElaValue(ElaObject.ElaInvalidObject.Instance);
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

            for (var i = 0; i < len; i++)
                stack.Push(args[len - i - 1]);

            if (fun.AppliedParameters > 0)
            {
                for (var i = 0; i < fun.AppliedParameters; i++)
                    stack.Push(fun.Parameters[fun.AppliedParameters - i - 1]);
            }

            if (len + fun.AppliedParameters != fun.Parameters.Length + 1)
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
                var rf = fun.Force(new ElaValue(fun), thread.Context).Ref;

                if (rf.TypeId == FUN)
                {
                    fun = rf;
                    goto goon;
                }

                if (thread.Context.Failed)
                {
                    stack.Push(new ElaValue(fun));
                    ExecuteThrow(thread, stack);
                    return true;
                }

                InvalidType(new ElaValue(rf), thread, stack, "fun");
                return true;
            }

        goon:

            var natFun = (ElaFunction)fun;

            if (natFun.table)
            {
                natFun = ((ElaFunTable)fun).GetFunction(stack.Peek(), thread.Context);

                if (thread.Context.Failed)
                {
                    stack.Push(new ElaValue(fun));
                    ExecuteThrow(thread, stack);
                    return true;
                }
            }

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
                return CallExternal(thread, stack, natFun);
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


        private bool CallExternal(WorkerThread thread, EvalStack stack, ElaFunction funObj)
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
                thread.LastException = ex;
                ExecuteFail(new ElaError(ex.Category, ex.Message), thread, stack);
                return true;
            }
            catch (Exception ex)
            {
                thread.LastException = ex;
                ExecuteFail(new ElaError(ElaRuntimeError.CallFailed, ex.Message), thread, stack);
                return true;
            }

            return false;
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
            else if (thread.Context.Fun != null)
            {
                thread.Context.Failed = false;
                var f = thread.Context.Fun.Clone();
                thread.Context.Fun = null;
                var args = thread.Context.DefferedArgs;

                if (args == 1)
                    f.LastParameter = stack.Pop();
                if (args == 2)
                {
                    f.Parameters[0] = stack.Pop();
                    f.AppliedParameters = 1;
                    f.LastParameter = stack.Pop();
                }

                if (f.Parameters.Length + 1 > args)
                {
                    f.Parameters[f.AppliedParameters] = f.LastParameter;
                    f.LastParameter = default(ElaValue);
                    f.AppliedParameters++;
                    stack.Push(new ElaValue(f));
                    return;
                }

                Call(f, thread, stack, CallFlag.AllParams);
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
                throw CreateException(Dump(err, thread), thread.LastException);
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


        private ElaCodeException CreateException(ElaError err, Exception ex)
        {
            var deb = new ElaDebugger(asm);
            var mod = asm.GetModule(err.Module);
            var cs = deb.BuildCallStack(err.CodeOffset, mod, mod.File, err.Stack);

            var fi = cs.File;

            if (fi != null && StringComparer.OrdinalIgnoreCase.Equals(fi.Extension, ".elaobj"))
            {
                var nfi = new FileInfo(fi.FullName.Replace(fi.Extension, ".ela"));

                if (nfi.Exists)
                    fi = nfi;
            }

            return new ElaCodeException(err.FullMessage.Replace("\0", ""), err.Code, fi, cs.Line, cs.Column, cs, err, ex);
        }


        private ElaMachineException Exception(string message, Exception ex, params object[] args)
        {
            return new ElaMachineException(Strings.GetMessage(message, args), ex);
        }


        private ElaMachineException Exception(string message)
        {
            return Exception(message, null);
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
        #endregion


        #region Properties
        public CodeAssembly Assembly { get { return asm; } }

        internal WorkerThread MainThread { get; private set; }
        #endregion
    }
}