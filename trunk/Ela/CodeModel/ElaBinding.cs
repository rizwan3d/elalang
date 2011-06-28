using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

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
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			ToString(sb, fmt, "let");
		}


		internal void ToString(StringBuilder sb, Fmt fmt, string keyword)
		{
			var len = sb.Length;
			var indent = fmt.Indent;
			sb.Append(keyword);
			sb.Append(' ');

			if ((VariableFlags & ElaVariableFlags.Private) == ElaVariableFlags.Private)
				sb.Append("private ");

            if ((VariableFlags & ElaVariableFlags.Inline) == ElaVariableFlags.Inline)
                sb.Append("inline ");

			if (InitExpression.Type == ElaNodeType.FunctionLiteral)
			{
				var fun = (ElaFunctionLiteral)InitExpression;
				indent += sb.Length - len;
				fun.ToString(sb, new Fmt(indent));
				indent += fun.Name.Length + 1;				
			}
			else
			{
				if (!String.IsNullOrEmpty(VariableName))
					sb.Append(VariableName);
				else
					Format.PutInBraces(Pattern, sb, fmt);

				sb.Append(" = ");
				indent += sb.Length - len;
				sb.Append(InitExpression);
			}

			if (Where != null)
			{
				sb.AppendLine();
				sb.Append(' ', indent);
				Where.ToString(sb, new Fmt(indent), "where");
			}

            if (IsOverloaded)
            {
                sb.AppendLine();
                sb.Append(' ', indent);
                var cc = 0;
                sb.Append("on ");

                foreach (var n in OverloadNames)
                {
                    if (cc++ > 0)
                        sb.Append("->");

                    sb.Append(n);
                }
            }

			if (And != null)
			{
				sb.AppendLine();
				var nfm = new Fmt(fmt.Indent + keyword.Length - 2, fmt.Flags);
				sb.Append(' ', nfm.Indent);
				And.ToString(sb, nfm, "et");
			}

			if (In != null)
			{
				sb.AppendLine();
				sb.Append(' ', fmt.Indent);
				sb.Append(" in ");
				In.ToString(sb, fmt);
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

        public bool IsOverloaded
        {
            get { return _overloadNames != null; }
        }

        private List<String> _overloadNames;
        public List<String> OverloadNames
        {
            get 
            {
                if (_overloadNames == null)
                    _overloadNames = new List<String>();

                return _overloadNames; 
            }
        }		
		#endregion
	}
}