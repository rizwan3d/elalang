using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
    public sealed class ElaLetBinding : ElaExpression
    {
        internal ElaLetBinding(Token tok) : base(tok, ElaNodeType.LetBinding)
        {

        }

        public ElaLetBinding() : base(ElaNodeType.LetBinding)
        {

        }

        internal override bool Safe()
        {
            return false;
        }

        internal override void ToString(StringBuilder sb, Fmt fmt)
        {
            sb.Append("let ");
            Equations.ToString(sb, fmt);
            sb.Append(" in ");
            Expression.ToString(sb, fmt);
        }

        public ElaEquationSet Equations { get; set; }

        public ElaExpression Expression { get; set; }
    }
}