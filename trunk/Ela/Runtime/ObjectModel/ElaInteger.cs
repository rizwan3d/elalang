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
        internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
        {
            switch (type)
            {
                case ElaTypeCode.Integer: return @this;
                case ElaTypeCode.Single: return new ElaValue((Single)@this.I4);
                case ElaTypeCode.Double: return new ElaValue((Double)@this.I4);
                case ElaTypeCode.Long: return new ElaValue((Int64)@this.I4);
                case ElaTypeCode.Char: return new ElaValue((Char)@this.I4);
                default: return base.Convert(@this, type, ctx);
            }
        }


		public override string GetTag()
        {
            return "Int#";
        }


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
	}
}