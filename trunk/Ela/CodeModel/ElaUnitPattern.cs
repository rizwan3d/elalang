using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaUnitPattern : ElaPattern
	{
		#region Construction
		internal ElaUnitPattern(Token tok) : base(tok, ElaNodeType.UnitPattern)
		{

		}


		public ElaUnitPattern() : base(ElaNodeType.UnitPattern)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return "()";
		}
		#endregion
	}
}
