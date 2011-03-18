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
		internal override void ToString(StringBuilder sb)
		{
			var len = sb.Length;

			if (Pattern != null)
				Pattern.ToString(sb);

			if (Guard != null)
			{
				sb.Append(" | ");
				Guard.ToString(sb);
			}
			
			sb.Append(" = ");
			var indent = sb.Length - len;
			Expression.ToString(sb);

			if (Where != null)
			{
				var bin = (ElaBinding)Where;
				sb.AppendLine();
				sb.Append(new String(' ', indent));
				bin.ToString(sb, "where");				
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