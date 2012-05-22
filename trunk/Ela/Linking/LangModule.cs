using System;
using System.Collections.Generic;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Linking
{
    #region Functions
    internal abstract class LangBinaryFun : ElaFunction
    {
        protected LangBinaryFun()
            : base(2)
        {
            Spec = 2;
        }

        public override ElaValue Call(params ElaValue[] args)
        {
            var ctx = new ExecutionContext();

            var arg1 = args[0];

            if (arg1.TypeCode == ElaTypeCode.Lazy)
                arg1 = ((ElaLazy)arg1.Ref).Force();

            var arg2 = args[1];

            if (arg2.TypeCode == ElaTypeCode.Lazy)
                arg2 = ((ElaLazy)arg1.Ref).Force();

            var res = Call(arg1, arg2, ctx);

            if (ctx.Failed)
                throw new ElaRuntimeException(ctx.Error.FullMessage);

            return res;
        }

        protected internal override ElaValue Call(ElaValue value, ExecutionContext ctx)
        {
            if (AppliedParameters == 1)
                return Call(Parameters[0], value, ctx);
            else
            {
                var fun = Clone();
                fun.AppliedParameters = 1;
                fun.Parameters[0] = value;
                return new ElaValue(fun);
            }
        }

        protected override string GetFunctionName()
        {
            return GetType().Name.Replace("Fun", String.Empty).ToLower();
        }
    }

    internal abstract class LangUnaryFun : ElaFunction
    {
        protected LangUnaryFun()
            : base(1)
        {
            Spec = 1;
        }

        public override ElaValue Call(params ElaValue[] args)
        {
            var ctx = new ExecutionContext();
            var arg = args[0];

            if (arg.TypeCode == ElaTypeCode.Lazy)
                arg = ((ElaLazy)arg.Ref).Force();

            var res = Call(arg, ctx);

            if (ctx.Failed)
                throw new ElaRuntimeException(ctx.Error.FullMessage);

            return res;
        }

        protected override string GetFunctionName()
        {
            return GetType().Name.Replace("Fun", String.Empty).ToLower();
        }
    }

    internal sealed class SuccFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.Ref.Successor(arg, ctx);
        }
    }

    internal sealed class PredFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.Ref.Predecessor(arg, ctx);
        }
    }

    internal sealed class ShowfFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            var str = arg1.Convert(ElaString.TypeInfo, ctx);

            if (!ctx.Failed)
                return new ElaValue(arg2.Show(new ShowInfo(0, 0, str.ToString()), ctx));
            else
                return Default();
        }
    }

    internal sealed class ElemFun : LangUnaryFun
    {
        private readonly int elem;
        private readonly int len;

        internal ElemFun(int elem, int len)
        {
            this.elem = elem;
            this.len = len;
        }

        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            var res = arg.Ref.GetLength(ctx);

            if (!ctx.Failed)
            {
                if (!res.Equal(res, new ElaValue(len), ctx))
                {
                    if (!ctx.Failed)
                        ctx.Fail(ElaRuntimeError.MatchFailed);

                    return Default();
                }

                return arg.Ref.GetValue(new ElaValue(elem), ctx);
            }

            return Default();
        }

        public override ElaFunction Clone()
        {
            return CloneFast(new ElemFun(elem, len));
        }
    }

    internal sealed class ConvertFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (left.TypeId == ElaMachine.INT)
                return Convert(right, (ElaTypeCode)left.I4, ctx);
            else
            {
                var fv = left.Force(ctx);

                if (!ctx.Failed)
                    return right.Ref.Convert(right, (ElaTypeInfo)fv.Ref, ctx);

                return Default();
            }
        }

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
    }
    #endregion


    internal sealed class LangModule : ForeignModule
    {
        public override void Initialize()
        {
            Add("_succ", new SuccFun());
            Add("_pred", new PredFun());
            Add("_showf", new ShowfFun());
            Add("_elem1of2", new ElemFun(0, 2));
            Add("_elem2of2", new ElemFun(1, 2));
            Add("_elem1of3", new ElemFun(0, 3));
            Add("_elem2of3", new ElemFun(1, 3));
            Add("_convert", new ConvertFun());
        }
    }
}
