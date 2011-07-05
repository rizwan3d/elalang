using System;

namespace Ela.Runtime
{
	public abstract class DispatchTernaryFun
	{
		private readonly DispatchTernaryFun[][] funs;

		protected DispatchTernaryFun()
		{

		}

		internal DispatchTernaryFun(DispatchTernaryFun[][] funs)
		{
			this.funs = funs;
		}

		internal protected abstract ElaValue Call(ElaValue first, ElaValue second, ElaValue third, ExecutionContext ctx);

		protected ElaValue PerformOp(ElaValue first, ElaValue second, ElaValue third, ExecutionContext ctx)
		{
			return funs[first.TypeId][second.TypeId].Call(first, second, third, ctx);
		}
	}


    public abstract class DispatchBinaryFun
    {
        private readonly DispatchBinaryFun[][] funs;

        protected DispatchBinaryFun()
		{

		}

		internal DispatchBinaryFun(DispatchBinaryFun[][] funs)
        {
            this.funs = funs;
        }

        internal protected abstract ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx);

        protected ElaValue PerformOp(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return funs[left.TypeId][right.TypeId].Call(left, right, ctx);
        }
    }


	public abstract class DispatchUnaryFun
	{
		private readonly DispatchUnaryFun[] funs;

		protected DispatchUnaryFun()
		{

		}

		internal DispatchUnaryFun(DispatchUnaryFun[] funs)
		{
			this.funs = funs;
		}

		internal protected abstract ElaValue Call(ElaValue left, ExecutionContext ctx);

		protected ElaValue PerformOp(ElaValue left, ExecutionContext ctx)
		{
			return funs[left.TypeId].Call(left, ctx);
		}
	}
}
