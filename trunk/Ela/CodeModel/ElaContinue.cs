using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaContinue : ElaExpression
	{
		#region Construction
		internal ElaContinue(Token tok) : base(tok, ElaNodeType.Continue)
		{
			Flags = ElaExpressionFlags.ReturnsUnit | ElaExpressionFlags.BreaksExecution;
		}


		public ElaContinue() : this(null)
		{
			
		}
		#endregion
	}
}