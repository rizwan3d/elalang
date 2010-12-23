using System;
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
		public override string ToString()
		{
			return "(& " + Body.Entries[0].Expression.ToString() + ")";
		}
		#endregion
	}
}