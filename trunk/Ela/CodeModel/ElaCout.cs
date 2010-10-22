using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaCout : ElaExpression
	{
		#region Construction
		internal ElaCout(Token tok) : base(tok, ElaNodeType.Cout)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaCout() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		#endregion
	}
}