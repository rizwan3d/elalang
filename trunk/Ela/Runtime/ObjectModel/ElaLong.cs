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
        internal override ElaValue Convert(ElaValue @this, ElaTypeCode type)
        {
            switch (type)
            {
                case ElaTypeCode.Integer: return new ElaValue((Int32)Value);
                case ElaTypeCode.Single: return new ElaValue((Single)Value);
                case ElaTypeCode.Double: return new ElaValue((Double)Value);
                case ElaTypeCode.Long: return @this;
                case ElaTypeCode.Char: return new ElaValue((Char)Value);
                default: return base.Convert(@this, type);
            }
        }


        public override bool Equals(ElaValue other)
        {
            return other.TypeCode == ElaTypeCode.Long ? other.GetLong() == Value :
                other.TypeCode == ElaTypeCode.Integer ? other.I4 == Value :
                false;
        }


		public override string GetTag()
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


		public override string ToString()
		{
			return Value.ToString();
		}
        #endregion


		#region Properties
		public long Value { get; private set; }
		#endregion
	}
}