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
            return false;
        }

        internal override void ToString(StringBuilder sb, int ident)
		{
			if (ErrorCode == "Failure")
			{
				sb.Append("fail (");
                Expression.ToString(sb, 0);
                sb.Append(')');				
			}
			else
			{
				sb.Append("raise ");
				sb.Append(ErrorCode);

                if (Expression != null)
                {
                    sb.Append('(');
                    Expression.ToString(sb, 0);
                    sb.Append(')');
                }
			}
		}
		
		public string ErrorCode { get; set; }

        public ElaExpression Expression { get; set; }
	}
}