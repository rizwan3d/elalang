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
		#endregion
	}
}