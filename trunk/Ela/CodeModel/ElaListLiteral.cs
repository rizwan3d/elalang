using System;
using System.Collections.Generic;
using System.Text;
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


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			sb.Append('[');
			var c = 0;

			foreach (var v in Values)
			{
				if (c++ > 0)
					sb.Append(", ");

				sb.Append(v.ToString());
			}

			sb.Append(']');
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
			internal set { _values = value; }
		}
		#endregion
	}
}