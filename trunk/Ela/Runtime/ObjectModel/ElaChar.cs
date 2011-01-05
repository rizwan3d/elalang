using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaChar : ElaObject
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Show | ElaTraits.Eq | ElaTraits.Ord | ElaTraits.Convert | ElaTraits.Enum;
		
		internal static readonly ElaChar Instance = new ElaChar();

		private ElaChar() : base(ObjectType.Char, TRAITS)
		{

		}
		#endregion


		#region Traits
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.CHR)
				return right.Type == ElaMachine.CHR ? new ElaValue(left.I4 == right.I4) :
					right.Ref.Equals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
				return Default();
			}
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.CHR)
				return right.Type == ElaMachine.CHR ? new ElaValue(left.I4 != right.I4) :
					right.Ref.NotEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
				return Default();
			}
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.CHR)
				return right.Type == ElaMachine.CHR ? new ElaValue(left.I4 > right.I4) :
					right.Ref.Greater(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.CHR)
				return right.Type == ElaMachine.CHR ? new ElaValue(left.I4 < right.I4) :
					right.Ref.Lesser(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		protected internal override ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.CHR)
				return right.Type == ElaMachine.CHR ? new ElaValue(left.I4 >= right.I4) :
					right.Ref.GreaterEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		protected internal override ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.CHR)
				return right.Type == ElaMachine.CHR ? new ElaValue(left.I4 <= right.I4) :
					right.Ref.LesserEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Ord);
				return Default();
			}
		}


		internal override string Show(ElaValue @this, ExecutionContext ctx, ShowInfo info)
		{
			return ((Char)@this.I4).ToString();
		}


		internal override ElaValue Convert(ElaValue @this, ObjectType type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ObjectType.Integer: return new ElaValue(@this.I4);
				case ObjectType.Single: return new ElaValue((float)@this.I4);
				case ObjectType.Double: return new ElaValue((double)@this.I4);
				case ObjectType.Long: return new ElaValue((long)@this.I4);
				case ObjectType.Char: return new ElaValue(@this.I4, this);
				case ObjectType.String: return new ElaValue(Show(@this, ctx, ShowInfo.Default));
				default:
					ctx.ConversionFailed(@this, type);
					return Default();
			}
		}


		internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.I4 + 1, this);
		}


		internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.I4 - 1, this);
		}
		#endregion
	}
}