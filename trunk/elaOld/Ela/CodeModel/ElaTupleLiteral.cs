using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaTupleLiteral : ElaExpression
	{
		#region Construction
		internal ElaTupleLiteral(Token tok, ElaNodeType type) : base(tok, type)
		{

		}


		internal ElaTupleLiteral(Token tok) : base(tok, ElaNodeType.TupleLiteral)
		{
			
		}


		public ElaTupleLiteral() : this(null)
		{
			
		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('(');
			var c = 0;

			foreach (var f in Parameters)
			{
				if (c++ > 0)
					sb.Append(',');

				f.ToString(sb, fmt);
			}

			sb.Append(')');
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
		#endregion
	}
}
