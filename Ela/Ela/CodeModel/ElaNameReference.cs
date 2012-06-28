using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaNameReference : ElaExpression
	{
		internal ElaNameReference(Token tok) : base(tok, ElaNodeType.NameReference)
		{
			
		}
        
		public ElaNameReference() : this(null)
		{
			
		}
		
        internal override string GetName()
		{
			return Name;
        }

        internal override bool Safe()
        {
            return false;
        }

        internal override void ToString(StringBuilder sb, int ident)
		{
            if (Format.IsSymbolic(Name))
                sb.AppendFormat("({0})", Name);
            else
			    sb.Append(Name);
		}

		public string Name { get; set; }
	}
}