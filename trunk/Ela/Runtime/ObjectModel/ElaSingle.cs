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


		#region Operations
		protected internal override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(float.MinValue);
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.DirectGetReal() + 1);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.DirectGetReal() - 1);
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? @this.DirectGetReal().ToString(info.Format, Culture.NumberFormat) :
					@this.DirectGetReal().ToString(Culture.NumberFormat);
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
				case ElaTypeCode.Integer: return new ElaValue((Int32)@this.DirectGetReal());
				case ElaTypeCode.Single: return @this;
				case ElaTypeCode.Double: return new ElaValue((Double)@this.DirectGetReal());
				case ElaTypeCode.Long: return new ElaValue((Int64)@this.DirectGetReal());
				case ElaTypeCode.Char: return new ElaValue((Char)@this.DirectGetReal());
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
					ctx.ConversionFailed(@this, type);
					return Default();
			}
		}
		#endregion
	}
}