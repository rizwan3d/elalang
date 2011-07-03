using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaDouble : ElaObject
	{
		#region Construction
		public ElaDouble(double value) : base(ElaTypeCode.Double)
		{
			Value = value;
		}
		#endregion
		  

        #region Methods
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Single ? Value.CompareTo(other.DirectGetReal()) :
				other.TypeCode == ElaTypeCode.Integer ? Value.CompareTo((Double)other.I4) :
                other.TypeCode == ElaTypeCode.Long ? Value.CompareTo((Double)((ElaLong)other.Ref).Value) :
				other.TypeCode == ElaTypeCode.Double ? Value.CompareTo(((ElaDouble)other.Ref).Value) :
				-1;
		}
        #endregion


        #region Operations
        protected internal override ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Double.MaxValue);
		}


		protected internal override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Double.MinValue);
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Value + 1);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Value - 1);
		}


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

				ctx.InvalidFormat(info.Format, new ElaValue(this));
				return String.Empty;
			}
		}
		
		
		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			switch (type)
			{
				case ElaTypeCode.Integer: return new ElaValue((Int32)Value);
				case ElaTypeCode.Single: return new ElaValue((float)Value);
				case ElaTypeCode.Double: return @this;
				case ElaTypeCode.Long: return new ElaValue((Int64)Value);
				case ElaTypeCode.Char: return new ElaValue((Char)Value);
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
					ctx.ConversionFailed(@this, type);
					return Default();
			}
		}
		#endregion


		#region Properties
		internal double Value { get; private set; }
		#endregion
	}
}