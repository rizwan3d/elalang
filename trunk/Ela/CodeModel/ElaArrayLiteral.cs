using System;
using System.Collections.Generic;
using System.Text;
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

		
		#region Methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("[| ");

			var c = 0;

			foreach (var v in Values)
			{
				if (c++ > 0)
					sb.Append(", ");

				sb.Append(v.ToString());
			}

			sb.Append(" |]");
			return sb.ToString();
		}
		#endregion


		#region Properties
		public List<ElaExpression> Values { get; internal set; }
		#endregion
	}
}