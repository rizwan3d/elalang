using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaCondition : ElaExpression
	{
		#region Construction
		internal ElaCondition(Token tok) : base(tok, ElaNodeType.Condition)
		{
			
		}


		public ElaCondition() : base(ElaNodeType.Condition)
		{
			
		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
            sb.Append("if " + Condition.ToString() + " then " + True.ToString() + " else " + 
                (False ?? (Object)"<ERROR>").ToString());
		}
		#endregion


		#region Properties
		public ElaExpression Condition { get; set; }

		public ElaExpression True { get; set; }

		public ElaExpression False { get; set; }
		#endregion
	}
}