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
		public override string ToString()
		{
			return ToString(false);
		}


		internal string ToString(bool omitLet)
		{
			var sb = new StringBuilder();

			if (!omitLet)
			{
				sb.AppendLine();
				sb.Append("let ");

				if ((VariableFlags & ElaVariableFlags.Private) == ElaVariableFlags.Private)
					sb.Append("private ");

				if ((VariableFlags & ElaVariableFlags.Immutable) != ElaVariableFlags.Immutable)
					sb.Append("mutable ");
			}

			if (InitExpression.Type == ElaNodeType.FunctionLiteral &&
				(VariableFlags & ElaVariableFlags.Immutable) == ElaVariableFlags.Immutable)
			{
				var fun = (ElaFunctionLiteral)InitExpression;
                sb.Append(Format.FunctionToString(fun));
			}
			else
			{
				if (!String.IsNullOrEmpty(VariableName))
					sb.Append(VariableName);
				else
					sb.Append(Pattern.ToString());

				sb.Append(" = ");
				sb.Append(InitExpression);
			}

			if (Where != null)
                sb.Append(Format.BindingToStringAsWhere(Where));

			if (And != null)
			{
				sb.Append(" and ");
				sb.Append(And.ToString());
			}

			if (In != null)
			{
				sb.Append(" in ");
				sb.Append(In.ToString());
			}

			if (InitExpression.Type == ElaNodeType.FunctionLiteral)
				sb.Append(" end");

			return sb.ToString();
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