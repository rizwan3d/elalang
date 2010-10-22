using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaYield : ElaExpression
	{
		#region Construction
		internal ElaYield(Token tok) : base(tok, ElaNodeType.Yield)
		{
			Flags = ElaExpressionFlags.BreaksExecution;
		}


		public ElaYield() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		#endregion
	}
}