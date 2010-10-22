using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaIgnore : ElaExpression
	{
		#region Construction
		internal ElaIgnore(Token tok) : base(tok, ElaNodeType.Ignore)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaIgnore() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		#endregion
	}
}