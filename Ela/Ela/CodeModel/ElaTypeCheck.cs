using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
    public sealed class ElaTypeCheck : ElaExpression
    {
        internal ElaTypeCheck(Token tok) : base(tok, ElaNodeType.TypeCheck)
        {

        }

        internal ElaTypeCheck() : base(ElaNodeType.TypeCheck)
        {

        }

        internal override void ToString(StringBuilder sb, int ident)
        {
            sb.Append('?');

            if (_traits != null)
            {
                sb.Append('(');
                var c = 0;

                foreach (var ti in _traits)
                {
                    if (c++ > 0)
                        sb.Append(' ');

                    sb.Append(ti.ToString());
                }

                sb.Append(')');
            }
            else
            {
                if (TypePrefix != null)
                {
                    sb.Append(TypePrefix);
                    sb.Append('.');
                }

                sb.Append(TypeName);
            }
        }

        public string TypeName { get; set; }

        public string TypePrefix { get; set; }

        private List<TraitInfo> _traits;
        public List<TraitInfo> Traits
        {
            get { return _traits ?? (_traits = new List<TraitInfo>()); }
        }
    }
}
