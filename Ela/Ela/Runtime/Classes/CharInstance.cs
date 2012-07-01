using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class CharInstance : Class
    {
        internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            right = right.Ref.Force(right, ctx);
            
            if (right.TypeId != ElaMachine.CHR)
            {
                if (right.TypeId == ElaMachine.STR)
                    return new ElaValue(left.ToString() + right.DirectGetString());

                ctx.InvalidOperand(left, right, "concatenate");
                return Default();
            }

            return new ElaValue(((Char)left.I4).ToString() + (Char)right.I4);
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
                ctx.InvalidOperand(left, right, "equal");
                return false;
            }

            return left.I4 == right.I4;
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidOperand(left, right, "notequal");
                return false;
            }

            return left.I4 != right.I4;
        }

        internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidOperand(left, right, "greater");
                return false;
            }

            return left.I4 > right.I4;
        }

        internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidOperand(left, right, "lesser");
                return false;
            }

            return left.I4 < right.I4;
        }

        internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidOperand(left, right, "greaterequal");
                return false;
            }

            return left.I4 >= right.I4;
        }

        internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.CHR)
            {
                ctx.InvalidOperand(left, right, "lesserequal");
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
    }
}
