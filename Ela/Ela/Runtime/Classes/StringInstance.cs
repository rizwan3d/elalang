using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class StringInstance : Class
    {
        internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            right = right.Ref.Force(right, ctx);

            if (right.TypeId != ElaMachine.STR)
            {
                if (right.TypeId == ElaMachine.CHR)
                    return new ElaValue(left.DirectGetString() + right.ToString());

                ctx.InvalidOperand(left, right, "concatenate");
                return Default();
            }

            return new ElaValue(left.DirectGetString() + right.DirectGetString());
        }

        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.STR)
            {
                ctx.InvalidOperand(left, right, "equal");
                return false;
            }

            return left.DirectGetString() == right.DirectGetString();
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.STR)
            {
                ctx.InvalidOperand(left, right, "notequal");
                return false;
            }

            return left.DirectGetString() != right.DirectGetString();
        }

        internal override ElaValue GetLength(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(((ElaString)value.Ref).Value.Length);
        }

        internal override ElaValue GetValue(ElaValue value, ElaValue index, ExecutionContext ctx)
        {
            return ((ElaString)value.Ref).GetValue(index, ctx);
        }

        internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.STR)
            {
                ctx.InvalidOperand(left, right, "greater");
                return false;
            }

            return left.DirectGetString().CompareTo(right.DirectGetString()) > 0;
        }

        internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.STR)
            {
                ctx.InvalidOperand(left, right, "lesser");
                return false;
            }

            return left.DirectGetString().CompareTo(right.DirectGetString()) < 0;
        }

        internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.STR)
            {
                ctx.InvalidOperand(left, right, "greaterequal");
                return false;
            }

            return left.DirectGetString().CompareTo(right.DirectGetString()) >= 0;
        }

        internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.STR)
            {
                ctx.InvalidOperand(left, right, "lesserequal");
                return false;
            }

            return left.DirectGetString().CompareTo(right.DirectGetString()) <= 0;
        }

        internal override ElaValue Parse(ElaValue instance, string format, string value, ExecutionContext ctx)
        {
            return new ElaValue(value);
        }

        internal override ElaValue Head(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetString()[0]);
        }

        internal override ElaValue Tail(ElaValue left, ExecutionContext ctx)
        {
            return left.Ref.Tail(ctx);
        }

        internal override bool IsNil(ElaValue left, ExecutionContext ctx)
        {
            return left.DirectGetString().Length == 0;
        }

        internal override ElaValue CastTo(ElaValue castTo, ElaValue value, ExecutionContext ctx)
        {
            switch (castTo.TypeCode)
            {
                case ElaTypeCode.List:
                    return new ElaValue(((ElaString)value.Ref).ToList());
                default:
                    {
                        ctx.ConversionFailed(value, castTo.GetTypeName());
                        return Default();
                    };
            }
        }
    }
}
