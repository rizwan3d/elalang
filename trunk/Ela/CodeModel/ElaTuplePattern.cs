using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaTuplePattern : ElaPattern
	{
		#region Construction
		internal ElaTuplePattern(Token tok) : base(tok, ElaNodeType.TuplePattern)
		{
			
		}


		internal ElaTuplePattern(Token tok, ElaNodeType type) : base(tok, type)
		{
			
		}


		public ElaTuplePattern() : this(null)
		{
			
		}
		#endregion


		#region Methods
		internal bool IsSimple()
		{
			foreach (var p in _patterns)
				if (p.Type != ElaNodeType.VariablePattern && p.Type != ElaNodeType.DefaultPattern)
					return false;

			return true;
		}


		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('(');
			var c = 0;

			foreach (var f in Patterns)
			{
				if (c++ > 0)
					sb.Append(',');

				f.ToString(sb, fmt);
			}

			sb.Append(')');
		}


		internal override bool CanFollow(ElaPattern pat)
		{
			if (pat.IsIrrefutable())
				return false;

			if (pat.Type == ElaNodeType.TuplePattern || pat.Type == ElaNodeType.PatternGroup)
			{
				var tuple = (ElaTuplePattern)pat;
				return CanFollow(tuple.Patterns, Patterns, tuple.Patterns.Count, Patterns.Count, false);
			}

			if (pat.Type == ElaNodeType.HeadTailPattern)
			{
				var ht = (ElaHeadTailPattern)pat;
				var last = ht.Patterns[ht.Patterns.Count - 1];

				if (!last.IsIrrefutable())
					return CanFollow(ht.Patterns, Patterns, ht.Patterns.Count - 1, Patterns.Count, false);

				return CanFollow(ht.Patterns, Patterns, ht.Patterns.Count, Patterns.Count, true);
			}

			return true;
		}
		#endregion


		#region Properties
		private List<ElaPattern> _patterns;
		public List<ElaPattern> Patterns
		{
			get
			{
				if (_patterns == null)
					_patterns = new List<ElaPattern>();

				return _patterns;
			}
		}


		public bool HasChildren
		{
			get { return _patterns != null; }
		}
		#endregion
	}
}