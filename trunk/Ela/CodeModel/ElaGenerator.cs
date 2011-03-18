﻿using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaGenerator : ElaExpression
	{
		#region Construction
		internal ElaGenerator(Token tok) : base(tok, ElaNodeType.Generator)
		{
			
		}


		public ElaGenerator() : this(null)
		{
			
		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb)		
		{
			var sbNew = new StringBuilder();
			var sel = GetSelect(this, sbNew);
			sb.Append(sel.ToString() + " \\\\ " + sbNew.ToString());
		}


		private ElaExpression GetSelect(ElaGenerator gen, StringBuilder sb)
		{
			sb.Append(gen.Pattern.ToString());
			sb.Append(" <- ");
			sb.Append(gen.Target.ToString());

			if (Guard != null)
			{
				sb.Append(" | ");
				gen.Guard.ToString(sb);
			}

			if (gen.Body.Type == ElaNodeType.Generator)
			{
				sb.Append(',');
				return GetSelect((ElaGenerator)gen.Body, sb);
			}
			else
				return gen.Body;
		}
		#endregion


		#region Properties
		public ElaPattern Pattern { get; set; }

		public ElaExpression Guard { get; set; }

		public ElaExpression Target { get; set; }

		public ElaExpression Body { get; set; }
		#endregion
	}
}