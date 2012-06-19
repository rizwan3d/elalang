using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaOtherwiseGuard : ElaExpression
	{
		internal ElaOtherwiseGuard(Token t) : base(t, ElaNodeType.OtherwiseGuard)
		{

		}

		public ElaOtherwiseGuard() : base(ElaNodeType.OtherwiseGuard)
		{

        }

        internal override bool Safe()
        {
            return true;
        }

		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append("else");
		}
	}
}
