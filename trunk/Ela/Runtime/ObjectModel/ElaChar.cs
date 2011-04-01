using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaChar : ElaObject
	{
		#region Construction
		internal static readonly ElaChar Instance = new ElaChar();

		private ElaChar() : base(ElaTypeCode.Char)
		{

		}
		#endregion


		#region Methods
        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Char ? @this.I4 - other.I4 : -1;
		}
		#endregion


		#region Operations
		protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 == right.I4) :
					right.Ref.Equal(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "equal");
			return Default();
		}


		protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 != right.I4) :
					right.Ref.NotEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "notequal");
			return Default();
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 > right.I4) :
					right.Ref.Greater(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "greater");
			return Default();
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 < right.I4) :
					right.Ref.Lesser(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "lesser");
			return Default();
		}


		protected internal override ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 >= right.I4) :
					right.Ref.GreaterEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "greaterequal");
			return Default();
		}


		protected internal override ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 <= right.I4) :
					right.Ref.LesserEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "lesserequal");
			return Default();
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return ((Char)@this.I4).ToString();
		}


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ElaTypeCode.Integer: return new ElaValue(@this.I4);
				case ElaTypeCode.Single: return new ElaValue((Single)@this.I4);
				case ElaTypeCode.Double: return new ElaValue((Double)@this.I4);
				case ElaTypeCode.Long: return new ElaValue((Int64)@this.I4);
				case ElaTypeCode.Char: return @this;
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
					ctx.ConversionFailed(@this, type);
					return Default();
			}
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.I4 + 1, this);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.I4 - 1, this);
		}
		#endregion
	}
}