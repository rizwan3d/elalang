using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaUnit : ElaObject
	{
		#region Construction
		private const string STR = "()";
		public static readonly ElaUnit Instance = new ElaUnit();

		private ElaUnit() : base(ElaTypeCode.Unit)
		{

		}
		#endregion


        #region Methods
        internal override ElaValue Convert(ElaValue @this, ElaTypeCode typeCode, ExecutionContext ctx)
        {
            switch (typeCode)
            {
                case ElaTypeCode.Unit: return @this;
                case ElaTypeCode.String: return new ElaValue(ToString());
                default: return base.Convert(@this, typeCode, ctx);
            }
        }


		public override string GetTag()
        {
            return "Unit#";
        }


        public override int GetHashCode()
        {
            return 0;
        }

	
		public override string ToString()
		{
			return STR;
		}
		#endregion
	}
}
