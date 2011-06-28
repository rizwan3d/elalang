using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaInteger : ElaObject
	{
		#region Construction
		internal static readonly ElaInteger Instance = new ElaInteger();
		
		private ElaInteger() : base(ElaTypeCode.Integer)
		{
			
		}
		#endregion


		#region Methods
        protected internal override int AsInteger(ElaValue value)
        {
            return value.I4;
        }


        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Integer ? @this.I4.CompareTo(other.I4) :
				other.TypeCode == ElaTypeCode.Long ? ((Int64)@this.I4).CompareTo(((ElaLong)other.Ref).Value) :
				other.TypeCode == ElaTypeCode.Single ? ((Single)@this.I4).CompareTo(other.DirectGetReal()) :
				other.TypeCode == ElaTypeCode.Double ? ((Double)@this.I4).CompareTo(other.GetDouble()) :
				-1;
		}
		#endregion


		#region Operations
        protected internal override string GetTag(ExecutionContext ctx)
        {
            return "Int";
        }


		protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            if (left.TypeId == right.TypeId)
                return new ElaValue(left.I4 == right.I4);
            else
                return base.Equal(left, right, ctx);
		}


		protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 != right.I4) :
					right.Ref.NotEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "notequal");
			return Default();
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 > right.I4) :
					right.Ref.Greater(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "greater");
			return Default();
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 < right.I4) :
					right.Ref.Lesser(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "lesser");
			return Default();
		}


		protected internal override ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 >= right.I4) :
					right.Ref.GreaterEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "greaterequal");
			return Default();
		}


		protected internal override ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 <= right.I4) :
					right.Ref.LesserEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "lesserequal");
			return Default();
		}


		protected internal override ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Int32.MaxValue);
		}


		protected internal override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Int32.MinValue);
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.I4 + 1);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.I4 - 1);
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
        {
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? @this.I4.ToString(info.Format, Culture.NumberFormat) :
					@this.I4.ToString(Culture.NumberFormat);
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
				case ElaTypeCode.Integer: return @this;
				case ElaTypeCode.Single: return new ElaValue((Single)@this.I4);
				case ElaTypeCode.Double: return new ElaValue((Double)@this.I4);
				case ElaTypeCode.Long: return new ElaValue((Int64)@this.I4);
				case ElaTypeCode.Char: return new ElaValue((Char)@this.I4);
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
					ctx.ConversionFailed(@this, type);
					return Default();
			}
		}


		protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(-@this.I4);
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            if (left.TypeId == right.TypeId)
                return new ElaValue(left.I4 + right.I4);
            else
                return base.Add(left, right, ctx);

            //    return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 + right.I4) :
            //        right.Ref.Add(left, right, ctx);

            //ctx.InvalidLeftOperand(left, right, "add");
            //return Default();
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 - right.I4) :
					right.Ref.Subtract(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "subtract");
			return Default();
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 * right.I4) :
					right.Ref.Multiply(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "multiply");
			return Default();
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
			{
				if (right.TypeId == ElaMachine.INT)
				{
					if (right.I4 == 0)
					{
						ctx.DivideByZero(left);
						return Default();
					}
					else
						return new ElaValue(left.I4 / right.I4);
				}
				else
					return right.Ref.Divide(left, right, ctx);
			}
			else
			{
				ctx.InvalidLeftOperand(left, right, "divide");
				return Default();
			}
		}


		protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
			{
				if (right.TypeId == ElaMachine.INT)
				{
					if (right.I4 == 0)
					{
						ctx.DivideByZero(left);
						return Default();
					}
					else
						return new ElaValue(left.I4 % right.I4);
				}
				else
					return right.Ref.Remainder(left, right, ctx);
			}
			else
			{
				ctx.InvalidLeftOperand(left, right, "remainder");
				return Default();
			}
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue((Int32)Math.Pow(left.I4, right.I4)) :
					right.Ref.Power(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "power");
			return Default();
		}


		protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 & right.I4) :
					right.Ref.BitwiseAnd(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "bitwiseand");
			return Default();
		}


		protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 | right.I4) :
					right.Ref.BitwiseOr(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "bitwiseor");
			return Default();
		}


		protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 ^ right.I4) :
					right.Ref.BitwiseXor(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "bitwisexor");
			return Default();
		}


		protected internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(~@this.I4);
		}


		protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 << right.I4) :
					right.Ref.ShiftLeft(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "shiftleft");
			return Default();
		}


		protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 >> right.I4) :
					right.Ref.ShiftRight(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "shiftright");
			return Default();
		}
		#endregion
	}
}