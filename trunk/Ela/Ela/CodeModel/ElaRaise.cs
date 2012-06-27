using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaRaise : ElaExpression
	{
		internal ElaRaise(Token tok) : base(tok, ElaNodeType.Raise)
		{
			
		}
        
		public ElaRaise() : this(null)
		{

        }

        internal override bool Safe()
        {
            return Expression == null || Expression.Safe();
        }
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			if (ErrorCode == "Failure")
			{
				sb.Append("fail ");
				Expression.ToString(sb, fmt);					
			}
			else
			{
				sb.Append("raise ");
				sb.Append(ErrorCode);
			
				if (Expression != null)
				{
					sb.Append('(');
					Expression.ToString(sb, fmt.Add(FmtFlags.NoParen));
					sb.Append(')');
				}
			}
		}
		
		public string ErrorCode { get; set; }

        public ElaExpression Expression { get; set; }
	}
}