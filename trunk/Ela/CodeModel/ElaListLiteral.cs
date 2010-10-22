using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaListLiteral : ElaExpression
	{
		#region Construction
		internal ElaListLiteral(Token tok) : base(tok, ElaNodeType.ListLiteral)
		{
			
		}


		public ElaListLiteral() : this(null)
		{
			
		}
		#endregion


		#region Properties
		private List<ElaExpression> _values;
		public List<ElaExpression> Values 
		{
			get
			{
				if (_values == null)
					_values = new List<ElaExpression>();

				return _values;
			}
		}



		public ElaExpression Comprehension { get; set; }
		#endregion
	}
}