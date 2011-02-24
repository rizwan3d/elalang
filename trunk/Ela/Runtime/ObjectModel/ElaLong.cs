using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLong : ElaObject
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Eq | ElaTraits.Ord | ElaTraits.Bound | ElaTraits.Enum | 
			ElaTraits.Show | ElaTraits.Convert | ElaTraits.Neg | ElaTraits.Num | ElaTraits.Bit;

		public ElaLong(long value) : base(ObjectType.Long, TRAITS)
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
			return other.DataType == ObjectType.Integer ? InternalValue.CompareTo(other.I4) :
				other.DataType == ObjectType.Long ? InternalValue.CompareTo(((ElaLong)other.Ref).InternalValue) :
				other.DataType == ObjectType.Single ? ((Single)InternalValue).CompareTo(other.DirectGetReal()) :
				other.DataType == ObjectType.Double ? ((Double)InternalValue).CompareTo(other.AsDouble()) :
				-1;
		}
        #endregion


		#region Traits
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() == right.GetLong());
				else 
					return right.Ref.Equals(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
			return Default();
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() != right.GetLong());
				else
					return right.Ref.NotEquals(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
			return Default();
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() > right.GetLong());
				else
					return right.Ref.Greater(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() < right.GetLong());
				else
					return right.Ref.Lesser(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() >= right.GetLong());
				else
					return right.Ref.GreaterEquals(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() >= right.GetLong());
				else
					return right.Ref.LesserEquals(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue GetMax(ExecutionContext ctx)
		{
			return new ElaValue(Int64.MaxValue);
		}


		protected internal override ElaValue GetMin(ExecutionContext ctx)
		{
			return new ElaValue(Int64.MinValue);
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
				case ObjectType.Double: return new ElaValue((Double)InternalValue);
				case ObjectType.Long: return new ElaValue(InternalValue);
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
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() + right.GetLong());
				else
					return right.Ref.Add(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() - right.GetLong());
				else
					return right.Ref.Subtract(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() * right.GetLong());
				else
					return right.Ref.Multiply(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
				{
					var lng = right.GetLong();

					if (lng == 0)
					{
						ctx.DivideByZero(new ElaValue(this));
						return Default();
					}

					return new ElaValue(left.GetLong() / lng);
				}
				else
					return right.Ref.Divide(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
				{
					var lng = right.GetLong();

					if (lng == 0)
					{
						ctx.DivideByZero(new ElaValue(this));
						return Default();
					}

					return new ElaValue(left.GetLong() % lng);
				}
				else
					return right.Ref.Modulus(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue((Int64)Math.Pow(left.GetLong(), right.GetLong()));
				else
					return right.Ref.Power(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() & right.GetLong());
				else
					return right.Ref.BitwiseAnd(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
			return Default();
		}


		protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() | right.GetLong());
				else
					return right.Ref.BitwiseOr(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
			return Default();
		}


		protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() ^ right.GetLong());
				else
					return right.Ref.BitwiseXor(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
			return Default();
		}


		protected internal override ElaValue BitwiseNot(ExecutionContext ctx)
		{
			return new ElaValue(~InternalValue);
		}


		protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() << (int)right.GetLong());
				else
					return right.Ref.ShiftLeft(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
			return Default();
		}


		protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.LNG)
			{
				if (right.Type <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() >> (int)right.GetLong());
				else
					return right.Ref.ShiftLeft(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
			return Default();
		}
		#endregion


		#region Properties
		internal long InternalValue { get; private set; }
		#endregion
	}
}