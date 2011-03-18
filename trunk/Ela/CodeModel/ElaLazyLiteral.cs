using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaLazyLiteral : ElaFunctionLiteral
	{
		#region Construction
		internal ElaLazyLiteral(Token tok) : base(tok, ElaNodeType.LazyLiteral)
		{

		}


		public ElaLazyLiteral() : base(ElaNodeType.LazyLiteral)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			sb.Append("(& "); 
			sb.Append(Body.Entries[0].Expression.ToString());
			sb.Append(')');
		}
		#endregion
	}
}