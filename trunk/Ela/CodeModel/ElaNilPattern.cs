using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaNilPattern : ElaPattern
	{
		#region Construction
		internal ElaNilPattern(Token tok) : base(tok, ElaNodeType.NilPattern)
		{

		}


		public ElaNilPattern() : base(null, ElaNodeType.NilPattern)
		{

		}
		#endregion
		

		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append("[]");
		}


		internal override bool CanFollow(ElaPattern pat)
		{
			return !pat.IsIrrefutable() && pat.Type != ElaNodeType.NilPattern;
		}
		#endregion
	}
}
