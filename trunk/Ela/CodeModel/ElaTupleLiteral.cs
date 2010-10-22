using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaTupleLiteral : ElaExpression
	{
		#region Construction
		internal ElaTupleLiteral(Token tok, bool unit) : base(tok, ElaNodeType.TupleLiteral)
		{
			if (unit)
				Flags = ElaExpressionFlags.ReturnsUnit;
			else
				Parameters = new List<ElaExpression>();
		}


		public ElaTupleLiteral(bool unit) : this(null, unit)
		{
			
		}
		#endregion


		#region Properties
		public List<ElaExpression> Parameters { get; private set; }
		#endregion
	}
}
