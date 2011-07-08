using System;
using Ela.Runtime.ObjectModel;
using System.Collections.Generic;
using System.Text;
using Ela.Compilation;
using Ela.CodeModel;

namespace Ela.Runtime
{
    #region Generic
    internal sealed class EqlAnyAny : NoneBinary
	{
		internal EqlAnyAny(string op, Dictionary<String,ElaOverloadedFunction> overloads) : base(op, overloads) { }
		protected override ElaValue ReturnDefault(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(false);
		}
	}

	internal sealed class NeqAnyAny : NoneBinary
	{
		internal NeqAnyAny(string op, Dictionary<String,ElaOverloadedFunction> overloads) : base(op, overloads) { }
		protected override ElaValue ReturnDefault(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(true);
		}
	}

	internal class NoneTernary : DispatchTernaryFun
	{
		private readonly string op;
		private readonly Dictionary<String,ElaOverloadedFunction> overloads;

		internal NoneTernary(string op, Dictionary<String,ElaOverloadedFunction> overloads) : base(null)
		{
			this.op = op;
			this.overloads = overloads;
		}

		protected internal override ElaValue Call(ElaValue first, ElaValue second, ElaValue third, ExecutionContext ctx)
		{
			var fun = default(ElaOverloadedFunction);

			if (overloads.TryGetValue(op, out fun))
			{
				var f1 = fun.Resolve(first, ctx);

				if (ctx.Failed)
					return ElaObject.GetDefault();

				var fo1 = f1 as ElaOverloadedFunction;

				if (fo1 == null)
				{
					ctx.Fail(ElaRuntimeError.OverloadNotFound, first.GetTag() + "->" + second.GetTag() + "->*",
						op.Replace("$", String.Empty));
					return ElaObject.GetDefault();
				}

				var f2 = fo1.Resolve(second, ctx);

				if (ctx.Failed)
					return ElaObject.GetDefault();

				ctx.Failed = true;
				f2.Parameters[0] = first;
				f2.Parameters[1] = second;
				f2.LastParameter = third;
				f2.AppliedParameters = f2.Parameters.Length;
				ctx.Function = f2;
				ctx.ToPop = 3;
				return ElaObject.GetDefault();
			}

			return ReturnDefault(first, second, ctx);
		}


		protected virtual ElaValue ReturnDefault(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			ctx.Fail(ElaRuntimeError.OverloadNotFound, left.GetTag() + "->" + right.GetTag() + "->*",
				op.Replace("$", String.Empty));
			return ElaObject.GetDefault();
		}
	}


    internal class NoneBinary : DispatchBinaryFun
    {
        private readonly string op;
        private readonly Dictionary<String,ElaOverloadedFunction> overloads;

        internal NoneBinary(string op, Dictionary<String,ElaOverloadedFunction> overloads) : base(null)
        {
            this.op = op;
            this.overloads = overloads;
        }

        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var fun = default(ElaOverloadedFunction);

            if (overloads.TryGetValue(op, out fun))
            {
                var f1 = fun.Resolve(left, ctx);

                if (ctx.Failed)
                    return ElaObject.GetDefault();

                var fo1 = f1 as ElaOverloadedFunction;

                if (fo1 == null)
                {
                    ctx.Fail(ElaRuntimeError.OverloadNotFound, left.GetTag() + "->" + right.GetTag(), 
                        op.Replace("$", String.Empty));
                    return ElaObject.GetDefault();                    
                }

                var f2 = fo1.Resolve(right, ctx);

                if (ctx.Failed)
                    return ElaObject.GetDefault();

                ctx.Failed = true;
                f2.Parameters[0] = left;
                f2.LastParameter = right;
                f2.AppliedParameters = f2.Parameters.Length;
                ctx.Function = f2;
                ctx.ToPop = 2;
                return ElaObject.GetDefault();
            }

            return ReturnDefault(left, right, ctx);
        }


