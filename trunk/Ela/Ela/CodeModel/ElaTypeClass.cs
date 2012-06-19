using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
    public sealed class ElaTypeClass : ElaExpression
    {
        internal ElaTypeClass(Token tok) : base(tok, ElaNodeType.TypeClass)
        {
            Members = new List<ElaClassMember>();
        }
        
        public ElaTypeClass() : this(null)
        {

        }

        internal override bool Safe()
        {
            return true;
        }
        
        internal override void ToString(StringBuilder sb, Fmt fmt)
        {
            sb.Append("class ");
            sb.Append(Name);

            if (BuiltinName != null)
            {
                sb.Append("__internal ");
                sb.Append(BuiltinName);

                foreach (var s in Members)
                {
                    sb.Append(' ');

                    if (Format.IsSymbolic(s.Name))
                    {
                        sb.Append('(');
                        sb.Append(s.Name);
                        sb.Append(')');
                    }
                    else
                        sb.Append(s.Name);
                }
            }
            else
            {
                sb.AppendLine(" a");
                sb.Append("  where ");
                var c = 0;

                foreach (var s in Members)
                {
                    if (c++ > 0)
                    {
                        sb.AppendLine();
                        sb.Append("     et ");
                    }
                    
                    s.ToString(sb, default(Fmt));
                }

                sb.AppendLine();
            }
        }

        public string Name { get; set; }

        public string BuiltinName { get; set; }

        public List<ElaClassMember> Members { get; set; }
    }
}