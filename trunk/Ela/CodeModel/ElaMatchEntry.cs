using System;
using Ela.Parsing;
using System.Collections.Generic;
using System.Text;

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
		public override string ToString()
		{
			var sb = new StringBuilder();

			if (Pattern != null)
				sb.Append(Pattern.ToString());

			if (Guard != null)
                sb.Append(Format.ExpressionToStringAsGuard(Guard));
			
			sb.Append(" = ");
			sb.Append(Expression.ToString());

			if (Where != null)
				sb.Append(Format.ExpressionToStringAsGuard((ElaBinding)Where));

			return sb.ToString();
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