using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaVariantLiteral : ElaExpression
	{
		#region Construction
		internal ElaVariantLiteral(Token tok)
			: base(tok, ElaNodeType.VariantLiteral)
		{

		}


		public ElaVariantLiteral()
			: this(null)
		{

		}
		#endregion


		#region Methods
		internal override string GetName()
		{
			return Tag;
		}


		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append(Tag);
		}
		#endregion


		#region Properties
		public string Tag { get; set; }
		#endregion
	}
}