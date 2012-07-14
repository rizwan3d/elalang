using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
    public sealed class ElaConstructor : ElaExpression
    {
        internal ElaConstructor(Token tok) : base(tok, ElaNodeType.Constructor)
        {
            Expressions = new List<ElaExpression>();
        }

        public ElaConstructor() : this(null)
        {

        }

        internal override void ToString(StringBuilder sb, int ident)
        {
            sb.Append(Name);

            if (Expressions.Count > 0)
            {
                foreach (var e in Expressions)
                {
                    sb.Append(' ');
                    Format.PutInBraces(e, sb);
                }
            }
        }

        internal override bool Safe()
        {
            return true;
        }

        public string Name { get; set; }

        public List<ElaExpression> Expressions { get; set; }
    }
}