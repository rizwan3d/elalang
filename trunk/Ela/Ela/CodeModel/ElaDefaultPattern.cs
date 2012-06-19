using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaDefaultPattern : ElaPattern
	{
		internal ElaDefaultPattern(Token tok) : base(tok, ElaNodeType.DefaultPattern)
		{

		}
        
		public ElaDefaultPattern() : base(ElaNodeType.DefaultPattern)
		{

		}
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('_');
		}
        
		internal override bool IsIrrefutable()
		{
			return true;
		}
        
		internal override bool CanFollow(ElaPattern pat)
		{
			return !pat.IsIrrefutable();
		}
	}
}
