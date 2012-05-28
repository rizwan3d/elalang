﻿using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaMatch : ElaExpression
	{
		#region Construction
		internal ElaMatch(Token tok, ElaNodeType type) : base(tok, type)
		{
			Entries = new List<ElaMatchEntry>();
		}


		internal ElaMatch(Token tok) : this(tok, ElaNodeType.Match)
		{
			
		}


		public ElaMatch() : this(null, ElaNodeType.Match)
		{
			
		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			ToString(sb, fmt, "match");
		}


		internal void ToString(StringBuilder sb, Fmt fmt, string keyword)
		{
			sb.Append(keyword);
			sb.Append(' ');
			var indent = keyword.Length + 1;

			Expression.ToString(sb, fmt);
			sb.AppendLine(" with ");
			var c = 0;
			var op = default(ElaPattern);

			foreach (var p in Entries)
			{
				if (c++ > 0)
				{
					sb.AppendLine();
					sb.Append(' ', 6);
				}

				if (p.Pattern == null && op != null)
					sb.Append(' ', op.ToString().Length);

				p.ToString(sb, fmt);
				
				if (p.Pattern != null) 
					op = p.Pattern;
			}
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		
		public List<ElaMatchEntry> Entries { get; private set; }
		#endregion
	}
}