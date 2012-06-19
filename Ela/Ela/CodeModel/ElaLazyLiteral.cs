using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaLazyLiteral : ElaExpression
	{
		internal ElaLazyLiteral(Token tok) : base(tok, ElaNodeType.LazyLiteral)
		{

		}
        
		public ElaLazyLiteral() : base(ElaNodeType.LazyLiteral)
		{

        }

        internal override bool Safe()
        {
            return false;
        }
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append("(& ");
            Expression.ToString(sb, fmt);
			sb.Append(')');
		}

        public ElaExpression Expression { get; set; }
	}
}