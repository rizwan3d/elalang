using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
    public sealed class ElaClassInstance : ElaExpression
    {
        internal ElaClassInstance(Token tok) : base(tok, ElaNodeType.ClassInstance)
        {
            
        }
        
        public ElaClassInstance() : this(null)
        {

        }

        internal override bool Safe()
        {
            return true;
        }
        
        internal override void ToString(StringBuilder sb, Fmt fmt)
        {
            sb.Append("instance ");

            if (TypeClassPrefix != null)
                sb.Append(TypeClassPrefix + ".");

            sb.Append(TypeClassName);
            sb.Append(' ');

            if (TypePrefix != null)
                sb.Append(TypePrefix + ".");
            
            sb.Append(TypeName);
            sb.AppendLine();            
            Where.ToString(sb, new Fmt(2), "where");
        }
        
        public string TypeClassPrefix { get; set; }

        public string TypeClassName { get; set; }

        public string TypeName { get; set; }

        public string TypePrefix { get; set; }

        public ElaBinding Where { get; set; }
    }
}