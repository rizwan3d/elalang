using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaBoolean : ElaObject
	{
		#region Construction
		internal static readonly ElaBoolean Instance = new ElaBoolean();

		private ElaBoolean() : base(ElaTypeCode.Boolean)
		{

		}
		#endregion


        #region Methods
        internal override ElaValue Convert(ElaValue @this, ElaTypeCode typeCode)
        {
            switch (typeCode)
            {
                case ElaTypeCode.Boolean: return @this;
                case ElaTypeCode.Integer: return new ElaValue(@this.I4);
                case ElaTypeCode.Single: return new ElaValue((Single)@this.I4);
                case ElaTypeCode.Double: return new ElaValue((Double)@this.I4);
                case ElaTypeCode.Long: return new ElaValue((Int64)@this.I4);
                case ElaTypeCode.Char: return new ElaValue((Char)@this.I4);
                default:
                    return base.Convert(@this, typeCode);
            }
        }


		public override string GetTag()
        {
            return "Bool#";
        }
        
        
        protected internal override bool AsBoolean(ElaValue value)
        {
            return value.I4 == 1;
        }


		internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Boolean ? @this.I4 - other.I4 : -1;
		}
		#endregion
	}
}