using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaPlaceholder : ElaExpression
	{
		#region Construction
		internal ElaPlaceholder(Token tok) : base(tok, ElaNodeType.Placeholder)
		{
			Flags = ElaExpressionFlags.Assignable;
		}


		public ElaPlaceholder() : this(null)
		{
			
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return "_";
		}
		#endregion
	}
}