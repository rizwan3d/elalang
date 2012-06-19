using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public abstract class ElaPattern : ElaExpression
	{
		internal ElaPattern(Token tok, ElaNodeType type) : base(tok, type)
		{

		}
        
		protected ElaPattern(ElaNodeType type) : base(null, type)
		{

		}
		
        internal abstract bool CanFollow(ElaPattern pat);
        
        internal override bool Safe()
        {
            return true;
        }

		internal virtual bool IsIrrefutable()
		{
			return false;
		}
        
		protected bool CanFollow(List<ElaPattern> prev, List<ElaPattern> next)
		{
			return CanFollow(prev, next, prev.Count, next.Count);
		}
        
		protected bool CanFollow(List<ElaPattern> prev, List<ElaPattern> next, int prevCount, int nextCount)
		{
			var len = nextCount > prevCount ? prevCount : nextCount;
			
			for (var i = 0; i < len; i++)
				if (next[i].CanFollow(prev[i]))
					return true;

			return false;
		}
	}
}