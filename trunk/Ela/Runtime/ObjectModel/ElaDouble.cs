using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaDouble : ElaObject
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Eq | ElaTraits.Ord | ElaTraits.Bound | ElaTraits.Enum | ElaTraits.Show | 
			ElaTraits.Convert | ElaTraits.Neg | ElaTraits.Num;

		public ElaDouble(double value) : base(ObjectType.Double, TRAITS)
		{
			InternalValue = value;
		}
		#endregion


        #region Methods
        public override int GetHashCode()
        {
            return InternalValue.GetHashCode();
        }


		internal override int Compare(ElaValue @this, ElaValue other)
		{
			return other.DataType == ObjectType.Single ? InternalValue.CompareTo(other.AsSingle()) :
				other.DataType == ObjectType.Integer ? InternalValue.CompareTo((Double)other.I4) :
				other.DataType == ObjectType.Long ? InternalValue.CompareTo((Double)other.AsLong()) :
				other.DataType == ObjectType.Double ? InternalValue.CompareTo(((ElaDouble)other.Ref).InternalValue) :
				-1;
		}
        #endregion


        #region Traits
        protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() == right.GetDouble());
				else
					return right.Ref.Equals(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
			return Default();
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() != right.GetDouble());
				else
					return right.Ref.NotEquals(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
			return Default();
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() > right.GetDouble());
				else
					return right.Ref.Greater(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() < right.GetDouble());
				else
					return right.Ref.Lesser(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() >= right.GetDouble());
				else
					return right.Ref.GreaterEquals(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() >= right.GetDouble());
				else
					return right.Ref.LesserEquals(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}
		

		protected internal override ElaValue GetMax(ExecutionContext ctx)
		{
			return new ElaValue(Double.MaxValue);
		}


		protected internal override ElaValue GetMin(ExecutionContext ctx)
		{
			return new ElaValue(Double.MinValue);
		}


		protected internal override ElaValue Successor(ExecutionContext ctx)
		{
			return new ElaValue(InternalValue + 1);
		}


		protected internal override ElaValue Predecessor(ExecutionContext ctx)
		{
			return new ElaValue(InternalValue - 1);
		}


		protected internal override string Show(ExecutionContext ctx, ShowInfo info)
		{
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? InternalValue.ToString(info.Format, Culture.NumberFormat) :
					InternalValue.ToString(Culture.NumberFormat);
			}
			catch (FormatException)
			{
				if (ctx == ElaObject.DummyContext)
					throw;

				ctx.InvalidFormat(info.Format, new ElaValue(this));
				return String.Empty;
			}
		}
		
		
		protected internal override ElaValue Convert(ObjectType type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ObjectType.Integer: return new ElaValue((Int32)InternalValue);
				case ObjectType.Single: return new ElaValue((float)InternalValue);
				case ObjectType.Double: return new ElaValue(this);
				case ObjectType.Long: return new ElaValue((long)InternalValue);
				case ObjectType.Char: return new ElaValue((Char)InternalValue);
				case ObjectType.String: return new ElaValue(Show(ctx, ShowInfo.Default));
				default:
					ctx.ConversionFailed(new ElaValue(this), type);
					return Default();
			}
		}


		protected internal override ElaValue Negate(ExecutionContext ctx)
		{
			return new ElaValue(-InternalValue);
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() + right.GetDouble());
				else
					return right.Ref.Add(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() - right.GetDouble());
				else
					return right.Ref.Subtract(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() * right.GetDouble());
				else
					return right.Ref.Multiply(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() / right.GetDouble());
				else
					return right.Ref.Divide(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() % right.GetDouble());
				else
					return right.Ref.Modulus(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.DBL)
			{
				if (right.Type <= ElaMachine.DBL)
					return new ElaValue(Math.Pow(left.GetDouble(), right.GetDouble()));
				else
					return right.Ref.Power(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}

		#endregion


		#region Properties
		internal double InternalValue { get; private set; }
		#endregion
	}
}