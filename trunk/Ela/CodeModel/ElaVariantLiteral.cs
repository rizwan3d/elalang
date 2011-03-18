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
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('`');
			sb.Append(Tag);

			if (Expression != null)
			{
				sb.Append(' ');
				Expression.ToString(sb, fmt);
			}
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }

		public string Tag { get; set; }
		#endregion
	}
}