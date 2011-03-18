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
		internal override void ToString(StringBuilder sb)
		{
			sb.Append('`');
			sb.Append(Tag);

			if (Expression != null)
				Expression.ToString(sb);
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }

		public string Tag { get; set; }
		#endregion
	}
}