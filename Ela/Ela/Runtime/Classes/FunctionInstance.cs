using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class FunctionInstance : Class
    {
        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.FUN)
            {
                ctx.InvalidRightOperand(left, right, "equal");
                return false;
            }

            var f1 = (ElaFunction)left.Ref;
            var f2 = (ElaFunction)left.Ref;
            return f1.Handle == f2.Handle && f1.AppliedParameters == f2.AppliedParameters && f1.AppliedParameters == 0;
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.FUN)
            {
                ctx.InvalidRightOperand(left, right, "notequal");
                return false;
            }

            var f1 = (ElaFunction)left.Ref;
            var f2 = (ElaFunction)left.Ref;
            return f1.Handle != f2.Handle || f1.AppliedParameters != f2.AppliedParameters || f1.AppliedParameters != 0;
        }
    }
}
