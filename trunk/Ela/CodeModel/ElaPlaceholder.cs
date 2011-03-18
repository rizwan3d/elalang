using System;
using System.Text;
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
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('_');
		}
		#endregion
	}
}