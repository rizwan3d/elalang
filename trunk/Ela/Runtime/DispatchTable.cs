using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
    #region Generic
    internal sealed class NoneBinary : DispatchBinaryFun
    {
        private string op;

        internal NoneBinary(string op) : base(null)
        {
            this.op = op;
        }

        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            ctx.NoOperation(left, right, op);
            return ElaObject.GetDefault();
        }
    }



	internal sealed class ListCompare : DispatchBinaryFun
	{
		private readonly OpFlag flag;

		internal enum OpFlag
		{
			Eq,
			Neq
		}

		internal ListCompare(DispatchBinaryFun[][] funs, OpFlag flag) : base(funs)
		{
			this.flag = flag;
		}

		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == right.TypeId)
				return ListWithList(left, right, ctx);
			else
				return new ElaValue(flag == OpFlag.Neq);
		}

		private ElaValue ListWithList(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var xs1 = (ElaList)left.Ref;
			var xs2 = (ElaList)right.Ref;

			while (!xs1.IsNil(ctx) && !xs2.IsNil(ctx))
			{
				var eq = PerformOp(xs1.Value, xs2.Value, ctx).I4 == 1;

				if (!eq && flag == OpFlag.Eq)
					return new ElaValue(false);

				xs1 = xs1.Tail(ctx).Ref as ElaList;
				xs2 = xs2.Tail(ctx).Ref as ElaList;

				if (xs1 == null || xs2 == null)
				{
					ctx.Fail(new ElaError("InvalidList", "Invalid list definition."));
					return new ElaValue(false);
				}
			}

			return new ElaValue(flag == OpFlag.Eq);
		}
	}

	internal sealed class RecordCompare : DispatchBinaryFun
	{
		private readonly OpFlag flag;

		internal enum OpFlag
		{
			Eq,
			Neq
		}

		internal RecordCompare(DispatchBinaryFun[][] funs, OpFlag flag) : base(funs)
		{
			this.flag = flag;
		}

		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == right.TypeId)
				return RecordWithRecord(left, right, ctx);
			else
				return new ElaValue(flag == OpFlag.Neq);
		}

		private ElaValue RecordWithRecord(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var recLeft = (ElaRecord)left.Ref;
			var recRight = (ElaRecord)right.Ref;

			if (recLeft.Length != recRight.Length)
				return new ElaValue(flag == OpFlag.Neq);

			if (!EqHelper.ListEquals(recLeft.keys, recRight.keys))
				return new ElaValue(flag == OpFlag.Neq);

			for (var i = 0; i < recLeft.Length; i++)
				if (PerformOp(recLeft[i], recRight[i], ctx).I4 != 1 && flag == OpFlag.Eq)
					return new ElaValue(false);

			return new ElaValue(flag == OpFlag.Eq);
		}
	}

	internal sealed class TupleCompare : DispatchBinaryFun
	{
		private readonly OpFlag flag;

		internal enum OpFlag
		{
			Eq,
			Neq,
			Gt,
			Lt
		}

		internal TupleCompare(DispatchBinaryFun[][] funs, OpFlag flag) : base(funs)
		{
			this.flag = flag;
		}

		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == right.TypeId)
				return TupleWithTuple(left, right, ctx);
			else
				return new ElaValue(flag == OpFlag.Neq);
		}

		private ElaValue TupleWithTuple(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var tupLeft = (ElaTuple)left.Ref;
			var tupRight = (ElaTuple)right.Ref;

			if (tupLeft.Length > tupRight.Length)
				return new ElaValue(flag == OpFlag.Gt || flag == OpFlag.Neq);

			if (tupLeft.Length < tupRight.Length)
				return new ElaValue(flag == OpFlag.Lt || flag == OpFlag.Neq);

			for (var i = 0; i < tupLeft.Length; i++)
				if (PerformOp(tupLeft.FastGet(i), tupRight.FastGet(i), ctx).I4 != 1 && flag != OpFlag.Neq)
					return new ElaValue(false);

			return new ElaValue(flag != OpFlag.Neq);
		}
	}


    internal sealed class TupleBinary : DispatchBinaryFun
    {
		internal TupleBinary(DispatchBinaryFun[][] funs) : base(funs) 
		{
			
		}        

        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return TupleWithTuple(left, right, ctx);
			if (left.TypeId == ElaMachine.TUP)
				return TupleWithAny(left, right, ctx);
			else
				return AnyWithTuple(left, right, ctx);
        }

        private ElaValue TupleWithTuple(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var tupleLeft = (ElaTuple)left.Ref;
            var tupleRight = (ElaTuple)right.Ref;

            if (tupleLeft.Length != tupleRight.Length)
            {
		        ctx.Fail(ElaRuntimeError.TuplesLength, tupleLeft, tupleRight);
                return ElaObject.GetDefault();
            }				

            var newArr = new ElaValue[tupleLeft.Length];

            for (var i = 0; i < tupleLeft.Length; i++)
                newArr[i] = PerformOp(tupleLeft.FastGet(i), tupleRight.FastGet(i), ctx);

            return new ElaValue(new ElaTuple(newArr));
        }

        private ElaValue AnyWithTuple(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var tuple = (ElaTuple)right.Ref;
            var newArr = new ElaValue[tuple.Length];

            for (var i = 0; i < tuple.Length; i++)
                newArr[i] = PerformOp(left, tuple.FastGet(i), ctx);

            return new ElaValue(new ElaTuple(newArr));
        }

        private ElaValue TupleWithAny(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var tuple = (ElaTuple)left.Ref;
            var newArr = new ElaValue[tuple.Length];

            for (var i = 0; i < tuple.Length; i++)
                newArr[i] = PerformOp(tuple.FastGet(i), right, ctx);

            return new ElaValue(new ElaTuple(newArr));
        }
    }

    //internal sealed class TupleUnary : DispatchBinaryFun
    //{
    //    private DispatchBinaryFun[] funs;

    //    internal TupleUnary(DispatchBinaryFun[] funs)
    //    {
    //        this.funs = funs;
    //    }

    //    protected internal override bool Call(EvalStack stack, ExecutionContext ctx)
    //    {
    //        var tuple = (ElaTuple)stack.Pop().Ref;
    //        var newArr = new ElaValue[tuple.Length];

    //        for (var i = 0; i < tuple.Length; i++)
    //            newArr[i] = ElaMachine.UnaryOp(funs, tuple.FastGet(i), ctx);

    //        stack.Push(new ElaValue(new ElaTuple(newArr)));
    //        return false;
    //    }
    //}
    
    internal sealed class ThunkBinary : DispatchBinaryFun
    {
        internal ThunkBinary(DispatchBinaryFun[][] funs) : base(funs) { }
        
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (!left.Ref.IsEvaluated())
            {
                var t = (ElaLazy)left.Ref;
                ctx.Failed = true;
                ctx.Thunk = t;
                return ElaObject.GetDefault();
            }

            if (!right.Ref.IsEvaluated())
            {
                var t = (ElaLazy)right.Ref;
                ctx.Failed = true;
                ctx.Thunk = t;
                return ElaObject.GetDefault();
            }

            return PerformOp(left.Force(ctx), right.Force(ctx), ctx);
        }
    }

    //internal sealed class ThunkUnary : DispatchBinaryFun
    //{
    //    private DispatchBinaryFun[] funs;

    //    internal ThunkUnary(DispatchBinaryFun[] funs)
    //    {
    //        this.funs = funs;
    //    }

    //    protected internal override bool Call(EvalStack stack, ExecutionContext ctx)
    //    {
    //        var left = stack.Pop();

    //        if (!left.Ref.IsEvaluated())
    //        {
    //            var t = (ElaLazy)left.Ref;
    //            ctx.Failed = true;
    //            ctx.Thunk = t;
    //            return true;
    //        }

    //        stack.Push(ElaMachine.UnaryOp(funs, left.Force(ctx), ctx));
    //        return false;
    //    }
    //}
    #endregion


    #region Add
    internal sealed class AddIntInt : DispatchBinaryFun
    {
        internal AddIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 + right.I4);
        }
    }

    internal sealed class AddIntLong : DispatchBinaryFun
    {
        internal AddIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.I4 + right.GetLong()));            
        }
    }

    internal sealed class AddIntSingle : DispatchBinaryFun
    {
        internal AddIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 + right.DirectGetReal());            
        }
    }

    internal sealed class AddIntDouble : DispatchBinaryFun
    {
        internal AddIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.I4 + right.GetDouble()));            
        }
    }

    internal sealed class AddLongInt : DispatchBinaryFun
    {
        internal AddLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() + right.I4));            
        }
    }

    internal sealed class AddLongLong : DispatchBinaryFun
    {
        internal AddLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() + right.GetLong()));            
        }
    }

    internal sealed class AddLongSingle : DispatchBinaryFun
    {
        internal AddLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() + right.DirectGetReal());            
        }
    }

    internal sealed class AddLongDouble : DispatchBinaryFun
    {
        internal AddLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetLong() + right.GetDouble()));            
        }
    }

    internal sealed class AddSingleSingle : DispatchBinaryFun
    {
        internal AddSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() + right.DirectGetReal());            
        }
    }

    internal sealed class AddSingleInt : DispatchBinaryFun
    {
        internal AddSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() + right.I4);            
        }
    }

    internal sealed class AddSingleLong : DispatchBinaryFun
    {
        internal AddSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() + right.GetLong());            
        }
    }

    internal sealed class AddSingleDouble : DispatchBinaryFun
    {
        internal AddSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.DirectGetReal() + right.GetDouble()));            
        }
    }

    internal sealed class AddDoubleDouble : DispatchBinaryFun
    {
        internal AddDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() + right.GetDouble()));            
        }
    }

    internal sealed class AddDoubleInt : DispatchBinaryFun
    {
        internal AddDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() + right.I4));            
        }
    }

    internal sealed class AddDoubleLong : DispatchBinaryFun
    {
        internal AddDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() + right.GetLong()));            
        }
    }

    internal sealed class AddDoubleSingle : DispatchBinaryFun
    {
        internal AddDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() + right.DirectGetReal()));            
        }
    }
    #endregion


    #region Sub
    internal sealed class SubIntInt : DispatchBinaryFun
    {
        internal SubIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 - right.I4);
        }
    }

    internal sealed class SubIntLong : DispatchBinaryFun
    {
        internal SubIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.I4 - right.GetLong()));
        }
    }

    internal sealed class SubIntSingle : DispatchBinaryFun
    {
        internal SubIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 - right.DirectGetReal());
        }
    }

    internal sealed class SubIntDouble : DispatchBinaryFun
    {
        internal SubIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.I4 - right.GetDouble()));
        }
    }

    internal sealed class SubLongInt : DispatchBinaryFun
    {
        internal SubLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() - right.I4));
        }
    }

    internal sealed class SubLongLong : DispatchBinaryFun
    {
        internal SubLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() - right.GetLong()));
        }
    }

    internal sealed class SubLongSingle : DispatchBinaryFun
    {
        internal SubLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() - right.DirectGetReal());
        }
    }

    internal sealed class SubLongDouble : DispatchBinaryFun
    {
        internal SubLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetLong() - right.GetDouble()));
        }
    }

    internal sealed class SubSingleSingle : DispatchBinaryFun
    {
        internal SubSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() - right.DirectGetReal());
        }
    }

    internal sealed class SubSingleInt : DispatchBinaryFun
    {
        internal SubSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() - right.I4);
        }
    }

    internal sealed class SubSingleLong : DispatchBinaryFun
    {
        internal SubSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() - right.GetLong());
        }
    }

    internal sealed class SubSingleDouble : DispatchBinaryFun
    {
        internal SubSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.DirectGetReal() - right.GetDouble()));
        }
    }

    internal sealed class SubDoubleDouble : DispatchBinaryFun
    {
        internal SubDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() - right.GetDouble()));
        }
    }

    internal sealed class SubDoubleInt : DispatchBinaryFun
    {
        internal SubDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() - right.I4));
        }
    }

    internal sealed class SubDoubleLong : DispatchBinaryFun
    {
        internal SubDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() - right.GetLong()));
        }
    }

    internal sealed class SubDoubleSingle : DispatchBinaryFun
    {
        internal SubDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() - right.DirectGetReal()));
        }
    }
    #endregion


    #region Mul
    internal sealed class MulIntInt : DispatchBinaryFun
    {
        internal MulIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 * right.I4);
        }
    }

    internal sealed class MulIntLong : DispatchBinaryFun
    {
        internal MulIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.I4 * right.GetLong()));
        }
    }

    internal sealed class MulIntSingle : DispatchBinaryFun
    {
        internal MulIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 * right.DirectGetReal());
        }
    }

    internal sealed class MulIntDouble : DispatchBinaryFun
    {
        internal MulIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.I4 * right.GetDouble()));
        }
    }

    internal sealed class MulLongInt : DispatchBinaryFun
    {
        internal MulLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() * right.I4));
        }
    }

    internal sealed class MulLongLong : DispatchBinaryFun
    {
        internal MulLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() * right.GetLong()));
        }
    }

    internal sealed class MulLongSingle : DispatchBinaryFun
    {
        internal MulLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() * right.DirectGetReal());
        }
    }

    internal sealed class MulLongDouble : DispatchBinaryFun
    {
        internal MulLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetLong() * right.GetDouble()));
        }
    }

    internal sealed class MulSingleSingle : DispatchBinaryFun
    {
        internal MulSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() * right.DirectGetReal());
        }
    }

    internal sealed class MulSingleInt : DispatchBinaryFun
    {
        internal MulSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() * right.I4);
        }
    }

    internal sealed class MulSingleLong : DispatchBinaryFun
    {
        internal MulSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() * right.GetLong());
        }
    }

    internal sealed class MulSingleDouble : DispatchBinaryFun
    {
        internal MulSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.DirectGetReal() * right.GetDouble()));
        }
    }

    internal sealed class MulDoubleDouble : DispatchBinaryFun
    {
        internal MulDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() * right.GetDouble()));
        }
    }

    internal sealed class MulDoubleInt : DispatchBinaryFun
    {
        internal MulDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() * right.I4));
        }
    }

    internal sealed class MulDoubleLong : DispatchBinaryFun
    {
        internal MulDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() * right.GetLong()));
        }
    }

    internal sealed class MulDoubleSingle : DispatchBinaryFun
    {
        internal MulDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() * right.DirectGetReal()));
        }
    }
    #endregion


    #region Div
    internal sealed class DivIntInt : DispatchBinaryFun
    {
        internal DivIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.I4 == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(left.I4 / right.I4);
        }
    }

    internal sealed class DivIntLong : DispatchBinaryFun
    {
        internal DivIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.GetLong() == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaLong(left.I4 / right.GetLong()));
        }
    }

    internal sealed class DivIntSingle : DispatchBinaryFun
    {
        internal DivIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 / right.DirectGetReal());
        }
    }

    internal sealed class DivIntDouble : DispatchBinaryFun
    {
        internal DivIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.I4 / right.GetDouble()));
        }
    }

    internal sealed class DivLongInt : DispatchBinaryFun
    {
        internal DivLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.I4 == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaLong(left.GetLong() / right.I4));
        }
    }

    internal sealed class DivLongLong : DispatchBinaryFun
    {
        internal DivLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.GetLong() == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaLong(left.GetLong() / right.GetLong()));
        }
    }

    internal sealed class DivLongSingle : DispatchBinaryFun
    {
        internal DivLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() / right.DirectGetReal());
        }
    }

    internal sealed class DivLongDouble : DispatchBinaryFun
    {
        internal DivLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetLong() / right.GetDouble()));
        }
    }

    internal sealed class DivSingleSingle : DispatchBinaryFun
    {
        internal DivSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() / right.DirectGetReal());
        }
    }

    internal sealed class DivSingleInt : DispatchBinaryFun
    {
        internal DivSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.I4 == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(left.DirectGetReal() / right.I4);
        }
    }

    internal sealed class DivSingleLong : DispatchBinaryFun
    {
        internal DivSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.GetLong() == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(left.DirectGetReal() / right.GetLong());
        }
    }

    internal sealed class DivSingleDouble : DispatchBinaryFun
    {
        internal DivSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.DirectGetReal() / right.GetDouble()));
        }
    }

    internal sealed class DivDoubleDouble : DispatchBinaryFun
    {
        internal DivDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() / right.GetDouble()));
        }
    }

    internal sealed class DivDoubleInt : DispatchBinaryFun
    {
        internal DivDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.I4 == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaDouble(left.GetDouble() / right.I4));
        }
    }

    internal sealed class DivDoubleLong : DispatchBinaryFun
    {
        internal DivDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.GetLong() == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaDouble(left.GetDouble() / right.GetLong()));
        }
    }

    internal sealed class DivDoubleSingle : DispatchBinaryFun
    {
        internal DivDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() / right.DirectGetReal()));
        }
    }
    #endregion


    #region Rem
    internal sealed class RemIntInt : DispatchBinaryFun
    {
        internal RemIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.I4 == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(left.I4 % right.I4);
        }
    }

    internal sealed class RemIntLong : DispatchBinaryFun
    {
        internal RemIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.GetLong() == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaLong(left.I4 % right.GetLong()));
        }
    }

    internal sealed class RemIntSingle : DispatchBinaryFun
    {
        internal RemIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 % right.DirectGetReal());
        }
    }

    internal sealed class RemIntDouble : DispatchBinaryFun
    {
        internal RemIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.I4 % right.GetDouble()));
        }
    }

    internal sealed class RemLongInt : DispatchBinaryFun
    {
        internal RemLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.I4 == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaLong(left.GetLong() % right.I4));
        }
    }

    internal sealed class RemLongLong : DispatchBinaryFun
    {
        internal RemLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.GetLong() == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaLong(left.GetLong() % right.GetLong()));
        }
    }

    internal sealed class RemLongSingle : DispatchBinaryFun
    {
        internal RemLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() % right.DirectGetReal());
        }
    }

    internal sealed class RemLongDouble : DispatchBinaryFun
    {
        internal RemLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetLong() % right.GetDouble()));
        }
    }

    internal sealed class RemSingleSingle : DispatchBinaryFun
    {
        internal RemSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() % right.DirectGetReal());
        }
    }

    internal sealed class RemSingleInt : DispatchBinaryFun
    {
        internal RemSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.I4 == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(left.DirectGetReal() % right.I4);
        }
    }

    internal sealed class RemSingleLong : DispatchBinaryFun
    {
        internal RemSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.GetLong() == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(left.DirectGetReal() % right.GetLong());
        }
    }

    internal sealed class RemSingleDouble : DispatchBinaryFun
    {
        internal RemSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.DirectGetReal() % right.GetDouble()));
        }
    }

    internal sealed class RemDoubleDouble : DispatchBinaryFun
    {
        internal RemDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() % right.GetDouble()));
        }
    }

    internal sealed class RemDoubleInt : DispatchBinaryFun
    {
        internal RemDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.I4 == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaDouble(left.GetDouble() % right.I4));
        }
    }

    internal sealed class RemDoubleLong : DispatchBinaryFun
    {
        internal RemDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.GetLong() == 0)
            {
                ctx.DivideByZero(left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(new ElaDouble(left.GetDouble() % right.GetLong()));
        }
    }

    internal sealed class RemDoubleSingle : DispatchBinaryFun
    {
        internal RemDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDouble(left.GetDouble() % right.DirectGetReal()));
        }
    }
    #endregion


    #region Pow
    internal sealed class PowIntInt : DispatchBinaryFun
    {
        internal PowIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.I4, right.I4));
        }
    }

    internal sealed class PowIntLong : DispatchBinaryFun
    {
        internal PowIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.I4, right.GetLong()));
        }
    }

    internal sealed class PowIntSingle : DispatchBinaryFun
    {
        internal PowIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.I4, right.DirectGetReal()));
        }
    }

    internal sealed class PowIntDouble : DispatchBinaryFun
    {
        internal PowIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.I4, right.GetDouble()));
        }
    }

    internal sealed class PowLongInt : DispatchBinaryFun
    {
        internal PowLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.GetLong(), right.I4));
        }
    }

    internal sealed class PowLongLong : DispatchBinaryFun
    {
        internal PowLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.GetLong(), right.GetLong()));
        }
    }

    internal sealed class PowLongSingle : DispatchBinaryFun
    {
        internal PowLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.GetLong(), right.DirectGetReal()));
        }
    }

    internal sealed class PowLongDouble : DispatchBinaryFun
    {
        internal PowLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
           return new ElaValue(Math.Pow(left.GetLong(), right.GetDouble()));
        }
    }

    internal sealed class PowSingleSingle : DispatchBinaryFun
    {
        internal PowSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.DirectGetReal(), right.DirectGetReal()));
        }
    }

    internal sealed class PowSingleInt : DispatchBinaryFun
    {
        internal PowSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.DirectGetReal(), right.I4));
        }
    }

    internal sealed class PowSingleLong : DispatchBinaryFun
    {
        internal PowSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.DirectGetReal(), right.GetLong()));
        }
    }

    internal sealed class PowSingleDouble : DispatchBinaryFun
    {
        internal PowSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.DirectGetReal(), right.GetDouble()));
        }
    }

    internal sealed class PowDoubleDouble : DispatchBinaryFun
    {
        internal PowDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.GetDouble(), right.GetDouble()));
        }
    }

    internal sealed class PowDoubleInt : DispatchBinaryFun
    {
        internal PowDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.GetDouble(), right.I4));
        }
    }

    internal sealed class PowDoubleLong : DispatchBinaryFun
    {
        internal PowDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.GetDouble(), right.GetLong()));
        }
    }

    internal sealed class PowDoubleSingle : DispatchBinaryFun
    {
        internal PowDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Math.Pow(left.GetDouble(), right.DirectGetReal()));
        }
    }
    #endregion


    #region And
    internal sealed class AndIntInt : DispatchBinaryFun
    {
        internal AndIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 & right.I4);
        }
    }

    internal sealed class AndIntLong : DispatchBinaryFun
    {
        internal AndIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.I4 & right.GetLong()));
        }
    }

    internal sealed class AndLongInt : DispatchBinaryFun
    {
        internal AndLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() & right.I4));
        }
    }

    internal sealed class AndLongLong : DispatchBinaryFun
    {
        internal AndLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() & right.GetLong()));
        }
    }
    #endregion


    #region Bor
    internal sealed class BorIntInt : DispatchBinaryFun
    {
        internal BorIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 | right.I4);
        }
    }

    internal sealed class BorIntLong : DispatchBinaryFun
    {
        internal BorIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var l = (long)left.I4;
            return new ElaValue(new ElaLong(l | right.GetLong()));
        }
    }

    internal sealed class BorLongInt : DispatchBinaryFun
    {
        internal BorLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var l = (long)right.I4;
            return new ElaValue(new ElaLong(left.GetLong() | l));
        }
    }

    internal sealed class BorLongLong : DispatchBinaryFun
    {
        internal BorLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() | right.GetLong()));
        }
    }
    #endregion


    #region Xor
    internal sealed class XorIntInt : DispatchBinaryFun
    {
        internal XorIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 ^ right.I4);
        }
    }

    internal sealed class XorIntLong : DispatchBinaryFun
    {
        internal XorIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var l = (long)left.I4;
            return new ElaValue(new ElaLong(l ^ right.GetLong()));
        }
    }

    internal sealed class XorLongInt : DispatchBinaryFun
    {
        internal XorLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var l = (long)right.I4;
            return new ElaValue(new ElaLong(left.GetLong() ^ l));
        }
    }

    internal sealed class XorLongLong : DispatchBinaryFun
    {
        internal XorLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() ^ right.GetLong()));
        }
    }
    #endregion


    #region Shl
    internal sealed class ShlIntInt : DispatchBinaryFun
    {
        internal ShlIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 << right.I4);
        }
    }

    internal sealed class ShlIntLong : DispatchBinaryFun
    {
        internal ShlIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.I4 << (int)right.GetLong()));
        }
    }

    internal sealed class ShlLongInt : DispatchBinaryFun
    {
        internal ShlLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() << right.I4));
        }
    }

    internal sealed class ShlLongLong : DispatchBinaryFun
    {
        internal ShlLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() << (int)right.GetLong()));
        }
    }
    #endregion


    #region Shr
    internal sealed class ShrIntInt : DispatchBinaryFun
    {
        internal ShrIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 >> right.I4);
        }
    }

    internal sealed class ShrIntLong : DispatchBinaryFun
    {
        internal ShrIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.I4 >> (int)right.GetLong()));
        }
    }

    internal sealed class ShrLongInt : DispatchBinaryFun
    {
        internal ShrLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() >> right.I4));
        }
    }

    internal sealed class ShrLongLong : DispatchBinaryFun
    {
        internal ShrLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLong(left.GetLong() >> (int)right.GetLong()));
        }
    }
    #endregion


    #region Gtr
    internal sealed class GtrIntInt : DispatchBinaryFun
    {
        internal GtrIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 > right.I4);
        }
    }

    internal sealed class GtrIntLong : DispatchBinaryFun
    {
        internal GtrIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 > right.GetLong());
        }
    }

    internal sealed class GtrIntSingle : DispatchBinaryFun
    {
        internal GtrIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 > right.DirectGetReal());
        }
    }

    internal sealed class GtrIntDouble : DispatchBinaryFun
    {
        internal GtrIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 > right.GetDouble());
        }
    }

    internal sealed class GtrLongInt : DispatchBinaryFun
    {
        internal GtrLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() > right.I4);
        }
    }

    internal sealed class GtrLongLong : DispatchBinaryFun
    {
        internal GtrLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() > right.GetLong());
        }
    }

    internal sealed class GtrLongSingle : DispatchBinaryFun
    {
        internal GtrLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() > right.DirectGetReal());
        }
    }

    internal sealed class GtrLongDouble : DispatchBinaryFun
    {
        internal GtrLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() > right.GetDouble());
        }
    }

    internal sealed class GtrSingleSingle : DispatchBinaryFun
    {
        internal GtrSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() > right.DirectGetReal());
        }
    }

    internal sealed class GtrSingleInt : DispatchBinaryFun
    {
        internal GtrSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() > right.I4);
        }
    }

    internal sealed class GtrSingleLong : DispatchBinaryFun
    {
        internal GtrSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() > right.GetLong());
        }
    }

    internal sealed class GtrSingleDouble : DispatchBinaryFun
    {
        internal GtrSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() > right.GetDouble());
        }
    }

    internal sealed class GtrDoubleDouble : DispatchBinaryFun
    {
        internal GtrDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() > right.GetDouble());
        }
    }

    internal sealed class GtrDoubleInt : DispatchBinaryFun
    {
        internal GtrDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() > right.I4);
        }
    }

    internal sealed class GtrDoubleLong : DispatchBinaryFun
    {
        internal GtrDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() > right.GetLong());
        }
    }

    internal sealed class GtrDoubleSingle : DispatchBinaryFun
    {
        internal GtrDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() > right.DirectGetReal());
        }
    }

    internal sealed class GtrCharChar : DispatchBinaryFun
    {
        internal GtrCharChar(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 > right.I4);
        }
    }

    internal sealed class GtrStringString : DispatchBinaryFun
    {
        internal GtrStringString(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetString().CompareTo(right.DirectGetString()) > 0);
        }
    }
    #endregion


    #region Ltr
    internal sealed class LtrIntInt : DispatchBinaryFun
    {
        internal LtrIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 < right.I4);
        }
    }

    internal sealed class LtrIntLong : DispatchBinaryFun
    {
        internal LtrIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 < right.GetLong());
        }
    }

    internal sealed class LtrIntSingle : DispatchBinaryFun
    {
        internal LtrIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 < right.DirectGetReal());
        }
    }

    internal sealed class LtrIntDouble : DispatchBinaryFun
    {
        internal LtrIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 < right.GetDouble());
        }
    }

    internal sealed class LtrLongInt : DispatchBinaryFun
    {
        internal LtrLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() < right.I4);
        }
    }

    internal sealed class LtrLongLong : DispatchBinaryFun
    {
        internal LtrLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() < right.GetLong());
        }
    }

    internal sealed class LtrLongSingle : DispatchBinaryFun
    {
        internal LtrLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() < right.DirectGetReal());
        }
    }

    internal sealed class LtrLongDouble : DispatchBinaryFun
    {
        internal LtrLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() < right.GetDouble());
        }
    }

    internal sealed class LtrSingleSingle : DispatchBinaryFun
    {
        internal LtrSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() < right.DirectGetReal());
        }
    }

    internal sealed class LtrSingleInt : DispatchBinaryFun
    {
        internal LtrSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() < right.I4);
        }
    }

    internal sealed class LtrSingleLong : DispatchBinaryFun
    {
        internal LtrSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() < right.GetLong());
        }
    }

    internal sealed class LtrSingleDouble : DispatchBinaryFun
    {
        internal LtrSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() < right.GetDouble());
        }
    }

    internal sealed class LtrDoubleDouble : DispatchBinaryFun
    {
        internal LtrDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() < right.GetDouble());
        }
    }

    internal sealed class LtrDoubleInt : DispatchBinaryFun
    {
        internal LtrDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() < right.I4);
        }
    }

    internal sealed class LtrDoubleLong : DispatchBinaryFun
    {
        internal LtrDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() < right.GetLong());
        }
    }

    internal sealed class LtrDoubleSingle : DispatchBinaryFun
    {
        internal LtrDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() < right.DirectGetReal());
        }
    }

    internal sealed class LtrCharChar : DispatchBinaryFun
    {
        internal LtrCharChar(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 < right.I4);
        }
    }

    internal sealed class LtrStringString : DispatchBinaryFun
    {
        internal LtrStringString(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetString().CompareTo(right.DirectGetString()) < 0);
        }
    }
    #endregion


    #region Gte
    internal sealed class GteIntInt : DispatchBinaryFun
    {
        internal GteIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 >= right.I4);
        }
    }

    internal sealed class GteIntLong : DispatchBinaryFun
    {
        internal GteIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 >= right.GetLong());
        }
    }

    internal sealed class GteIntSingle : DispatchBinaryFun
    {
        internal GteIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 >= right.DirectGetReal());
        }
    }

    internal sealed class GteIntDouble : DispatchBinaryFun
    {
        internal GteIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 >= right.GetDouble());
        }
    }

    internal sealed class GteLongInt : DispatchBinaryFun
    {
        internal GteLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() >= right.I4);
        }
    }

    internal sealed class GteLongLong : DispatchBinaryFun
    {
        internal GteLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() >= right.GetLong());
        }
    }

    internal sealed class GteLongSingle : DispatchBinaryFun
    {
        internal GteLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() >= right.DirectGetReal());
        }
    }

    internal sealed class GteLongDouble : DispatchBinaryFun
    {
        internal GteLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() >= right.GetDouble());
        }
    }

    internal sealed class GteSingleSingle : DispatchBinaryFun
    {
        internal GteSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() >= right.DirectGetReal());
        }
    }

    internal sealed class GteSingleInt : DispatchBinaryFun
    {
        internal GteSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() >= right.I4);
        }
    }

    internal sealed class GteSingleLong : DispatchBinaryFun
    {
        internal GteSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() >= right.GetLong());
        }
    }

    internal sealed class GteSingleDouble : DispatchBinaryFun
    {
        internal GteSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() >= right.GetDouble());
        }
    }

    internal sealed class GteDoubleDouble : DispatchBinaryFun
    {
        internal GteDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() >= right.GetDouble());
        }
    }

    internal sealed class GteDoubleInt : DispatchBinaryFun
    {
        internal GteDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() >= right.I4);
        }
    }

    internal sealed class GteDoubleLong : DispatchBinaryFun
    {
        internal GteDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() >= right.GetLong());
        }
    }

    internal sealed class GteDoubleSingle : DispatchBinaryFun
    {
        internal GteDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() >= right.DirectGetReal());
        }
    }

    internal sealed class GteCharChar : DispatchBinaryFun
    {
        internal GteCharChar(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 >= right.I4);
        }
    }

    internal sealed class GteStringString : DispatchBinaryFun
    {
        internal GteStringString(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetString().CompareTo(right.DirectGetString()) >= 0);
        }
    }
    #endregion


    #region Lte
    internal sealed class LteIntInt : DispatchBinaryFun
    {
        internal LteIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 <= right.I4);
        }
    }

    internal sealed class LteIntLong : DispatchBinaryFun
    {
        internal LteIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 <= right.GetLong());
        }
    }

    internal sealed class LteIntSingle : DispatchBinaryFun
    {
        internal LteIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 <= right.DirectGetReal());
        }
    }

    internal sealed class LteIntDouble : DispatchBinaryFun
    {
        internal LteIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 <= right.GetDouble());
        }
    }

    internal sealed class LteLongInt : DispatchBinaryFun
    {
        internal LteLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() <= right.I4);
        }
    }

    internal sealed class LteLongLong : DispatchBinaryFun
    {
        internal LteLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() <= right.GetLong());
        }
    }

    internal sealed class LteLongSingle : DispatchBinaryFun
    {
        internal LteLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() <= right.DirectGetReal());
        }
    }

    internal sealed class LteLongDouble : DispatchBinaryFun
    {
        internal LteLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() <= right.GetDouble());
        }
    }

    internal sealed class LteSingleSingle : DispatchBinaryFun
    {
        internal LteSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() <= right.DirectGetReal());
        }
    }

    internal sealed class LteSingleInt : DispatchBinaryFun
    {
        internal LteSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() <= right.I4);
        }
    }

    internal sealed class LteSingleLong : DispatchBinaryFun
    {
        internal LteSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() <= right.GetLong());
        }
    }

    internal sealed class LteSingleDouble : DispatchBinaryFun
    {
        internal LteSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() <= right.GetDouble());
        }
    }

    internal sealed class LteDoubleDouble : DispatchBinaryFun
    {
        internal LteDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() <= right.GetDouble());
        }
    }

    internal sealed class LteDoubleInt : DispatchBinaryFun
    {
        internal LteDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() <= right.I4);
        }
    }

    internal sealed class LteDoubleLong : DispatchBinaryFun
    {
        internal LteDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() <= right.GetLong());
        }
    }

    internal sealed class LteDoubleSingle : DispatchBinaryFun
    {
        internal LteDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() <= right.DirectGetReal());
        }
    }

    internal sealed class LteCharChar : DispatchBinaryFun
    {
        internal LteCharChar(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 <= right.I4);
        }
    }

    internal sealed class LteStringString : DispatchBinaryFun
    {
        internal LteStringString(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetString().CompareTo(right.DirectGetString()) <= 0);
        }
    }
    #endregion


    #region Eql
    internal sealed class EqlIntInt : DispatchBinaryFun
    {
        internal EqlIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 == right.I4);
        }
    }

    internal sealed class EqlIntLong : DispatchBinaryFun
    {
        internal EqlIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 == right.GetLong());
        }
    }

    internal sealed class EqlIntSingle : DispatchBinaryFun
    {
        internal EqlIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 == right.DirectGetReal());
        }
    }

    internal sealed class EqlIntDouble : DispatchBinaryFun
    {
        internal EqlIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 == right.GetDouble());
        }
    }

    internal sealed class EqlLongInt : DispatchBinaryFun
    {
        internal EqlLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() == right.I4);
        }
    }

    internal sealed class EqlLongLong : DispatchBinaryFun
    {
        internal EqlLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() == right.GetLong());
        }
    }

    internal sealed class EqlLongSingle : DispatchBinaryFun
    {
        internal EqlLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() == right.DirectGetReal());
        }
    }

    internal sealed class EqlLongDouble : DispatchBinaryFun
    {
        internal EqlLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() == right.GetDouble());
        }
    }

    internal sealed class EqlSingleSingle : DispatchBinaryFun
    {
        internal EqlSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() == right.DirectGetReal());
        }
    }

    internal sealed class EqlSingleInt : DispatchBinaryFun
    {
        internal EqlSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() == right.I4);
        }
    }

    internal sealed class EqlSingleLong : DispatchBinaryFun
    {
        internal EqlSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() == right.GetLong());
        }
    }

    internal sealed class EqlSingleDouble : DispatchBinaryFun
    {
        internal EqlSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() == right.GetDouble());
        }
    }

    internal sealed class EqlDoubleDouble : DispatchBinaryFun
    {
        internal EqlDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() == right.GetDouble());
        }
    }

    internal sealed class EqlDoubleInt : DispatchBinaryFun
    {
        internal EqlDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() == right.I4);
        }
    }

    internal sealed class EqlDoubleLong : DispatchBinaryFun
    {
        internal EqlDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() == right.GetLong());
        }
    }

    internal sealed class EqlDoubleSingle : DispatchBinaryFun
    {
        internal EqlDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() == right.DirectGetReal());
        }
    }

    internal sealed class EqlCharChar : DispatchBinaryFun
    {
        internal EqlCharChar(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 == right.I4);
        }
    }

    internal sealed class EqlStringString : DispatchBinaryFun
    {
        internal EqlStringString(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetString() == right.DirectGetString());
        }
    }

    internal sealed class EqlModMod : DispatchBinaryFun
    {
        internal EqlModMod(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(((ElaModule)left.Ref).Handle == ((ElaModule)right.Ref).Handle);
        }
    }

    internal sealed class EqlFunFun : DispatchBinaryFun
    {
        internal EqlFunFun(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(ElaFunction.IsEqual(left.Ref, right.Ref));
        }
    }

	internal sealed class EqlVariantVariant : DispatchBinaryFun
	{
		internal EqlVariantVariant(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var v1 = (ElaVariant)left.Ref;
			var v2 = (ElaVariant)right.Ref;
			return new ElaValue(v1.Tag == v2.Tag && PerformOp(v1.Value, v2.Value, ctx).I4 == 1);
		}
	}

	internal sealed class EqlUnitUnit : DispatchBinaryFun
	{
		internal EqlUnitUnit(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(true);
		}
	}
    #endregion


	#region Neq
	internal sealed class NeqIntInt : DispatchBinaryFun
	{
		internal NeqIntInt(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.I4 != right.I4);
		}
	}

	internal sealed class NeqIntLong : DispatchBinaryFun
	{
		internal NeqIntLong(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.I4 != right.GetLong());
		}
	}

	internal sealed class NeqIntSingle : DispatchBinaryFun
	{
		internal NeqIntSingle(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.I4 != right.DirectGetReal());
		}
	}

	internal sealed class NeqIntDouble : DispatchBinaryFun
	{
		internal NeqIntDouble(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.I4 != right.GetDouble());
		}
	}

	internal sealed class NeqLongInt : DispatchBinaryFun
	{
		internal NeqLongInt(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.GetLong() != right.I4);
		}
	}

	internal sealed class NeqLongLong : DispatchBinaryFun
	{
		internal NeqLongLong(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.GetLong() != right.GetLong());
		}
	}

	internal sealed class NeqLongSingle : DispatchBinaryFun
	{
		internal NeqLongSingle(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.GetLong() != right.DirectGetReal());
		}
	}

	internal sealed class NeqLongDouble : DispatchBinaryFun
	{
		internal NeqLongDouble(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.GetLong() != right.GetDouble());
		}
	}

	internal sealed class NeqSingleSingle : DispatchBinaryFun
	{
		internal NeqSingleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.DirectGetReal() != right.DirectGetReal());
		}
	}

	internal sealed class NeqSingleInt : DispatchBinaryFun
	{
		internal NeqSingleInt(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.DirectGetReal() != right.I4);
		}
	}

	internal sealed class NeqSingleLong : DispatchBinaryFun
	{
		internal NeqSingleLong(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.DirectGetReal() != right.GetLong());
		}
	}

	internal sealed class NeqSingleDouble : DispatchBinaryFun
	{
		internal NeqSingleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.DirectGetReal() != right.GetDouble());
		}
	}

	internal sealed class NeqDoubleDouble : DispatchBinaryFun
	{
		internal NeqDoubleDouble(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.GetDouble() != right.GetDouble());
		}
	}

	internal sealed class NeqDoubleInt : DispatchBinaryFun
	{
		internal NeqDoubleInt(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.GetDouble() != right.I4);
		}
	}

	internal sealed class NeqDoubleLong : DispatchBinaryFun
	{
		internal NeqDoubleLong(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.GetDouble() != right.GetLong());
		}
	}

	internal sealed class NeqDoubleSingle : DispatchBinaryFun
	{
		internal NeqDoubleSingle(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.GetDouble() != right.DirectGetReal());
		}
	}

	internal sealed class NeqCharChar : DispatchBinaryFun
	{
		internal NeqCharChar(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.I4 != right.I4);
		}
	}

	internal sealed class NeqStringString : DispatchBinaryFun
	{
		internal NeqStringString(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.DirectGetString() != right.DirectGetString());
		}
	}

	internal sealed class NeqModMod : DispatchBinaryFun
	{
		internal NeqModMod(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(((ElaModule)left.Ref).Handle != ((ElaModule)right.Ref).Handle);
		}
	}

	internal sealed class NeqFunFun : DispatchBinaryFun
	{
		internal NeqFunFun(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(!ElaFunction.IsEqual(left.Ref, right.Ref));
		}
	}

	internal sealed class NeqVariantVariant : DispatchBinaryFun
	{
		internal NeqVariantVariant(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var v1 = (ElaVariant)left.Ref;
			var v2 = (ElaVariant)right.Ref;
			return new ElaValue(v1.Tag != v2.Tag || PerformOp(v1.Value, v2.Value, ctx).I4 == 0);
		}
	}

	internal sealed class NeqUnitUnit : DispatchBinaryFun
	{
		internal NeqUnitUnit(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(false);
		}
	}
	#endregion
}
