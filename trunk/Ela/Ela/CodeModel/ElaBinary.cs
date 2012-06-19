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

		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
            var paren = (Format.IsHiddenVar(Left) || Format.IsHiddenVar(Right)) &&
				(fmt.Flags & FmtFlags.NoParen) != FmtFlags.NoParen;

			if (paren)
				sb.Append('(');

			Left.ToString(sb, fmt);
			sb.Append(' ');
			sb.Append(Format.OperatorAsString(Operator));
			sb.Append(' ');
			Right.ToString(sb, fmt);
		
			if (paren)
				sb.Append(')');
		}
		
        public ElaExpression Left { get; set; }

		public ElaExpression Right { get; set; }

		public ElaOperator Operator { get; set; }
	}
}