using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaHeadTailPattern : ElaTuplePattern
	{
		internal ElaHeadTailPattern(Token tok) : base(tok, ElaNodeType.HeadTailPattern)
		{
			
		}
        
		public ElaHeadTailPattern() : this(null)
		{
			
		}
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			var c = 0;

			foreach (var f in Patterns)
			{
				if (c++ > 0)
					sb.Append("::");

				Format.PutInBraces(f, sb, fmt);
			}
		}
        
		internal override bool CanFollow(ElaPattern pat)
		{
			if (pat.IsIrrefutable())
				return false;

			var fixedLen = Patterns[Patterns.Count - 1].Type == ElaNodeType.NilPattern;

			if (pat.Type == ElaNodeType.TuplePattern) //validated
			{
				var tuple = (ElaTuplePattern)pat;

				if (!fixedLen || fixedLen && tuple.Patterns.Count != Patterns.Count)
					return true;

				return CanFollow(tuple.Patterns, Patterns, tuple.Patterns.Count, Patterns.Count - 1);
			}

			if (pat.Type == ElaNodeType.HeadTailPattern) //?
			{
				var ht = (ElaHeadTailPattern)pat;
				var prevFixedLen = ht.Patterns[ht.Patterns.Count - 1].Type == ElaNodeType.NilPattern;

				if (!fixedLen && prevFixedLen ||
					fixedLen && prevFixedLen && Patterns.Count != ht.Patterns.Count)
					return true;

				if (!fixedLen && !prevFixedLen)
				{
					if (ht.Patterns.Count > Patterns.Count)
						return true;
					else
						return CanFollow(ht.Patterns, Patterns);
				}

				if (fixedLen && !prevFixedLen)
					return CanFollow(ht.Patterns, Patterns, ht.Patterns.Count, Patterns.Count - 1);
			}

			return true;
		}
	}
}
