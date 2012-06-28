using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaJuxtaposition : ElaExpression
	{
		private static readonly char[] opChars = new char[] { '!', '%', '&', '*', '+', '-', '.', ':', '/', '\\', '<', '=', '>', '?', '@', '^', '|', '~', '"' };

		internal ElaJuxtaposition(Token tok) : base(tok, ElaNodeType.Juxtaposition)
		{
			Parameters = new List<ElaExpression>();
		}
        
		public ElaJuxtaposition() : this(null)
		{
			
		}
		
		internal override string GetName()
		{
			return Target.GetName();
		}
        
        internal override bool Safe()
        {
            return false;
        }

        internal override void ToString(StringBuilder sb, int ident)
		{
			if (Target.Type == ElaNodeType.NameReference && Target.GetName().IndexOfAny(opChars) != -1 &&
				Parameters.Count == 2)
			{
				Parameters[0].ToString(sb, 0);
				sb.Append(' ');
				sb.Append(Target.GetName());
				sb.Append(' ');
				Parameters[1].ToString(sb, 0);
			}
			else
			{
				Format.PutInBraces(Target, sb);

				foreach (var p in Parameters)
				{
					sb.Append(' ');
					Format.PutInBraces(p, sb);
				}
			}
		}
		
        public ElaExpression Target { get; set; }

		public List<ElaExpression> Parameters { get; set; }

		public bool FlipParameters { get; set; }
	}

}