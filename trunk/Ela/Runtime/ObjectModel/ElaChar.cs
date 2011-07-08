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
        internal override ElaValue Convert(ElaValue @this, ElaTypeCode type)
        {
            switch (type)
            {
                case ElaTypeCode.Integer: return new ElaValue(@this.I4);
                case ElaTypeCode.Single: return new ElaValue((Single)@this.I4);
                case ElaTypeCode.Double: return new ElaValue((Double)@this.I4);
                case ElaTypeCode.Long: return new ElaValue((Int64)@this.I4);
                case ElaTypeCode.Char: return @this;
                case ElaTypeCode.String: return new ElaValue(((Char)@this.I4).ToString());
                default:
                    return base.Convert(@this, type);
            }
        }


        internal override string GetTag()
        {
            return "Char#";
        }


        protected internal override char AsChar(ElaValue value)
        {
            return (Char)value.I4;
        }


        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Char ? @this.I4 - other.I4 : -1;
		}
		#endregion
	}
}