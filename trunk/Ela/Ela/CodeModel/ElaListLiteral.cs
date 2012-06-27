using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaListLiteral : ElaExpression
	{
        internal static readonly ElaListLiteral Empty = new ElaListLiteral();

		internal ElaListLiteral(Token tok) : base(tok, ElaNodeType.ListLiteral)
		{
			
		}
        
		public ElaListLiteral() : this(null)
		{
			
		}
        
        internal override bool Safe()
        {
            if (_values == null)
                return true;

            foreach (var e in _values)
                if (!e.Safe())
                    return false;

            return true;
        }
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('[');
			var c = 0;

			foreach (var v in Values)
			{
				if (c++ > 0)
					sb.Append(',');

				v.ToString(sb, fmt);
			}

			sb.Append(']');
		}

        public bool HasValues()
        {
            return _values != null && _values.Count > 0;
        }
		
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
	}
}