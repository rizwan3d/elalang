using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaPrimitive : ElaExpression
	{
		internal ElaPrimitive(Token tok) : base(tok, ElaNodeType.Primitive)
		{
			
		}
        
		public ElaPrimitive() : base(ElaNodeType.Primitive)
		{
			
		}

        internal override bool Safe()
        {
            return true;
        }

        internal override bool IsLiteral()
        {
            return true;
        }

        internal override void ToString(StringBuilder sb, int ident)
		{
			sb.Append(Value);
		}
		
		public ElaLiteralValue Value { get; set; }
	}
}