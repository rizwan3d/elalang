using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
    public sealed class ElaNewtype : ElaFunctionLiteral
    {
        internal ElaNewtype(Token tok): base(tok, ElaNodeType.Newtype)
        {

        }
        
        public ElaNewtype() : base(ElaNodeType.Newtype)
        {

        }
        
        internal override bool Safe()
        {
            return true;
        }

        internal override void ToString(StringBuilder sb, Fmt fmt)
        {
            sb.Append("type ");
            sb.Append(Name);
            sb.Append(' ');
            Body.Entries[0].Pattern.ToString(sb, fmt);
            sb.Append(" = ");
            Body.Entries[0].Expression.ToString(sb, fmt);
            sb.AppendLine();
        }
    }
}