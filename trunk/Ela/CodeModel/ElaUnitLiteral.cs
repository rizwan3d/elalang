using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaUnitLiteral : ElaExpression
	{
		#region Construction
		internal ElaUnitLiteral(Token tok) : base(tok, ElaNodeType.UnitLiteral)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaUnitLiteral() : this(null)
		{

		}
		#endregion
		

		#region Methods
		public override string ToString()
		{
			return "()";
		}
		#endregion
	}
}
