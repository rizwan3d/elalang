using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaTuplePattern : ElaPattern
	{
		internal ElaTuplePattern(Token tok) : base(tok, ElaNodeType.TuplePattern)
		{
			
		}
        
		internal ElaTuplePattern(Token tok, ElaNodeType type) : base(tok, type)
		{
			
		}
        
		public ElaTuplePattern() : this(null)
		{
			
		}
		
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

			if (pat.Type == ElaNodeType.TuplePattern || pat.Type == ElaNodeType.PatternGroup) //validated
			{
				var tuple = (ElaTuplePattern)pat;
                
                if (tuple.Patterns.Count != Patterns.Count)
                    return true;

				return CanFollow(tuple.Patterns, Patterns);
			}

			if (pat.Type == ElaNodeType.HeadTailPattern) //?
			{
				var ht = (ElaHeadTailPattern)pat;
				var fixedLen = ht.Patterns[ht.Patterns.Count - 1].Type == ElaNodeType.NilPattern;

				if (fixedLen)
				{
					if (Patterns.Count != ht.Patterns.Count)
						return true;
					else
						return CanFollow(ht.Patterns, Patterns, ht.Patterns.Count - 1, Patterns.Count);
				}

				return CanFollow(ht.Patterns, Patterns);
			}

			return true;
		}

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
	}
}