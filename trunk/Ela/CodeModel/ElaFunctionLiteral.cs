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
		public int ParameterSlots { get; set; }

		public ElaFunctionType FunctionType { get; set; }

		public string Name { get; set; }
				
		public ElaExpression Parameters { get; set; }

		public ElaExpression Expression { get; set; }

		private int _parameterCount = -1;
		public virtual int ParameterCount
		{
			get
			{
				if (_parameterCount == -1)
				{
					if (ParameterSlots > 0)
						_parameterCount = ParameterSlots;
					else 
					{
						if (Parameters == null)
							_parameterCount = 0;
						else if (Parameters.Type == ElaNodeType.VariableReference)
							_parameterCount = 1;
						else if (Parameters.Type == ElaNodeType.TupleLiteral)
						{
							var t = (ElaTupleLiteral)Parameters;
							_parameterCount = t.Parameters != null ? t.Parameters.Count : 0;
						}
						else
							_parameterCount = 0;
					}
				}

				return _parameterCount;
			}
		}
		#endregion
	}
}