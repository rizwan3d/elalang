using System;

namespace Ela.Runtime
{
    internal abstract class DispatchBinaryFun
    {
        private readonly DispatchBinaryFun[][] funs;

        protected DispatchBinaryFun(DispatchBinaryFun[][] funs)
        {
            this.funs = funs;
        }

        internal protected abstract ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx);

        protected ElaValue PerformOp(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return funs[left.TypeId][right.TypeId].Call(left, right, ctx);
        }
    }
}
