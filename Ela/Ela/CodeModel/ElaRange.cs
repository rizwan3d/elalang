using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaRange : ElaExpression
	{
		internal ElaRange(Token tok) : base(tok, ElaNodeType.Range)
		{

		}
        
		public ElaRange() : base(ElaNodeType.Range)
		{

        }

        internal override bool Safe()
        {
            return (First == null || First.Safe()) &&
                (Second == null || Second.Safe()) &&
                (Last == null || Last.Safe());
        }
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('[');
			First.ToString(sb, fmt);

			if (Second != null)
			{
				sb.Append(',');
				Second.ToString(sb, fmt);
			}

			sb.Append("..");

			if (Last != null)
				Last.ToString(sb, fmt);

			sb.Append(']');
		}
		
		public ElaExpression First { get; set; }

		public ElaExpression Second { get; set; }

		public ElaExpression Last { get; set; }
	}
}