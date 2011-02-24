using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaSingle : ElaObject
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Eq | ElaTraits.Ord | ElaTraits.Bound | ElaTraits.Enum | ElaTraits.Show | 
			ElaTraits.Convert | ElaTraits.Neg | ElaTraits.Num;

		internal static readonly ElaSingle Instance = new ElaSingle();
		
		private ElaSingle() : base(ObjectType.Single, TRAITS)
		{

		}
		#endregion


		#region Methods
		internal override int Compare(ElaValue @this, ElaValue other)
		{
			return other.DataType == ObjectType.Single ? @this.DirectGetReal().CompareTo(other.DirectGetReal()) :
				other.DataType == ObjectType.Integer ? @this.DirectGetReal().CompareTo((Single)other.I4) :
				other.DataType == ObjectType.Long ? @this.DirectGetReal().CompareTo((Single)other.AsLong()) :
				other.DataType == ObjectType.Double ? ((Double)@this.DirectGetReal()).CompareTo(other.AsDouble()) :
				-1;
		}
		#endregion


		#region Traits
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() == right.GetReal()) :
					right.Ref.Equals(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
			return Default();
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() != right.GetReal()) :
					right.Ref.NotEquals(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
			return Default();
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() > right.GetReal()) :
					right.Ref.Greater(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() < right.GetReal()) :
					right.Ref.Lesser(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() >= right.GetReal()) :
					right.Ref.GreaterEquals(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() <= right.GetReal()) :
					right.Ref.LesserEquals(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
			return Default();
		}


		protected internal override ElaValue GetMax(ExecutionContext ctx)
		{
			return new ElaValue(float.MaxValue);
		}


		protected internal override ElaValue GetMin(ExecutionContext ctx)
		{
			return new ElaValue(float.MinValue);
		}


		internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.DirectGetReal() + 1);
		}


		internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.DirectGetReal() - 1);
		}


		internal override string Show(ElaValue @this, ExecutionContext ctx, ShowInfo info)
		{
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? @this.GetReal().ToString(info.Format, Culture.NumberFormat) :
					@this.GetReal().ToString(Culture.NumberFormat);
			}
			catch (FormatException)
			{
				if (ctx == ElaObject.DummyContext)
					throw;

				ctx.InvalidFormat(info.Format, @this);
				return String.Empty;
			}
		}


		internal override ElaValue Convert(ElaValue @this, ObjectType type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ObjectType.Integer: return new ElaValue((Int32)@this.GetReal());
				case ObjectType.Single: return new ElaValue(@this.GetReal());
				case ObjectType.Double: return new ElaValue((Double)@this.GetReal());
				case ObjectType.Long: return new ElaValue((Int64)@this.GetReal());
				case ObjectType.Char: return new ElaValue((Char)@this.GetReal());
				case ObjectType.String: return new ElaValue(Show(@this, ctx, ShowInfo.Default));
				default:
					ctx.ConversionFailed(@this, type);
					return Default();
			}
		}


		internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(-@this.DirectGetReal());
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() + right.GetReal()) :
					right.Ref.Add(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() - right.GetReal()) :
					right.Ref.Subtract(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() * right.GetReal()) :
					right.Ref.Multiply(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() / right.GetReal()) :	
					right.Ref.Divide(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue(left.GetReal() % right.GetReal()) :	
					right.Ref.Modulus(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type <= ElaMachine.REA)
				return right.Type <= ElaMachine.REA ? new ElaValue((Single)Math.Pow(left.GetReal(), right.GetReal())) :
					right.Ref.Power(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, ElaTraits.Num);
			return Default();
		}
		#endregion
	}
}