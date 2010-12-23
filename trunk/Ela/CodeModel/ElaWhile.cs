using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaWhile : ElaExpression
	{
		#region Construction
		internal ElaWhile(Token tok) : base(tok, ElaNodeType.While)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaWhile() : this(null)
		{
			
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return "while " + Condition + " do " + Body;
		}
		#endregion


		#region Properties
		public ElaExpression Condition { get; set; }

		public ElaExpression Body { get; set; }
		#endregion
	}
}