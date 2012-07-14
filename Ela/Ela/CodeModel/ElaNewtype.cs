using System;
using System.Text;
using Ela.Parsing;
using System.Collections.Generic;

namespace Ela.CodeModel
{
    public sealed class ElaNewtype : ElaExpression
    {
        internal ElaNewtype(Token tok): base(tok, ElaNodeType.Newtype)
        {
            Constructors = new List<ElaConstructor>();
        }
        
        public ElaNewtype() : this(null)
        {

        }
        
        internal override bool Safe()
        {
            return true;
        }

        internal override void ToString(StringBuilder sb, int indent)
        {
            if (Extends)
                sb.Append("data ");
            else
                sb.Append("type ");

            if (Prefix != null)
            {
                sb.Append(Prefix);
                sb.Append('.');
            }

            sb.Append(Name);

            if (And != null)
            {
                sb.AppendLine();
                And.ToString(sb, indent);
            }
        }

        public List<ElaConstructor> Constructors { get; private set; }

        public bool HasBody
        {
            get { return Constructors.Count > 0; }
        }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public bool Extends { get; set; }

        public ElaNewtype And { get; set; }
    }
}