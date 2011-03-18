﻿using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaFunctionLiteral : ElaExpression
	{
		#region Construction
		internal ElaFunctionLiteral(Token tok, ElaNodeType type)
			: base(tok, type)
		{

		}


		internal ElaFunctionLiteral(Token tok)
			: this(tok, ElaNodeType.FunctionLiteral)
		{

		}


		public ElaFunctionLiteral(ElaNodeType type)
			: this(null, type)
		{

		}


		public ElaFunctionLiteral()
			: this(null, ElaNodeType.FunctionLiteral)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			if (String.IsNullOrEmpty(Name))
			{
				var pat = Body.Entries[0].Pattern;

				if (pat.Type == ElaNodeType.VariablePattern && ((ElaVariablePattern)pat).Name[0] == '$')
					Body.Entries[0].Expression.ToString(sb);
				else
				{
					sb.Append('\\');
					Body.Entries[0].Pattern.ToString(sb);
					sb.Append(" -> ");
					Body.Entries[0].Expression.ToString(sb);
				}
			}
			else
			{
				var str = sb.ToString();
				var idx = str.LastIndexOf('\n');

				if (idx == -1)
					idx = str.LastIndexOf('\r');

				var indent = idx > 0 ? sb.Length - idx - 1 : sb.Length;

				sb.Append(Name);
				sb.Append(' ');
				var c = 0;
				var op = default(ElaPattern);

				foreach (var p in Body.Entries)
				{
					if (c++ > 0)
					{
						sb.AppendLine();
						sb.Append(new String(' ', indent));

						if (p.Pattern != null)
						{
							sb.Append(Name);
							sb.Append(' ');
						}
						else
							sb.Append(' ', Name.Length + 1);
					}

					if (p.Pattern == null && op != null)
						sb.Append(' ', op.ToString().Length);

					p.ToString(sb);

					if (p.Pattern != null)
						op = p.Pattern;
				}
			}
		}
		#endregion


		#region Properties
		public int ParameterCount
		{
			get
			{
				return Body.Entries[0].Pattern.Type != ElaNodeType.PatternGroup ? 1 :
					((ElaPatternGroup)Body.Entries[0].Pattern).Patterns.Count;
			}
		}

		public ElaFunctionType FunctionType { get; set; }

		public string Name { get; set; }

		public ElaMatch Body { get; set; }
		#endregion
	}
}