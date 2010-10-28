using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariantLiteral : ElaExpression
	{
		#region Construction
		internal ElaVariantLiteral(Token tok) : base(tok, ElaNodeType.VariantLiteral)
		{
			
		}


		public ElaVariantLiteral() : base(ElaNodeType.VariantLiteral)
		{
			
		}
		#endregion


		#region Properties
		private List<ElaExpression> _parameters;
		public List<ElaExpression> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<ElaExpression>();

				return _parameters;
			}
		}

		public bool HasParameters { get { return _parameters != null; } }

		public string Name { get; set; }
		#endregion
	}
}