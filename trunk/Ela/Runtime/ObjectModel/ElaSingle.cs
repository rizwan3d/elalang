using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaSingle : ElaObject
	{
		#region Construction
		internal static readonly ElaSingle Instance = new ElaSingle();
		
		private ElaSingle() : base(ElaTypeCode.Single)
		{

		}
		#endregion


        #region Methods
        internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
        {
            switch (type)
            {
                case ElaTypeCode.Integer: return new ElaValue((Int32)@this.DirectGetReal());
                case ElaTypeCode.Single: return @this;
                case ElaTypeCode.Double: return new ElaValue((Double)@this.DirectGetReal());
                case ElaTypeCode.Long: return new ElaValue((Int64)@this.DirectGetReal());
                case ElaTypeCode.Char: return new ElaValue((Char)@this.DirectGetReal());
                default: return base.Convert(@this, type, ctx);
            }
        }


		public override string GetTag()
        {
            return "Single#";
        }


        protected internal override float AsSingle(ElaValue value)
        {
            return value.DirectGetReal();
        }


        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Single ? @this.DirectGetReal().CompareTo(other.DirectGetReal()) :
				other.TypeCode == ElaTypeCode.Integer ? @this.DirectGetReal().CompareTo((Single)other.I4) :
                other.TypeCode == ElaTypeCode.Long ? @this.DirectGetReal().CompareTo((Single)((ElaLong)other.Ref).Value) :
				other.TypeCode == ElaTypeCode.Double ? ((Double)@this.DirectGetReal()).CompareTo(other.GetDouble()) :
				-1;
		}
		#endregion
	}
}