using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaArrayLiteral : ElaExpression
	{
		#region Construction
		public ElaArrayLiteral() : this(null)
		{
			
		}


		internal ElaArrayLiteral(Token tok) : base(tok, ElaNodeType.ArrayLiteral)
		{
			Values = new List<ElaExpression>();
		}
		#endregion


		#region Properties
		public List<ElaExpression> Values { get; private set; }
		
		public ElaExpression Comprehension { get; set; }
		#endregion
	}
}