        protected virtual ElaValue ReturnDefault(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            ctx.Fail(ElaRuntimeError.OverloadNotFound, left.GetTag() + "->" + right.GetTag(),
                op.Replace("$", String.Empty));
            return ElaObject.GetDefault();
        }
    }


	internal class NoneUnary : DispatchUnaryFun
	{
		private readonly string op;
        private readonly Dictionary<String,ElaOverloadedFunction> overloads;

        internal NoneUnary(string op, Dictionary<String,ElaOverloadedFunction> overloads) : base(null)
        {
            this.op = op;
            this.overloads = overloads;
        }

		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			var fun = default(ElaOverloadedFunction);

            if (overloads.TryGetValue(op, out fun))
            {
                var f1 = fun.Resolve(left, ctx);

                if (ctx.Failed)
                    return ElaObject.GetDefault();

                f1.LastParameter = left;
                ctx.Failed = true;
                ctx.Function = f1;
                ctx.ToPop = 1;
                return ElaObject.GetDefault();
            }

            return ReturnDefault(left, ctx);
        }


        protected virtual ElaValue ReturnDefault(ElaValue left, ExecutionContext ctx)
        {
            ctx.Fail(ElaRuntimeError.OverloadNotFound, left.GetTag(), op.Replace("$", String.Empty));
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

			for (; ; ) 
			{
				if (xs1 == ElaList.Empty || xs1 == ElaLazyList.Empty)
				{
                    if (xs2 == ElaList.Empty || xs2 == ElaLazyList.Empty)
						break;
					else
						return new ElaValue(flag == OpFlag.Neq);
				}

				var res = PerformOp(xs1.Value, xs2.Value, ctx).I4 == 1;

				if (res && flag == OpFlag.Neq)
					return new ElaValue(true);
				else if (!res && flag != OpFlag.Neq)
					return new ElaValue(false);

				xs1 = xs1.Tail(ctx).Ref as ElaList;
				xs2 = xs2.Tail(ctx).Ref as ElaList;

				if (xs1 == null || xs2 == null)
				{
					ctx.Fail(new ElaError("InvalidList", "Invalid list definition."));
					return new ElaValue(false);
				}
			}

			return new ElaValue(flag != OpFlag.Neq);
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
			{
				var res = PerformOp(recLeft[i], recRight[i], ctx).I4 == 1;

				if (res && flag == OpFlag.Neq)
					return new ElaValue(true);
				else if (!res && flag != OpFlag.Neq)
					return new ElaValue(false);
			}

			return new ElaValue(flag != OpFlag.Neq);
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
			{
				var res = PerformOp(tupLeft.FastGet(i), tupRight.FastGet(i), ctx).I4 == 1;

				if (res && flag == OpFlag.Neq)
					return new ElaValue(true);
				else if (!res && flag != OpFlag.Neq)
					return new ElaValue(false);
			}

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

	internal sealed class TupleUnary : DispatchUnaryFun
	{
		internal TupleUnary(DispatchUnaryFun[] funs) : base(funs) { }

		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			var tuple = (ElaTuple)left.Ref;
			var newArr = new ElaValue[tuple.Length];

			for (var i = 0; i < tuple.Length; i++)
				newArr[i] = PerformOp(tuple.FastGet(i), ctx);

			return new ElaValue(new ElaTuple(newArr));
		}
	}
    
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

            return PerformOp(
                left.TypeId == ElaMachine.LAZ ? ((ElaLazy)left.Ref).Value : left,
                right.TypeId == ElaMachine.LAZ ? ((ElaLazy)right.Ref).Value : right,
                ctx);
        }
    }

	internal sealed class ThunkTernary : DispatchTernaryFun
	{
		internal ThunkTernary(DispatchTernaryFun[][] funs) : base(funs) { }

		protected internal override ElaValue Call(ElaValue first, ElaValue second, ElaValue third, ExecutionContext ctx)
		{
			if (!first.Ref.IsEvaluated())
			{
				var t = (ElaLazy)first.Ref;
				ctx.Failed = true;
				ctx.Thunk = t;
				return ElaObject.GetDefault();
			}

			if (!second.Ref.IsEvaluated())
			{
				var t = (ElaLazy)second.Ref;
				ctx.Failed = true;
				ctx.Thunk = t;
				return ElaObject.GetDefault();
			}

			return PerformOp(
                first.TypeId == ElaMachine.LAZ ? ((ElaLazy)first.Ref).Value : first, 
                second.TypeId == ElaMachine.LAZ ? ((ElaLazy)second.Ref).Value : second, 
                third, ctx);
		}
	}

	internal sealed class ThunkUnary : DispatchUnaryFun
	{
		internal ThunkUnary(DispatchUnaryFun[] funs) : base(funs) { }

		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			if (!left.Ref.IsEvaluated())
			{
				var t = (ElaLazy)left.Ref;
				ctx.Failed = true;
				ctx.Thunk = t;
				return ElaObject.GetDefault();
			}

			return PerformOp(
                left.TypeId == ElaMachine.LAZ ? ((ElaLazy)left.Ref).Value : left, ctx);
		}
	}
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

	internal sealed class EqlBoolBool : DispatchBinaryFun
	{
		internal EqlBoolBool(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.I4 == right.I4);
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

	internal sealed class NeqBoolBool : DispatchBinaryFun
	{
		internal NeqBoolBool(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.I4 != right.I4);
		}
	}
	#endregion


	#region Concat
	internal sealed class ConcatStringString : DispatchBinaryFun
	{
		internal ConcatStringString(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.DirectGetString() + right.DirectGetString());
		}
	}

	internal sealed class ConcatCharChar : DispatchBinaryFun
	{
		internal ConcatCharChar(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(((Char)left.I4).ToString() + (Char)right.I4);
		}
	}

	internal sealed class ConcatStringChar : DispatchBinaryFun
	{
		internal ConcatStringChar(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.DirectGetString() + (Char)right.I4);
		}
	}

    internal sealed class ConcatCharString : DispatchBinaryFun
    {
        internal ConcatCharString(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue((Char)left.I4 + right.DirectGetString());
        }
    }

    internal sealed class ConcatTupleTuple : DispatchBinaryFun
    {
        internal ConcatTupleTuple(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var arr1 = ((ElaTuple)left.Ref).Values;
            var arr2 = ((ElaTuple)right.Ref).Values;
            var res = new ElaValue[arr1.Length + arr2.Length];
            arr1.CopyTo(res, 0);
            arr2.CopyTo(res, arr1.Length);
            return new ElaValue(new ElaTuple(res));
        }
    }

	internal sealed class ConcatListList : DispatchBinaryFun
	{
		internal ConcatListList(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var xs1 = (ElaList)left.Ref;
			var xs2 = (ElaList)right.Ref;
			var xs1Lazy = xs1 is ElaLazyList;
			var xs2Lazy = xs2 is ElaLazyList;
			
			if (!xs1Lazy && xs2Lazy) //in this case we don't have to force lazy list
			{
				var newLst = xs2;

				foreach (var e in xs1.Reverse())
					newLst = new ElaLazyList((ElaLazyList)newLst, e);

				return new ElaValue(newLst);
			}

			if (xs1Lazy)
				xs1.GetLength(ctx);

			if (xs2Lazy)
				xs2.GetLength(ctx);

			var res = ElaList.FromEnumerable(xs1).Concatenate(xs2);
			return new ElaValue(res);
		}
	}

	internal sealed class ConcatThunk : DispatchBinaryFun
	{
		internal ConcatThunk(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var xs1Lazy = left.Ref is ElaLazyList;
			var xs2Lazy = right.Ref is ElaLazyList;
			var xs1Thunk = left.TypeId == ElaMachine.LAZ;
			var xs2Thunk = right.TypeId == ElaMachine.LAZ;

			if (xs1Lazy) //We have to force both. Right is ElaLazy
			{
				if (!right.Ref.IsEvaluated())
				{
					ctx.Failed = true;
					ctx.Thunk = (ElaLazy)right.Ref;
					return ElaObject.GetDefault();
				}

				right = ((ElaLazy)right.Ref).Value;
				left.Ref.GetLength(ctx);
				return PerformOp(left, right, ctx);
			}
			else if (xs1Thunk && !xs2Thunk) //We don't have to force list. Left is ElaLazy
			{
				if (!left.Ref.IsEvaluated())
				{
					ctx.Failed = true;
					ctx.Thunk = (ElaLazy)left.Ref;
					return ElaObject.GetDefault();
				}

				left = ((ElaLazy)left.Ref).Value;
				return PerformOp(left, right, ctx);
			}			
			else if (xs2Thunk && left.Ref is ElaList)
			{
				var lazy = (ElaLazy)right.Ref;
				var xs = (ElaList)left.Ref;
				var c = 0;
				var newLst = default(ElaLazyList);

				foreach (var e in xs.Reverse())
				{
					if (c == 0)
						newLst = new ElaLazyList(lazy, e);
					else
						newLst = new ElaLazyList(newLst, e);

					c++;
				}

				return new ElaValue(newLst);
			}
			else
			{
				ctx.Fail("Unable to concatenate two entities.");
				return ElaObject.GetDefault();
			}
		}
	}
	#endregion


    #region Cons
    internal sealed class ConsAnyList : DispatchBinaryFun
    {
        internal ConsAnyList(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var laz = right.Ref as ElaLazyList;

            if (laz != null)
                return new ElaValue(new ElaLazyList(laz, left));
            else
            {
                var xs = (ElaList)right.Ref;
                return new ElaValue(new ElaList(xs, left));
            }
        }
    }


    internal sealed class ConsStringString : DispatchBinaryFun
    {
        internal ConsStringString(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var str1 = left.DirectGetString();
            var str2 = right.DirectGetString();
            return new ElaValue(new ElaString(str1 + str2));
        }
    }


    internal sealed class ConsCharString : DispatchBinaryFun
    {
        internal ConsCharString(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var str1 = ((Char)left.I4).ToString();
            var str2 = right.DirectGetString();
            return new ElaValue(new ElaString(str1 + str2));
        }
    }


    internal sealed class ConsAnyThunk : DispatchBinaryFun
    {
        internal ConsAnyThunk(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLazyList((ElaLazy)right.Ref, left));
        }
    }
    #endregion


    #region GetValue
	internal sealed class GetModuleString : DispatchBinaryFun
	{
		internal GetModuleString(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var mod = (ElaModule)left.Ref;
			var fld = right.DirectGetString();
			var fr = mod.Machine.Assembly.GetModule(mod.Handle);
			ScopeVar sc;

			if (!fr.GlobalScope.Locals.TryGetValue(fld, out sc))
			{
				ctx.Fail(ElaRuntimeError.UndefinedVariable, fld, mod.Machine.Assembly.GetModuleName(mod.Handle));
				return ElaObject.GetDefault();
			}

			if ((sc.Flags & ElaVariableFlags.Private) == ElaVariableFlags.Private)
			{
				ctx.Fail(ElaRuntimeError.PrivateVariable, fld);
				return ElaObject.GetDefault();
			}

			return mod.Machine.modules[mod.Handle][sc.Address];
		}
	}
	
	
	internal sealed class GetTupleInt : DispatchBinaryFun
    {
        internal GetTupleInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var t = (ElaTuple)left.Ref;

            if (right.I4 < t.Length && left.I4 > -1)
                return t.Values[right.I4];

            ctx.IndexOutOfRange(right, left);
            return ElaObject.GetDefault();
        }
    }


    internal sealed class GetRecordInt : DispatchBinaryFun
    {
        internal GetRecordInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var t = (ElaRecord)left.Ref;

            if (right.I4 < t.Length && left.I4 > -1)
                return t.values[right.I4];

            ctx.IndexOutOfRange(right, left);
            return ElaObject.GetDefault();
        }
    }


    internal sealed class GetRecordString : DispatchBinaryFun
    {
        internal GetRecordString(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var t = (ElaRecord)left.Ref;
            var index = t.GetOrdinal(right.DirectGetString());

            if (index != -1 && index < t.values.Length)
                return t.values[index];

            ctx.IndexOutOfRange(right, left);
            return ElaObject.GetDefault();
        }
    }


    internal sealed class GetListInt : DispatchBinaryFun
    {
        internal GetListInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var t = (ElaList)left.Ref;

            if (right.I4 < 0)
            {
                ctx.IndexOutOfRange(right, left);
                return ElaObject.GetDefault();
            }

            var xs = t;

            for (var i = 0; i < right.I4; i++)
            {
                xs = xs.Tail(ctx).Ref as ElaList;

                if (xs == null)
                {
                    ctx.Fail(new ElaError("InvalidList", "Invalid list definition."));
                    return ElaObject.GetDefault();
                }

                if (xs == ElaList.Empty || xs == ElaLazyList.Empty)
                {
                    ctx.IndexOutOfRange(right, left);
                    return ElaObject.GetDefault();
                }
            }

            return xs.Head(ctx);
        }
    }


    internal sealed class GetStringInt : DispatchBinaryFun
    {
        internal GetStringInt(DispatchBinaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var t = (ElaString)left.Ref;
            var val = t.GetValue();

            if (right.I4 >= val.Length || right.I4 < 0)
            {
                ctx.IndexOutOfRange(right, left);
                return ElaObject.GetDefault();
            }

            return new ElaValue(val[right.I4]);
        }
    }
    #endregion


	#region SetValue
	internal sealed class SetRecordInt : DispatchTernaryFun
    {
		internal SetRecordInt(DispatchTernaryFun[][] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue first, ElaValue second, ElaValue third, ExecutionContext ctx)
        {
            var t = (ElaRecord)first.Ref;
			var idx = second.I4;

			if (idx > -1 && idx < t.values.Length)
			{
				if (!t.flags[idx])
				{
					ctx.Fail(ElaRuntimeError.FieldImmutable, idx, first);
					return ElaObject.GetDefault();
				}

				t.values[idx] = third;
				return first;
			}
			
			ctx.IndexOutOfRange(second, first);
            return ElaObject.GetDefault();
        }
    }

	internal sealed class SetRecordString : DispatchTernaryFun
	{
		internal SetRecordString(DispatchTernaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue first, ElaValue second, ElaValue third, ExecutionContext ctx)
		{
			var t = (ElaRecord)first.Ref;
			var idx = t.GetOrdinal(second.DirectGetString());

			if (idx < 0)
			{
				ctx.IndexOutOfRange(second, first);
				return ElaObject.GetDefault();
			}

			if (!t.flags[idx])
			{
				ctx.Fail(ElaRuntimeError.FieldImmutable, idx, first);
				return ElaObject.GetDefault();
			}

			t.values[idx] = third;
			return first;
		}
	}
	#endregion


	#region Negate
	internal sealed class NegInt : DispatchUnaryFun
	{
		internal NegInt(DispatchUnaryFun[] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			return new ElaValue(-left.I4);
		}
	}

	internal sealed class NegLong : DispatchUnaryFun
	{
		internal NegLong(DispatchUnaryFun[] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			return new ElaValue(-left.GetLong());
		}
	}

	internal sealed class NegSingle : DispatchUnaryFun
	{
		internal NegSingle(DispatchUnaryFun[] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			return new ElaValue(-left.DirectGetReal());
		}
	}

	internal sealed class NegDouble : DispatchUnaryFun
	{
		internal NegDouble(DispatchUnaryFun[] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			return new ElaValue(-left.GetDouble());
		}
	}
	#endregion


	#region BitwiseNegate
	internal sealed class BinNegInt : DispatchUnaryFun
	{
		internal BinNegInt(DispatchUnaryFun[] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			return new ElaValue(~left.I4);
		}
	}

	internal sealed class BinNegLong : DispatchUnaryFun
	{
		internal BinNegLong(DispatchUnaryFun[] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			return new ElaValue(~left.GetLong());
		}
	}
	#endregion


	#region Clone
	internal sealed class CloneRec : DispatchUnaryFun
	{
		internal CloneRec(DispatchUnaryFun[] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
		{
			var rec = (ElaRecord)left.Ref;
			return new ElaValue(rec.Clone());
		}
	}
	#endregion


    #region Succ
    internal sealed class SuccInt : DispatchUnaryFun
    {
        internal SuccInt(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 + 1);
        }
    }

    internal sealed class SuccLong : DispatchUnaryFun
    {
        internal SuccLong(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() + 1);
        }
    }

    internal sealed class SuccSingle : DispatchUnaryFun
    {
        internal SuccSingle(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() + 1);
        }
    }

    internal sealed class SuccDouble : DispatchUnaryFun
    {
        internal SuccDouble(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() + 1);
        }
    }

    internal sealed class SuccChar : DispatchUnaryFun
    {
        internal SuccChar(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue((Char)(left.I4 + 1));
        }
    }

    internal sealed class SuccPredTuple : DispatchUnaryFun
    {
        internal SuccPredTuple(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            var tuple = (ElaTuple)left.Ref;
            var arr = new ElaValue[tuple.Length];

            for (var i = 0; i < tuple.Length; i++)
                arr[i] = PerformOp(tuple[i], ctx);

            return new ElaValue(new ElaTuple(arr));
        }
    }
    #endregion


    #region Pred
    internal sealed class PredInt : DispatchUnaryFun
    {
        internal PredInt(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.I4 - 1);
        }
    }

    internal sealed class PredLong : DispatchUnaryFun
    {
        internal PredLong(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.GetLong() - 1);
        }
    }

    internal sealed class PredSingle : DispatchUnaryFun
    {
        internal PredSingle(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.DirectGetReal() - 1);
        }
    }

    internal sealed class PredDouble : DispatchUnaryFun
    {
        internal PredDouble(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.GetDouble() - 1);
        }
    }

    internal sealed class PredChar : DispatchUnaryFun
    {
        internal PredChar(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue((Char)(left.I4 - 1));
        }
    }
    #endregion


    #region Min
    internal sealed class MinInt : DispatchUnaryFun
    {
        internal MinInt(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(Int32.MinValue);
        }
    }

    internal sealed class MinLong : DispatchUnaryFun
    {
        internal MinLong(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(Int64.MinValue);
        }
    }

    internal sealed class MinSingle : DispatchUnaryFun
    {
        internal MinSingle(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(Single.MinValue);
        }
    }

    internal sealed class MinDouble : DispatchUnaryFun
    {
        internal MinDouble(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(Double.MinValue);
        }
    }
    #endregion


    #region Max
    internal sealed class MaxInt : DispatchUnaryFun
    {
        internal MaxInt(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(Int32.MaxValue);
        }
    }

    internal sealed class MaxLong : DispatchUnaryFun
    {
        internal MaxLong(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(Int64.MaxValue);
        }
    }

    internal sealed class MaxSingle : DispatchUnaryFun
    {
        internal MaxSingle(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(Single.MaxValue);
        }
    }

    internal sealed class MaxDouble : DispatchUnaryFun
    {
        internal MaxDouble(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(Double.MaxValue);
        }
    }
    #endregion


    #region Length
    internal sealed class LenList : DispatchUnaryFun
    {
        internal LenList(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            var count = 0;
            var xs = (ElaList)left.Ref;

            while (xs != ElaList.Empty)
            {
                count++;
                xs = xs.Tail(ctx).Ref as ElaList;

                if (xs == null)
                {
                    ctx.Fail(new ElaError("InvalidList", "Invalid list definition."));
                    return ElaObject.GetDefault();
                }
            }

            return new ElaValue(count);
        }
    }

    internal sealed class LenTuple : DispatchUnaryFun
    {
        internal LenTuple(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(((ElaTuple)left.Ref).Length);
        }
    }

    internal sealed class LenRecord : DispatchUnaryFun
    {
        internal LenRecord(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(((ElaRecord)left.Ref).Length);
        }
    }

    internal sealed class LenString : DispatchUnaryFun
    {
        internal LenString(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(((ElaString)left.Ref).Length);
        }
    }
    #endregion


    #region Nil
    internal sealed class NilAny : DispatchUnaryFun
    {
        internal NilAny(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(ElaList.Empty);
        }
    }


    internal sealed class NilString : DispatchUnaryFun
    {
        internal NilString(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(ElaString.Empty);
        }
    }


    internal sealed class NilLazy : DispatchUnaryFun
    {
        internal NilLazy(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(ElaLazyList.Empty);
        }
    }
    #endregion


    #region Head
    internal sealed class HeadList : DispatchUnaryFun
    {
        internal HeadList(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return ((ElaList)left.Ref).Head(ctx);
        }
    }

    internal sealed class HeadString : DispatchUnaryFun
    {
        internal HeadString(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return ((ElaString)left.Ref).Head();
        }
    }
    #endregion


    #region Tail
    internal sealed class TailList : DispatchUnaryFun
    {
        internal TailList(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return ((ElaList)left.Ref).Tail(ctx);
        }
    }


    internal sealed class TailString : DispatchUnaryFun
    {
        internal TailString(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return ((ElaString)left.Ref).Tail();
        }
    }
    #endregion


    #region IsNil
    internal sealed class IsNilList : DispatchUnaryFun
    {
        internal IsNilList(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(left.Ref == ElaList.Empty || left.Ref == ElaLazyList.Empty);
        }
    }


    internal sealed class IsNilString : DispatchUnaryFun
    {
        internal IsNilString(DispatchUnaryFun[] funs) : base(funs) { }
        protected internal override ElaValue Call(ElaValue left, ExecutionContext ctx)
        {
            return new ElaValue(((ElaString)left.Ref).IsNil());
        }
    }
    #endregion


	#region Show
	internal abstract class ShowFun : DispatchBinaryFun
	{
		protected ShowFun(DispatchBinaryFun[][] funs) : base(funs) { }
		protected internal override ElaValue Call(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var str = left.DirectGetString();
			var info = String.IsNullOrEmpty(str) ? ShowInfo.Default : new ShowInfo(0, 0, str);
			var ret = Show(info, right, ctx);
			return ret == null ? ElaObject.GetDefault() : new ElaValue(ret);
		}

		protected abstract string Show(ShowInfo info, ElaValue value, ExecutionContext ctx);
	}

	internal sealed class ShowString : ShowFun
	{
		internal ShowString(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			return value.DirectGetString();
		}
	}

	internal sealed class ShowInt : ShowFun
	{
		internal ShowInt(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? value.I4.ToString(info.Format, Culture.NumberFormat) :
					value.I4.ToString(Culture.NumberFormat);
			}
			catch (FormatException)
			{
				ctx.InvalidFormat(info.Format, value);
				return null;
			}
		}
	}
	
	internal sealed class ShowLong : ShowFun
	{
		internal ShowLong(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? value.GetLong().ToString(info.Format, Culture.NumberFormat) :
					value.GetLong().ToString(Culture.NumberFormat);
			}
			catch (FormatException)
			{
				ctx.InvalidFormat(info.Format, value);
				return null;
			}
		}
	}

	internal sealed class ShowSingle : ShowFun
	{
		internal ShowSingle(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? value.DirectGetReal().ToString(info.Format, Culture.NumberFormat) :
					value.DirectGetReal().ToString(Culture.NumberFormat);
			}
			catch (FormatException)
			{
				ctx.InvalidFormat(info.Format, value);
				return null;
			}
		}
	}

	internal sealed class ShowDouble : ShowFun
	{
		internal ShowDouble(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? value.GetDouble().ToString(info.Format, Culture.NumberFormat) :
					value.GetDouble().ToString(Culture.NumberFormat);
			}
			catch (FormatException)
			{
				ctx.InvalidFormat(info.Format, value);
				return null;
			}
		}
	}

	internal sealed class ShowChar : ShowFun
	{
		internal ShowChar(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			return ((Char)value.I4).ToString();
		}
	}

	internal sealed class ShowBoolean : ShowFun
	{
		internal ShowBoolean(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			return (value.I4 == 1).ToString();
		}
	}

	internal sealed class ShowModule : ShowFun
	{
		internal ShowModule(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			return value.ToString();
		}
	}

	internal sealed class ShowThunk : ShowFun
	{
		internal ShowThunk(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			return value.Ref.IsEvaluated() ? PerformOp(new ElaValue(""), ((ElaLazy)value.Ref).Value, ctx).ToString() : "<thunk>";
		}
	}


	internal sealed class ShowUnit : ShowFun
	{
		internal ShowUnit(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			return "()";
		}
	}


	internal sealed class ShowFunction : ShowFun
	{
		internal ShowFunction(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			return value.ToString();
		}
	}


	internal sealed class ShowTuple : ShowFun
	{
		internal ShowTuple(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			var t = (ElaTuple)value.Ref;
			var sb = new StringBuilder();
			sb.Append('(');
			var c = 0;
			var maxLen = info.SequenceLength;

			foreach (var v in t)
			{
				if (maxLen > 0 && c > maxLen)
				{
					sb.Append("...");
					break;
				}

				if (c++ > 0)
					sb.Append(',');

				if (v.Ref != null)
				{
					var s = PerformOp(new ElaValue(String.Empty), v, ctx).ToString();
					sb.Append(s);
				}
			}

			if (t.Length < 2)
				sb.Append(",");
			else
				sb.Append(')');

			return sb.ToString();
		}
	}

	internal sealed class ShowRecord : ShowFun
	{
		internal ShowRecord(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			var t = (ElaRecord)value.Ref;
			var sb = new StringBuilder();
			sb.Append('{');

			var c = 0;

			foreach (var k in t.GetKeys())
			{
				if (info.SequenceLength > 0 && c > info.SequenceLength)
				{
					sb.Append("...");
					break;
				}

				if (c++ > 0)
					sb.Append(",");

				if (t.flags[c - 1])
					sb.Append("!");

				var s = PerformOp(new ElaValue(String.Empty), t[k], ctx).ToString();
				sb.AppendFormat("{0}={1}", k, s);
			}

			sb.Append('}');
			return sb.ToString();
		}
	}

	internal sealed class ShowList : ShowFun
	{
		internal ShowList(DispatchBinaryFun[][] funs) : base(funs) { }
		protected override string Show(ShowInfo info, ElaValue value, ExecutionContext ctx)
		{
			var xs = (ElaList)value.Ref;

			var sb = new StringBuilder();
			sb.Append('[');
			var c = 0;
			var maxLen = xs is ElaLazyList ? 999 : info.SequenceLength;

			while (xs != ElaList.Empty && xs != ElaLazyList.Empty)
			{
				if (maxLen > 0 && c > maxLen)
				{
					sb.Append("...");
					break;
				}

				if (c++ > 0)
					sb.Append(',');
				
				var v = xs.Head(ctx);
				var s = PerformOp(new ElaValue(String.Empty), v, ctx).ToString();
				sb.Append(s);
				xs = xs.Tail(ctx).Ref as ElaList;

				if (ctx.Failed || xs == null)
					break;
			}

			sb.Append(']');

			return sb.ToString();
		}
	}
	#endregion
}
