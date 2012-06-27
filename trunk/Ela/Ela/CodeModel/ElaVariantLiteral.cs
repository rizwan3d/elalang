using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaVariantLiteral : ElaExpression
	{
		internal ElaVariantLiteral(Token tok) : base(tok, ElaNodeType.VariantLiteral)
		{

		}

		public ElaVariantLiteral() : this(null)
		{

        }

        internal override bool Safe()
        {
            return true;
        }

		internal override string GetName()
		{
			return Tag;
		}
        
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append(Tag);

            if (Expression != null)
            {
                sb.Append(' ');
                Format.PutInBraces(Expression, sb, default(Fmt));
            }
		}

		public string Tag { get; set; }

        public ElaExpression Expression { get; set; }
	}
}