using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariantPattern : ElaPattern
	{
		#region Construction
		internal ElaVariantPattern(Token tok) : base(tok, ElaNodeType.VariantPattern)
		{

		}


		public ElaVariantPattern() : base(ElaNodeType.VariantPattern)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('`');
			sb.Append(Tag);

			if (Pattern != null)
			{
				sb.Append(' ');
				Format.PutInBraces(Pattern, sb, fmt);
			}
		}
		#endregion


		#region Properties
		public string Tag { get; set; }

		public ElaPattern Pattern { get; set; }

		internal override ElaPatternAffinity Affinity { get { return ElaPatternAffinity.Any; } }
		#endregion
	}
}
