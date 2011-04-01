using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLong : ElaObject
	{
		#region Construction
		public ElaLong(long value) : base(ElaTypeCode.Long)
		{
			InternalValue = value;
		}
		#endregion


        #region Methods
        public override int GetHashCode()
        {
            return InternalValue.GetHashCode();
        }


        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Integer ? InternalValue.CompareTo(other.I4) :
				other.TypeCode == ElaTypeCode.Long ? InternalValue.CompareTo(((ElaLong)other.Ref).InternalValue) :
				other.TypeCode == ElaTypeCode.Single ? ((Single)InternalValue).CompareTo(other.DirectGetReal()) :
				other.TypeCode == ElaTypeCode.Double ? ((Double)InternalValue).CompareTo(other.AsDouble()) :
				-1;
		}
        #endregion


		#region Operations
		protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() == right.GetLong());
				else 
					return right.Ref.Equal(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "equal");
			return Default();
		}


		protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() != right.GetLong());
				else
					return right.Ref.NotEqual(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "notequal");
			return Default();
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() > right.GetLong());
				else
					return right.Ref.Greater(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "greater");
			return Default();
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() < right.GetLong());
				else
					return right.Ref.Lesser(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "lesser");
			return Default();
		}


		protected internal override ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() >= right.GetLong());
				else
					return right.Ref.GreaterEqual(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "greaterequal");
			return Default();
		}


		protected internal override ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() >= right.GetLong());
				else
					return right.Ref.LesserEqual(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "lesserequal");
			return Default();
		}


		protected internal override ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Int64.MaxValue);
		}


		protected internal override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Int64.MinValue);
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(InternalValue + 1);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(InternalValue - 1);
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
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

				ctx.InvalidFormat(info.Format, @this);
				return String.Empty;
			}
		}


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ElaTypeCode.Integer: return new ElaValue((Int32)InternalValue);
				case ElaTypeCode.Single: return new ElaValue((Single)InternalValue);
				case ElaTypeCode.Double: return new ElaValue((Double)InternalValue);
				case ElaTypeCode.Long: return @this;
				case ElaTypeCode.Char: return new ElaValue((Char)InternalValue);
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
					ctx.ConversionFailed(@this, type);
					return Default();
			}
		}


		protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(-InternalValue);
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() + right.GetLong());
				else
					return right.Ref.Add(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "add");
			return Default();
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() - right.GetLong());
				else
					return right.Ref.Subtract(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "subtract");
			return Default();
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() * right.GetLong());
				else
					return right.Ref.Multiply(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "multiply");
			return Default();
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
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
			
			ctx.InvalidLeftOperand(left, right, "divide");
			return Default();
		}


		protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
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
					return right.Ref.Remainder(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "remainder");
			return Default();
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue((Int64)Math.Pow(left.GetLong(), right.GetLong()));
				else
					return right.Ref.Power(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "power");
			return Default();
		}


		protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() & right.GetLong());
				else
					return right.Ref.BitwiseAnd(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "bitwiseand");
			return Default();
		}


		protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() | right.GetLong());
				else
					return right.Ref.BitwiseOr(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "bitwiseor");
			return Default();
		}


		protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() ^ right.GetLong());
				else
					return right.Ref.BitwiseXor(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "bitwisexor");
			return Default();
		}


		protected internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(~InternalValue);
		}


		protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() << (int)right.GetLong());
				else
					return right.Ref.ShiftLeft(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "shiftleft");
			return Default();
		}


		protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return new ElaValue(left.GetLong() >> (int)right.GetLong());
				else
					return right.Ref.ShiftLeft(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "shiftright");
			return Default();
		}
		#endregion


		#region Properties
		internal long InternalValue { get; private set; }
		#endregion
	}
}