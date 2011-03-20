using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaInteger : ElaObject
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Eq | ElaTraits.Ord | ElaTraits.Bound | 
			ElaTraits.Enum | ElaTraits.Show | ElaTraits.Convert | ElaTraits.Neg | ElaTraits.Num | ElaTraits.Bit | ElaTraits.Int;

		internal static readonly ElaInteger Instance = new ElaInteger();
		
		private ElaInteger() : base(ElaTypeCode.Integer, TRAITS)
		{
			
		}
		#endregion


		#region Methods
        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Integer ? @this.I4.CompareTo(other.I4) :
				other.TypeCode == ElaTypeCode.Long ? ((Int64)@this.I4).CompareTo(other.AsLong()) :
				other.TypeCode == ElaTypeCode.Single ? ((Single)@this.I4).CompareTo(other.DirectGetReal()) :
				other.TypeCode == ElaTypeCode.Double ? ((Double)@this.I4).CompareTo(other.AsDouble()) :
				-1;
		}
		#endregion


		#region Traits
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 == right.I4) :
					right.Ref.Equals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 != right.I4) :
					right.Ref.NotEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 > right.I4) :
					right.Ref.Greater(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 < right.I4) :
					right.Ref.Lesser(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		protected internal override ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 >= right.I4) :
					right.Ref.GreaterEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		protected internal override ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 <= right.I4) :
					right.Ref.LesserEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		protected internal override ElaValue GetMax(ExecutionContext ctx)
		{
			return new ElaValue(Int32.MaxValue);
		}


		protected internal override ElaValue GetMin(ExecutionContext ctx)
		{
			return new ElaValue(Int32.MinValue);
		}


		internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.I4 + 1);
		}


		internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.I4 - 1);
		}


		internal override string Show(ElaValue @this, ExecutionContext ctx, ShowInfo info)
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


		internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ElaTypeCode.Integer: return new ElaValue(@this.I4);
				case ElaTypeCode.Single: return new ElaValue((float)@this.I4);
				case ElaTypeCode.Double: return new ElaValue((Double)@this.I4);
				case ElaTypeCode.Long: return new ElaValue((Int64)@this.I4);
				case ElaTypeCode.Char: return new ElaValue((Char)@this.I4);
				case ElaTypeCode.String: return new ElaValue(Show(@this, ctx, ShowInfo.Default));
				default:
					ctx.ConversionFailed(@this, type);
					return Default();
			}
		}


		internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(-@this.I4);
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 + right.I4) :
					right.Ref.Add(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
				return Default();
			}
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 - right.I4) :
					right.Ref.Subtract(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
				return Default();
			}
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 * right.I4) :
					right.Ref.Multiply(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
				return Default();
			}
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
				ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
				return Default();
			}
		}


		protected internal override ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
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
					return right.Ref.Modulus(left, right, ctx);
			}
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
				return Default();
			}
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue((Int32)Math.Pow(left.I4, right.I4)) :
					right.Ref.Power(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
				return Default();
			}
		}


		protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 & right.I4) :
					right.Ref.BitwiseAnd(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
				return Default();
			}
		}


		protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 | right.I4) :
					right.Ref.BitwiseOr(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
				return Default();
			}
		}


		protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 ^ right.I4) :
					right.Ref.BitwiseXor(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
				return Default();
			}
		}


		internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(~@this.I4);
		}


		protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 << right.I4) :
					right.Ref.ShiftLeft(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
				return Default();
			}
		}


		protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.INT)
				return right.TypeId == ElaMachine.INT ? new ElaValue(left.I4 >> right.I4) :
					right.Ref.ShiftRight(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Bit);
				return Default();
			}
		}
		#endregion
	}
}