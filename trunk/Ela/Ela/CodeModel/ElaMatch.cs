using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaMatch : ElaExpression
	{
		internal ElaMatch(Token tok, ElaNodeType type) : base(tok, type)
		{
            
		}

		internal ElaMatch(Token tok) : this(tok, ElaNodeType.Match)
		{
			
		}

		public ElaMatch() : this(null, ElaNodeType.Match)
		{

        }

        internal override bool Safe()
        {
            return Expression.Safe() && Entries.Safe();
        }

		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
            ToString(sb, fmt, "match");
		}

        internal void ToString(StringBuilder sb, Fmt fmt, string keyword)
        {
            sb.Append(keyword);
            sb.Append(' ');
            Expression.ToString(sb, fmt);
            sb.AppendLine(" with ");
            Entries.ToString(sb, new Fmt(keyword.Length + 1));
        }

		public ElaExpression Expression { get; set; }
		
		public ElaEquationSet Entries { get; set; }
	}
}