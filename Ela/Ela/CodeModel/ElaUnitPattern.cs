using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaUnitPattern : ElaPattern
	{
		internal ElaUnitPattern(Token tok) : base(tok, ElaNodeType.UnitPattern)
		{

		}
        
		public ElaUnitPattern() : base(ElaNodeType.UnitPattern)
		{

		}
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append("()");
		}
        
		internal override bool CanFollow(ElaPattern pat)
		{
			return !pat.IsIrrefutable();
		}
	}
}
