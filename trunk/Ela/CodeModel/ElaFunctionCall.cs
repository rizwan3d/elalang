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


		public override string ToString()
		{
			var sb = new StringBuilder();

			if (FlipParameters)
				sb.Append('(');

			var simple = Target.IsSimpleExpression();

			if (!simple)
				sb.Append('(');
				
			sb.Append(Target.ToString());

			if (!simple)
				sb.Append(')');

			foreach (var p in Parameters)
			{
				if (p.IsSimpleExpression())
					sb.Append(" " + p.ToString());
				else
					sb.Append(" " + p.PutInBraces());
			}

			if (FlipParameters)
				sb.Append(')');
			
			return sb.ToString();
		}
		#endregion


		#region Properties
		public ElaExpression Target { get; set; }

		public List<ElaExpression> Parameters { get; set; }

		public bool FlipParameters { get; set; }
		#endregion
	}

}