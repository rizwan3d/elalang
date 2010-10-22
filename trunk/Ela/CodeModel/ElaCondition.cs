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


		#region Properties
		public ElaExpression Condition { get; set; }

		public ElaExpression True { get; set; }

		public ElaExpression False { get; set; }

		public bool Unless { get; set; }
		#endregion
	}
}