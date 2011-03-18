using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaCastPattern : ElaPattern
	{
		#region Construction
		internal ElaCastPattern(Token tok) : base(tok, ElaNodeType.CastPattern)
		{

		}


		internal ElaCastPattern() : base(ElaNodeType.CastPattern)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
            sb.Append(VariableName);
			sb.Append(':');
			sb.Append(TypeCodeFormat.GetShortForm(TypeAffinity));
		}
		#endregion


		#region Properties
		public ElaTypeCode TypeAffinity { get; set; }

		public string VariableName { get; set; }
		#endregion
	}
}
