using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaBoolean : ElaObject
	{
		#region Construction
		internal static readonly ElaBoolean Instance = new ElaBoolean();
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.Boolean), (Int32)ElaTypeCode.Boolean, false, typeof(ElaBoolean));
		
		private ElaBoolean() : base(ElaTypeCode.Boolean)
		{

		}
		#endregion


		#region Methods
        protected internal override bool AsBoolean(ElaValue value)
        {
            return value.I4 == 1;
        }


		internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Boolean ? @this.I4 - other.I4 : -1;
		}
		#endregion


		#region Operations
		protected internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.TypeId == right.TypeId && left.I4 == right.I4;
		}


		protected internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return left.TypeId != right.TypeId || left.I4 != right.I4;
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return @this.I4 == 1 ? Boolean.TrueString : Boolean.FalseString;
		}


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
            switch (type.ReflectedTypeCode)
			{
				case ElaTypeCode.Boolean: return @this;
				case ElaTypeCode.Integer: return new ElaValue(@this.I4);
				case ElaTypeCode.Single: return new ElaValue((Single)@this.I4);
				case ElaTypeCode.Double: return new ElaValue((Double)@this.I4);
				case ElaTypeCode.Long: return new ElaValue((Int64)@this.I4);
				case ElaTypeCode.Char: return new ElaValue((Char)@this.I4);
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
                    ctx.ConversionFailed(@this, type.ReflectedTypeName);
					return Default();
			}
		}
		#endregion
	}
}