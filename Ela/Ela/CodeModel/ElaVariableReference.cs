using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariableReference : ElaExpression
	{
		internal ElaVariableReference(Token tok) : base(tok, ElaNodeType.VariableReference)
		{
			
		}
        
		public ElaVariableReference() : this(null)
		{
			
		}
		
        internal override string GetName()
		{
			return VariableName;
        }

        internal override bool Safe()
        {
            return false;
        }

		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			if (VariableName[0] != '$')
				sb.Append(VariableName);
		}

		public string VariableName { get; set; }
	}
}