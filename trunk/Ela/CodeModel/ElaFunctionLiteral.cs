using System;
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
		public override string ToString()
		{
			if (FunctionType == ElaFunctionType.Operator || Body.Entries.Count > 0)
				return "let " + Format.FunctionToString(this);
			else
			{
				var pat = Body.Entries[0].Pattern;

				if (pat.Type == ElaNodeType.VariablePattern &&
					((ElaVariablePattern)pat).Name[0] == '$')
					return Body.Entries[0].Expression.ToString();
				else
					return "\\" + Body.Entries[0].Pattern.ToString() + " -> " + Body.Entries[0].Expression.ToString();
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