using System;
using System.Text;
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
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append("()");
		}


		internal override bool CanFollow(ElaPattern pat)
		{
			return !pat.IsIrrefutable();
		}
		#endregion
	}
}
