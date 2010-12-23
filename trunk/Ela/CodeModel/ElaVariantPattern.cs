using System;
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
		public override string ToString()
		{
			return "`" + Tag + " " + Pattern.ToStringAsFuncPattern();
		}
		#endregion


		#region Properties
		public string Tag { get; set; }

		public ElaPattern Pattern { get; set; }

		internal override ElaPatternAffinity Affinity { get { return ElaPatternAffinity.Any; } }
		#endregion
	}
}
