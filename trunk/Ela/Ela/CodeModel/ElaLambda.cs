using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
    public sealed class ElaLambda : ElaEquation
    {
        internal ElaLambda(Token tok) : base(tok, ElaNodeType.Lambda)
		{
			
		}
        
		public ElaLambda() : this(null)
		{
			
		}

        internal override bool Safe()
        {
            return false;
        }

        internal override void ToString(StringBuilder sb, int ident)
        {
            sb.Append('\\');
            
            foreach (var p in ((ElaJuxtaposition)Left).Parameters)
            {
                Format.PutInBraces(p, sb);
                sb.Append(' ');
            }

            sb.Append("-> ");
            Right.ToString(sb, 0);
        }

        internal int GetParameterCount()
        {
            return Left.Type != ElaNodeType.Juxtaposition ? 1 : ((ElaJuxtaposition)Left).Parameters.Count + 1;
        }
    }
}
