using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaThrow : ElaExpression
	{
		#region Construction
		internal ElaThrow(Token tok) : base(tok, ElaNodeType.Throw)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaThrow() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		#endregion
	}
}