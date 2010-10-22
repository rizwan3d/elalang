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


		#region Properties
		public ElaExpression Condition { get; set; }

		public ElaExpression Body { get; set; }

		public ElaWhileType WhileType { get; set; } 
		#endregion
	}
}