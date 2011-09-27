using System;
using Ela.Runtime.ObjectModel;

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


    internal sealed class FunWrapper : ElaObject
    {
        private DispatchUnaryFun[] unary;
        private DispatchBinaryFun[][] binary;
        private DispatchTernaryFun[][] ternary;

        public FunWrapper(DispatchUnaryFun[] unary)
        {
            this.unary = Clone(unary);
        }


        public FunWrapper(DispatchBinaryFun[][] binary)
        {
            this.binary = Clone(binary);
        }


        public FunWrapper(DispatchTernaryFun[][] ternary)
        {
            this.ternary = Clone(ternary);
        }


        internal ElaValue Call(ElaValue first, ExecutionContext ctx)
        {
            return unary[first.TypeId].Call(first, ctx);
        }


        internal ElaValue Call(ElaValue first, ElaValue second, ExecutionContext ctx)
        {
            return binary[first.TypeId][second.TypeId].Call(first, second, ctx);
        }


        internal ElaValue Call(ElaValue first, ElaValue second, ElaValue third, ExecutionContext ctx)
        {
            return ternary[first.TypeId][second.TypeId].Call(first, second, third, ctx);
        }


        private T[] Clone<T>(T[] funs)
        {
            var ret = new T[funs.Length];

            for (var i = 0; i < funs.Length; i++)
                ret[i] = funs[i];

            return ret;
        }


        private T[][] Clone<T>(T[][] funs)
        {
            var ret = new T[funs.Length][];

            for (var i = 0; i < funs.Length; i++)
                ret[i] = Clone(funs[i]);

            return ret;
        }
    }
}
