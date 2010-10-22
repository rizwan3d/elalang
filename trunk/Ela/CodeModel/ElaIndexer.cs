using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaIndexer : ElaExpression
	{
		#region Construction
		internal ElaIndexer(Token tok) : base(tok, ElaNodeType.Indexer)
		{
			Flags = ElaExpressionFlags.Assignable;
		}


		public ElaIndexer() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public ElaExpression Index { get; set; }

		public ElaExpression TargetObject { get; set; }
		#endregion
	}
}