using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaMatchEntry : ElaExpression
	{
		#region Construction
		internal ElaMatchEntry(Token tok) : base(tok, ElaNodeType.MatchEntry)
		{
			
		}


		public ElaMatchEntry() : base(ElaNodeType.MatchEntry)
		{
			
		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			var len = sb.Length;
			var indent = fmt.Indent;

			if (Pattern != null)
			{
				if ((fmt.Flags & FmtFlags.Paren) == FmtFlags.Paren)
					Format.PutInBraces(Pattern, sb, fmt);
				else
					Pattern.ToString(sb, fmt);
			}
			
			if (Guard != null)
			{
				sb.Append(" | ");
				Guard.ToString(sb, fmt);
			}
			
			sb.Append(" = ");
			indent += sb.Length - len;
			Expression.ToString(sb, new Fmt(indent, fmt.Flags));

			if (Where != null)
			{
				var bin = (ElaBinding)Where;
				sb.AppendLine();
				sb.Append(' ', indent);
				bin.ToString(sb, new Fmt(indent), "where");				
			}
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }

		public ElaPattern Pattern { get; set; }
		
		public ElaExpression Guard { get; set; }

		public ElaExpression Where { get; set; }
		#endregion
	}
}