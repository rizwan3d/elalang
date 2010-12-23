using System;
using System.Collections.Generic;
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
		public override string ToString()
		{
			return "if " + Condition.ToString() + " then " + True.ToString() + " else " + False.ToString();
		}
		#endregion


		#region Properties
		public ElaExpression Condition { get; set; }

		public ElaExpression True { get; set; }

		public ElaExpression False { get; set; }
		#endregion
	}
}