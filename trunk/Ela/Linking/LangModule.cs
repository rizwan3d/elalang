using System;
using System.Collections.Generic;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Linking
{
    #region Functions
    internal abstract class LangBinaryFun : ElaFunction
    {
        protected LangBinaryFun() : base(2)
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
                var fun = CloneFast();
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
        protected LangUnaryFun() : base(1)
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

    internal sealed class AddFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Add(arg1, arg2, ctx);
        }
    }

    internal sealed class SubFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Subtract(arg1, arg2, ctx);
        }
    }

    internal sealed class MulFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Multiply(arg1, arg2, ctx);
        }
    }

    internal sealed class DivFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Divide(arg1, arg2, ctx);
        }
    }

    internal sealed class RemFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Remainder(arg1, arg2, ctx);
        }
    }

    internal sealed class PowFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Power(arg1, arg2, ctx);
        }
    }

    internal sealed class NegFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.Negate(ctx);
        }
    }

    internal sealed class MinFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.GetMin(ctx);
        }
    }

    internal sealed class MaxFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.GetMax(ctx);
        }
    }

    internal sealed class BitAndFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.BitwiseAnd(arg1, arg2, ctx);
        }
    }

    internal sealed class BitOrFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.BitwiseOr(arg1, arg2, ctx);
        }
    }

    internal sealed class BitXorFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.BitwiseXor(arg1, arg2, ctx);
        }
    }

    internal sealed class BitNotFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.BitwiseNot(ctx);
        }
    }

    internal sealed class ShlFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.ShiftLeft(arg1, arg2, ctx);
        }
    }

    internal sealed class ShrFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.ShiftRight(arg1, arg2, ctx);
        }
    }

    internal sealed class EqFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Equal(arg1, arg2, ctx);
        }
    }

    internal sealed class NeqFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.NotEqual(arg1, arg2, ctx);
        }
    }

    internal sealed class GteqFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.GreaterEqual(arg1, arg2, ctx);
        }
    }

    internal sealed class LteqFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.LesserEqual(arg1, arg2, ctx);
        }
    }

    internal sealed class LtFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Lesser(arg1, arg2, ctx);
        }
    }

    internal sealed class GtFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Greater(arg1, arg2, ctx);
        }
    }

    internal sealed class NotFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return new ElaValue(!arg.Ref.Bool(arg, ctx));
        }
    }

    internal sealed class ConsFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg2.Cons(arg2.Ref, arg1, ctx);
        }
    }

    internal sealed class CatFun : LangBinaryFun
    {
        protected internal override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
        {
            return arg1.Concatenate(arg1, arg2, ctx);
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

    internal sealed class NilFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.Ref.Nil(ctx);
        }
    }

    internal sealed class IsNilFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return new ElaValue(arg.Ref.IsNil(ctx));
        }
    }

    internal sealed class HeadFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.Ref.Head(ctx);
        }
    }

    internal sealed class TailFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.Ref.Tail(ctx);
        }
    }

    internal sealed class LenFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.Ref.GetLength(ctx);
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

    internal sealed class FlipFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            arg = arg.Force(ctx);

            if (arg.TypeId != ElaMachine.FUN)
            {
                ctx.Fail(new ElaError(ElaRuntimeError.ExpectedFunction, TypeCodeFormat.GetShortForm(arg.TypeCode)));
                return Default();
            }

            var fun = (ElaFunction)arg.Ref;
            fun = fun.Captures != null ? fun.CloneFast() : fun.Clone();
            fun.Flip = !fun.Flip;
            return new ElaValue(fun);
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
                if (!res.Equal(res, new ElaValue(len), ctx).Bool(ctx))
                {
                    if (!ctx.Failed)
                        ctx.Fail(ElaRuntimeError.MatchFailed);

                    return Default();
                }

                return arg.Ref.GetValue(new ElaValue(elem), ctx);
            }

            return Default();
        }
    }

    internal sealed class ForceFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.Ref.Force(arg, ctx);
        }
    }

    internal sealed class TypeFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return new ElaValue(arg.Ref.GetTypeInfo());
        }
    }

    internal sealed class GettagFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return new ElaValue(arg.Ref.GetTag(ctx));
        }
    }

    internal sealed class UntagFun : LangUnaryFun
    {
        protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
        {
            return arg.Ref.Untag(ctx);
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
            AddFastCall2("add", new AddFun());
            AddFastCall2("sub", new SubFun());
            AddFastCall2("mul", new MulFun());
            AddFastCall2("div", new DivFun());
            AddFastCall2("rem", new RemFun());
            AddFastCall2("pow", new PowFun());
            AddFastCall1("neg", new NegFun());
            AddFastCall1("min", new MinFun());
            AddFastCall1("max", new MaxFun());
            AddFastCall2("bitand", new BitAndFun());
            AddFastCall2("bitor", new BitOrFun());
            AddFastCall2("bitxor", new BitXorFun());
            AddFastCall2("bitnot", new BitNotFun());
            AddFastCall2("shl", new ShlFun());
            AddFastCall2("shr", new ShrFun());
            AddFastCall2("eq", new EqFun());
            AddFastCall2("neq", new NeqFun());
            AddFastCall2("gteq", new GteqFun());
            AddFastCall2("lteq", new LteqFun());
            AddFastCall2("lt", new LtFun());
            AddFastCall2("gt", new GtFun());
            AddFastCall1("_not", new NotFun());
            AddFastCall2("cons", new ConsFun());
            AddFastCall2("cat", new CatFun());
            AddFastCall1("succ", new SuccFun());
            AddFastCall1("pred", new PredFun());
            AddFastCall1("_nil", new NilFun());
            AddFastCall1("isnil", new IsNilFun());
            AddFastCall1("_head", new HeadFun());
            AddFastCall1("_tail", new TailFun());
            AddFastCall1("len", new LenFun());
            AddFastCall2("_showf", new ShowfFun());
            AddFastCall1("_flip", new FlipFun());
            AddFastCall1("elem1of2", new ElemFun(0, 2));
            AddFastCall1("elem2of2", new ElemFun(1, 2));
            AddFastCall1("elem1of3", new ElemFun(0, 3));
            AddFastCall1("elem2of3", new ElemFun(1, 3));
            AddFastCall1("_force", new ForceFun());
            AddFastCall1("_type", new TypeFun());
            AddFastCall1("gettag", new GettagFun());
            AddFastCall1("_untag", new UntagFun());
            AddFastCall2("conv", new ConvertFun());
        }


        
    }
}
