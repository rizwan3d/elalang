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
