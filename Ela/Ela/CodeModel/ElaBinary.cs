using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBinary : ElaExpression
	{
		internal ElaBinary(Token tok) : base(tok, ElaNodeType.Binary)
		{

		}
        
		public ElaBinary() : base(ElaNodeType.Binary)
		{

		}
		
        internal override bool Safe()
        {
            return Left.Safe() && Right.Safe();
        }

        internal override void ToString(StringBuilder sb, int ident)
		{
            Left.ToString(sb, 0);
			sb.Append(' ');
			sb.Append(Format.OperatorAsString(Operator));
			sb.Append(' ');
			Right.ToString(sb, 0);
		}
		
        public ElaExpression Left { get; set; }

		public ElaExpression Right { get; set; }

		public ElaOperator Operator { get; set; }
	}
}