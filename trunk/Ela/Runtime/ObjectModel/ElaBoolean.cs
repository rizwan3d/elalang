﻿using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaBoolean : ElaObject
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Bool | ElaTraits.Show | ElaTraits.Eq | ElaTraits.Convert;

		internal static readonly ElaBoolean Instance = new ElaBoolean();

		private ElaBoolean() : base(ElaTypeCode.Boolean, TRAITS)
		{

		}
		#endregion


		#region Methods
		internal override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Boolean ? @this.I4 - other.I4 : -1;
		}
		#endregion


		#region Traits
		internal override bool Bool(ElaValue @this, ExecutionContext ctx)
		{
			return @this.I4 == 1;
		}


		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.BYT)
				return right.TypeId == ElaMachine.BYT ? new ElaValue(left.I4 == right.I4) :
					right.Ref.Equals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Eq);
				return Default();
			}
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.BYT)
				return right.TypeId == ElaMachine.BYT ? new ElaValue(left.I4 != right.I4) :
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


		internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ElaTypeCode.Boolean: return new ElaValue(@this.I4, this);
				case ElaTypeCode.Integer: return new ElaValue(@this.I4);
				case ElaTypeCode.Single: return new ElaValue((float)@this.I4);
				case ElaTypeCode.Double: return new ElaValue((double)@this.I4);
				case ElaTypeCode.Long: return new ElaValue((long)@this.I4);
				case ElaTypeCode.Char: return new ElaValue((char)@this.I4);
				case ElaTypeCode.String: return new ElaValue(Show(@this, ctx, ShowInfo.Default));
				default:
					ctx.ConversionFailed(@this, type);
					return base.Convert(type, ctx);
			}
		}
		#endregion
	}
}