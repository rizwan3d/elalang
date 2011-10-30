using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaUnit : ElaObject
	{
		#region Construction
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.Unit), (Int32)ElaTypeCode.Unit, true, typeof(ElaUnit));

		private const string STR = "()";
		public static readonly ElaUnit Instance = new ElaUnit();

		private ElaUnit() : base(ElaTypeCode.Unit)
		{

		}
		#endregion


        #region Methods
        public override int GetHashCode()
        {
            return 0;
        }
        #endregion


		#region Operations
		protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.Ref == right.Ref);
		}


		protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.Ref != right.Ref);
		}


		protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return STR;
		}
		#endregion
	}
}
