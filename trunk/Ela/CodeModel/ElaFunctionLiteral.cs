using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaFunctionLiteral : ElaExpression
	{
		#region Construction
		internal ElaFunctionLiteral(Token tok, ElaNodeType type) : base(tok, type)
		{

		}


		internal ElaFunctionLiteral(Token tok) : base(tok, ElaNodeType.FunctionLiteral)
		{
			
		}


		public ElaFunctionLiteral(ElaNodeType type) : this(null, type)
		{

		}


		public ElaFunctionLiteral() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public int Currying { get; set; }

		public ElaFunctionType FunctionType { get; set; }

		public string Name { get; set; }
				
		public ElaExpression Parameters { get; set; }

		public ElaExpression Expression { get; set; }

		private int _parameterCount = -1;
		public int ParameterCount
		{
			get
			{
				if (_parameterCount == -1)
				{
					if (Currying > 0)
						_parameterCount = Currying;
					else
						_parameterCount = Parameters.Type == ElaNodeType.VariableReference ? 1 :
							Parameters.Type == ElaNodeType.TupleLiteral ? ((ElaTupleLiteral)Parameters).Parameters.Count : 0;
				}

				return _parameterCount;
			}
		}
		#endregion
	}
}