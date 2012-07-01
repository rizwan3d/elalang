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
    }
}
