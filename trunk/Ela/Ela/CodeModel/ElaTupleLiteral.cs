using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaTupleLiteral : ElaExpression
	{
		internal ElaTupleLiteral(Token tok, ElaNodeType type) : base(tok, type)
		{

		}
        
		internal ElaTupleLiteral(Token tok) : base(tok, ElaNodeType.TupleLiteral)
		{
			
		}
        
		public ElaTupleLiteral() : this(null)
		{

        }

        internal override bool Safe()
        {
            if (_parameters == null)
                return true;

            foreach (var p in _parameters)
                if (!p.Safe())
                    return false;

            return true;
        }

        internal override void ToString(StringBuilder sb, int ident)
		{
			sb.Append('(');
			var c = 0;

			foreach (var f in Parameters)
			{
				if (c++ > 0)
					sb.Append(',');

				f.ToString(sb, 0);
			}

			sb.Append(')');
		}
		
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
	}
}
