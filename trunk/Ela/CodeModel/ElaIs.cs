using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaIs : ElaExpression
	{
		#region Construction
		internal ElaIs(Token t) : base(t, ElaNodeType.Is)
		{

		}


		public ElaIs() : base(ElaNodeType.Is)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			Format.PutInBraces(Expression, sb, fmt);
			sb.Append(" is ");
			Format.PutInBraces(Pattern, sb, fmt);
		}
		#endregion


		#region Properties
		public ElaPattern Pattern { get; set; }

		public ElaExpression Expression { get; set; }
		#endregion
	}
}