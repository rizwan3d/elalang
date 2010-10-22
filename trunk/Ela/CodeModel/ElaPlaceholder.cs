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


		#region Properties
		public override int Placeholders { get { return 1; } }		
		#endregion
	}
}