using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class CharInstance : Class
    {
        internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                if (right.TypeId == ElaMachine.STR)
                    return new ElaValue(left.ToString() + right.DirectGetString());

                ctx.InvalidRightOperand(left, right, "concatenate");
                return Default();
            }

            return new ElaValue((Char)left.I4 + (Char)right.I4);
        }

        internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.I4 + 1, ElaChar.Instance);
        }

        internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(@this.I4 - 1, ElaChar.Instance);
        }

        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidRightOperand(left, right, "equal");
                return false;
            }

            return left.I4 == right.I4;
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidRightOperand(left, right, "notequal");
                return false;
            }

            return left.I4 != right.I4;
        }

        internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidRightOperand(left, right, "greater");
                return false;
            }

            return left.I4 > right.I4;
        }

        internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidRightOperand(left, right, "lesser");
                return false;
            }

            return left.I4 < right.I4;
        }

        internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidRightOperand(left, right, "greaterequal");
                return false;
            }

            return left.I4 >= right.I4;
        }

        internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidRightOperand(left, right, "lesserequal");
                return false;
            }

            return left.I4 <= right.I4;
        }

        internal override ElaValue Parse(ElaValue instance, string format, string value, ExecutionContext ctx)
        {
            if (String.IsNullOrEmpty(value))
                return new ElaValue('\0');

            return new ElaValue(value[0]);
        }

        internal override ElaValue CastTo(ElaTypeInfo typeInfo, ElaValue value, ExecutionContext ctx)
        {
            var tc = (ElaTypeCode)typeInfo.ReflectedTypeCode;

            switch (tc)
            {
                case ElaTypeCode.String:
                    return new ElaValue(new ElaString(((Char)value.I4).ToString()));
                case ElaTypeCode.Integer:
                    return new ElaValue(value.I4);
                default:
                    {
                        ctx.ConversionFailed(value, typeInfo.ReflectedTypeName);
                        return Default();
                    };
            }
        }
    }
}
