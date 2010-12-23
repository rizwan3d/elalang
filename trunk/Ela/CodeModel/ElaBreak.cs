using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBreak : ElaExpression
	{
		#region Construction
		internal ElaBreak(Token tok) : base(tok, ElaNodeType.Break)
		{
			Flags = ElaExpressionFlags.ReturnsUnit | ElaExpressionFlags.BreaksExecution;
		}


		public ElaBreak() : this(null)
		{
			
		}
		#endregion

		
		#region Methods
		public override string ToString()
		{
			return "break";
		}
		#endregion
	}
}