using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaIs : ElaExpression
	{
		internal ElaIs(Token t) : base(t, ElaNodeType.Is)
		{

		}
        
		public ElaIs() : base(ElaNodeType.Is)
		{

        }

        internal override bool Safe()
        {
            return Expression.Safe();
        }

		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			Format.PutInBraces(Expression, sb, fmt);
			sb.Append(" is ");
			Format.PutInBraces(Pattern, sb, fmt);
		}

		public ElaPattern Pattern { get; set; }

		public ElaExpression Expression { get; set; }
	}
}