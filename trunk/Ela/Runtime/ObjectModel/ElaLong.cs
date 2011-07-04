using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLong : ElaObject
	{
		#region Construction
		public ElaLong(long value) : base(ElaTypeCode.Long)
		{
			Value = value;
		}
		#endregion


        #region Methods
        internal override string GetTag()
        {
            return "Long#";
        }


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


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ElaTypeCode.Integer: return new ElaValue((Int32)Value);
				case ElaTypeCode.Single: return new ElaValue((Single)Value);
				case ElaTypeCode.Double: return new ElaValue((Double)Value);
				case ElaTypeCode.Long: return @this;
				case ElaTypeCode.Char: return new ElaValue((Char)Value);
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
					ctx.ConversionFailed(@this, type);
					return Default();
			}
		}
		#endregion


		#region Properties
		public long Value { get; private set; }
		#endregion
	}
}