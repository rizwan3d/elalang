using System;
using System.Collections.Generic;
using Ela.Parsing;
using System.Text;

namespace Ela.CodeModel
{
	public sealed class ElaBinding : ElaExpression
	{
		#region Construction
		internal ElaBinding(Token tok) : base(tok, ElaNodeType.Binding)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaBinding() : this(null)
		{
			
		}
		#endregion
		

		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			ToString(sb, "let");
		}


		internal void ToString(StringBuilder sb, string keyword)
		{
			var len = sb.Length;
			var indent = 0;
			sb.Append(keyword);
			sb.Append(' ');

			if ((VariableFlags & ElaVariableFlags.Private) == ElaVariableFlags.Private)
				sb.Append("private ");
			
			if (InitExpression.Type == ElaNodeType.FunctionLiteral)
			{
				var fun = (ElaFunctionLiteral)InitExpression;
				indent = sb.Length - len + fun.Name.Length + 1;
				fun.ToString(sb);
			}
			else
			{
				if (!String.IsNullOrEmpty(VariableName))
					sb.Append(VariableName);
				else
					Pattern.ToString(sb);

				sb.Append(" = ");
				indent = sb.Length - len;
				sb.Append(InitExpression);
			}

			if (Where != null)
			{
				sb.AppendLine();
				sb.Append(new String(' ', indent));
				Where.ToString(sb, "where");
			}

			if (And != null)
			{
				sb.AppendLine();
				sb.Append(" et ");
				And.ToString(sb);
			}

			if (In != null)
			{
				sb.AppendLine();
				sb.Append(" in ");
				sb.Append(In.ToString());
			}
		}
		#endregion


		#region Properties
		public string VariableName { get; set; }

		public ElaVariableFlags VariableFlags { get; set; }

		public ElaExpression InitExpression { get; set; }

		public ElaPattern Pattern { get; set; }

		public ElaBinding Where { get; set; }
		
		public ElaBinding And { get; set; }

		public ElaExpression In { get; set; }
		#endregion
	}
}