using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFunctionCall : ElaExpression
	{
		#region Construction
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


		internal override void ToString(StringBuilder sb)
		{
			if (FlipParameters)
				sb.Append('(');

            var simple = Format.IsSimpleExpression(Target);

			if (!simple)
				sb.Append('(');
				
			sb.Append(Target.ToString());

			if (!simple)
				sb.Append(')');

			foreach (var p in Parameters)
			{
                if (Format.IsSimpleExpression(p))
					sb.Append(" " + p.ToString());
				else
                    sb.Append(" " + Format.PutInBraces(p));
			}

			if (FlipParameters)
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