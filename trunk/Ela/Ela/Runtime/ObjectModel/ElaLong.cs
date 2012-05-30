using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLong : ElaObject
	{
		#region Construction
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.Long), (Int32)ElaTypeCode.Long, true, typeof(ElaLong));
        
        public ElaLong(long value) : base(ElaTypeCode.Long)
		{
			Value = value;
		}
		#endregion


        #region Methods
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Integer ? Value.CompareTo(other.I4) :
				other.TypeCode == ElaTypeCode.Long ? Value.CompareTo(((ElaLong)other.Ref).Value) :
				other.TypeCode == ElaTypeCode.Single ? ((Single)Value).CompareTo(other.DirectGetReal()) :
				other.TypeCode == ElaTypeCode.Double ? ((Double)Value).CompareTo(other.GetDouble()) :
				-1;
		}
        #endregion


		#region Operations
		protected internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return left.GetLong() == right.GetLong();
				else 
					return right.Ref.Equal(left, right, ctx);
			}
			
			return false;
		}


		protected internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return left.GetLong() != right.GetLong();
				else
					return right.Ref.NotEqual(left, right, ctx);
			}
			
			return true;
		}


		protected internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return left.GetLong() > right.GetLong();
				else
					return right.Ref.Greater(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "greater");
            return false;
		}


		protected internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return left.GetLong() < right.GetLong();
				else
					return right.Ref.Lesser(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "lesser");
            return false;
		}


		protected internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return left.GetLong() >= right.GetLong();
				else
					return right.Ref.GreaterEqual(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "greaterequal");
            return false;
		}


		protected internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.LNG)
			{
				if (right.TypeId <= ElaMachine.LNG)
					return left.GetLong() >= right.GetLong();
				else
					return right.Ref.LesserEqual(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "lesserequal");
            return false;
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
			return new ElaValue(Value + 1);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Value - 1);
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
        {
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? Value.ToString(info.Format, Culture.NumberFormat) :
					Value.ToString(Culture.NumberFormat);
			}
			catch (FormatException)
			{
				if (ctx == ElaObject.DummyContext)
					throw;

				ctx.InvalidFormat(info.Format, @this);
				return String.Empty;
			}
		}


        protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
            switch (type.ReflectedTypeCode)
			{
				case ElaTypeCode.Integer: return new ElaValue((Int32)Value);
				case ElaTypeCode.Single: return new ElaValue((Single)Value);
				case ElaTypeCode.Double: return new ElaValue((Double)Value);
				case ElaTypeCode.Long: return @this;
				case ElaTypeCode.Char: return new ElaValue((Char)Value);
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
                    ctx.ConversionFailed(@this, type.ReflectedTypeName);
					return Default();
			}
		}


		protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(-Value);
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
			return new ElaValue(~Value);
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
		public long Value { get; private set; }
		#endregion
	}
}