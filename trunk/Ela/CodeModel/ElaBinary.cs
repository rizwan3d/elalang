using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBinary : ElaExpression
	{
		#region Construction
		internal ElaBinary(Token tok) : base(tok, ElaNodeType.Binary)
		{

		}


		public ElaBinary() : base(ElaNodeType.Binary)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
            var paren = (Format.IsHiddenVar(Left) || Format.IsHiddenVar(Right)) &&
				(fmt.Flags & FmtFlags.NoParen) != FmtFlags.NoParen;

			if (paren)
				sb.Append('(');

			if (Operator == ElaOperator.Negate || Operator == ElaOperator.BitwiseNot)
			{
				sb.Append(Format.OperatorAsString(Operator));
				Left.ToString(sb, fmt);
			}
			else
			{
				Left.ToString(sb, fmt);
				sb.Append(' ');
				sb.Append(Format.OperatorAsString(Operator));
				sb.Append(' ');
				Right.ToString(sb, fmt);
			}

			if (paren)
				sb.Append(')');
		}
		#endregion


		#region Properties
		public ElaExpression Left { get; set; }

		public ElaExpression Right { get; set; }

		public ElaOperator Operator { get; set; }

		public string CustomOperator { get; set; }
		#endregion
	}
}