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
        public override ElaPatterns GetSupportedPatterns()
        {
            return ElaPatterns.None;
        }


		internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Char ? @this.I4 - other.I4 : -1;
		}
		#endregion


		#region Operations
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 == right.I4) :
					right.Ref.Equals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, "equal");
				return Default();
			}
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 != right.I4) :
					right.Ref.NotEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, "notequal");
				return Default();
			}
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 > right.I4) :
					right.Ref.Greater(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, "greater");
				return Default();
			}
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 < right.I4) :
					right.Ref.Lesser(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, "lesser");
				return Default();
			}
		}


		protected internal override ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 >= right.I4) :
					right.Ref.GreaterEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, "greaterequal");
				return Default();
			}
		}


		protected internal override ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.CHR)
				return right.TypeId == ElaMachine.CHR ? new ElaValue(left.I4 <= right.I4) :
					right.Ref.LesserEquals(left, right, ctx);
			else
			{
				ctx.InvalidLeftOperand(left, right, "lesserequal");
				return Default();
			}
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
				case ElaTypeCode.Single: return new ElaValue((float)@this.I4);
				case ElaTypeCode.Double: return new ElaValue((double)@this.I4);
				case ElaTypeCode.Long: return new ElaValue((long)@this.I4);
				case ElaTypeCode.Char: return new ElaValue(@this.I4, this);
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