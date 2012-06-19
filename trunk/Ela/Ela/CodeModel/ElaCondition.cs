using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaCondition : ElaExpression
	{
		internal ElaCondition(Token tok) : base(tok, ElaNodeType.Condition)
		{
			
		}
        
		public ElaCondition() : base(ElaNodeType.Condition)
		{
			
		}

        internal override bool Safe()
        {
            return Condition.Safe() && True != null && True.Safe() && False != null && False.Safe();
        }

		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
            sb.Append("if " + Condition.ToString() + " then " + True.ToString() + " else " + 
                (False ?? (Object)"<ERROR>").ToString());
		}
		
        public ElaExpression Condition { get; set; }

		public ElaExpression True { get; set; }

		public ElaExpression False { get; set; }
	}
}