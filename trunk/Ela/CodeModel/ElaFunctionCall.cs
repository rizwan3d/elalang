using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFunctionCall : ElaExpression
	{
		#region Construction
		private static readonly char[] opChars = new char[] { '!', '%', '&', '*', '+', '-', '.', ':', '/', '<', '=', '>', '?', '@', '^', '|', '~', '"' };

		internal ElaFunctionCall(Token tok) : base(tok, ElaNodeType.FunctionCall)
		{
			Parameters = new List<ElaExpression>();
		}


		public ElaFunctionCall() : this(null)
		{
			
		}
		#endregion


		#region Methods
		internal override string GetName()
		{
			return Target.GetName();
		}


		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			var paren = (fmt.Flags & FmtFlags.NoParen) != FmtFlags.NoParen && FlipParameters;

			if (paren)
				sb.Append('(');

			if (Target.Type == ElaNodeType.VariableReference && Target.GetName().IndexOfAny(opChars) != -1 &&
				Parameters.Count == 2)
			{
				Parameters[0].ToString(sb, fmt);
				sb.Append(' ');
				sb.Append(Target.GetName());
				sb.Append(' ');
				Parameters[1].ToString(sb, fmt);
			}
			else
			{
				Format.PutInBraces(Target, sb, fmt);

				foreach (var p in Parameters)
				{
					sb.Append(' ');
					Format.PutInBraces(p, sb, fmt);
				}
			}

			if (paren)
				sb.Append(')');
		}
		#endregion


		#region Properties
		public ElaExpression Target { get; set; }

		public List<ElaExpression> Parameters { get; set; }

		public bool FlipParameters { get; set; }
		#endregion
	}

}