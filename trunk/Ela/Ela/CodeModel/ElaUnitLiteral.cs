using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaUnitLiteral : ElaExpression
	{
		internal ElaUnitLiteral(Token tok) : base(tok, ElaNodeType.UnitLiteral)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}

		public ElaUnitLiteral() : this(null)
		{

		}
        
        internal override bool Safe()
        {
            return true;
        }

		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append("()");
		}
	}
}
