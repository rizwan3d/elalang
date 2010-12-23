using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaBoolean : ElaObject
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Bool | ElaTraits.Show | ElaTraits.Eq | ElaTraits.Convert;

		internal static readonly ElaBoolean Instance = new ElaBoolean();

		private ElaBoolean() : base(ObjectType.Boolean, TRAITS)
		{

		}
		#endregion


		#region Traits
		internal override bool Bool(ElaValue @this, ExecutionContext ctx)
		{
			return @this.I4 == 1;
		}


		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.BYT)
				return right.Type == ElaMachine.BYT ? new ElaValue(left.I4 == right.I4) :
					right.Ref.Equals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
				return Default();
			}
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.BYT)
				return right.Type == ElaMachine.BYT ? new ElaValue(left.I4 != right.I4) :
					right.Ref.NotEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
				return Default();
			}
		}


		internal override string Show(ElaValue @this, ExecutionContext ctx, ShowInfo info)
		{
			return @this.I4 == 1 ? Boolean.TrueString : Boolean.FalseString;
		}


		internal override ElaValue Convert(ElaValue @this, ObjectType type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ObjectType.Boolean: return new ElaValue(@this.I4, this);
				case ObjectType.Integer: return new ElaValue(@this.I4);
				case ObjectType.Single: return new ElaValue((float)@this.I4);
				case ObjectType.Double: return new ElaValue((double)@this.I4);
				case ObjectType.Long: return new ElaValue((long)@this.I4);
				case ObjectType.Char: return new ElaValue((char)@this.I4);
				case ObjectType.String: return new ElaValue(Show(@this, ctx, ShowInfo.Default));
				default:
					ctx.ConversionFailed(@this, type);
					return base.Convert(type, ctx);
			}
		}
		#endregion
	}
}