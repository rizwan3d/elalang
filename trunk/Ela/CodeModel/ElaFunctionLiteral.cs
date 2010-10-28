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


		internal ElaFunctionLiteral(Token tok) : this(tok, ElaNodeType.FunctionLiteral)
		{
			
		}


		public ElaFunctionLiteral(ElaNodeType type) : this(null, type)
		{

		}


		public ElaFunctionLiteral() : this(null, ElaNodeType.FunctionLiteral)
		{
			
		}
		#endregion


		#region Properties
		public int ParameterSlots { get; set; }

		public ElaFunctionType FunctionType { get; set; }

		public string Name { get; set; }
	
		public ElaExpression Expression { get; set; }


		public int ParameterCount 
		{ 
			get 
			{ 
				return ParameterSlots > 0 ? ParameterSlots : 
					_parameters != null ? _parameters.Count : 0; 
			} 
		}

		
		private List<ElaPattern> _parameters;
		public List<ElaPattern> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<ElaPattern>();

				return _parameters;
			}
		}
		#endregion
	}
}