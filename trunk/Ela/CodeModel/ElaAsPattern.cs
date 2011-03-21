using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaAsPattern : ElaPattern
	{
		#region Construction
		internal ElaAsPattern(Token tok) : base(tok, ElaNodeType.AsPattern)
		{

		}


		public ElaAsPattern() : base(ElaNodeType.AsPattern)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			Format.PutInBraces(Pattern, sb, fmt);
			sb.Append('@');
			sb.Append(Name);
		}


		internal override bool IsIrrefutable()
		{
            return Pattern.IsIrrefutable();
		}


		internal override bool CanFollow(ElaPattern pat)
		{
            return Pattern.CanFollow(pat);
		}
		#endregion


		#region Properties
		public ElaPattern Pattern { get; set; }

		public string Name { get; set; }
		#endregion
	}
}