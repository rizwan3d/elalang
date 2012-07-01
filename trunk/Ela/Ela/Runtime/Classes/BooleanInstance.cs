using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class BooleanInstance : Class
    {
        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.BYT)
            {
                ctx.InvalidOperand(left, right, "equal");
                return false;
            }

            return left.I4 == right.I4;
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.BYT)
            {
                ctx.InvalidOperand(left, right, "notequal");
                return false;
            }

            return left.I4 != right.I4;
        }

        internal override ElaValue Parse(ElaValue instance, string format, string value, ExecutionContext ctx)
        {
            if (value == "True" || value == "true")
                return new ElaValue(true);
            else if (value == "False" || value == "false")
                return new ElaValue(false);
            else
            {
                ctx.UnableRead(instance, value);
                return Default();
            }
        }
    }
}
