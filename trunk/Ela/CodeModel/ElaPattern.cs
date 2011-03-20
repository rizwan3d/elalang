using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public abstract class ElaPattern : ElaExpression
	{
		#region Construction
		internal ElaPattern(Token tok, ElaNodeType type) : base(tok, type)
		{

		}


		protected ElaPattern(ElaNodeType type) : base(null, type)
		{

		}
		#endregion


		#region Methods
		internal abstract bool CanFollow(ElaPattern pat);

		internal virtual bool IsIrrefutable()
		{
			return false;
		}


		protected bool CanFollow(List<ElaPattern> prev, List<ElaPattern> next, int prevCount, int nextCount, bool ignoreLength)
		{
			var len = 0;

			if (!ignoreLength)
			{
				if (nextCount != prevCount)
					return true;

				len = nextCount;
			}
			else
				len = nextCount > prevCount ? prevCount : nextCount;
			
			for (var i = 0; i < len; i++)
				if (next[i].CanFollow(prev[i]))
					return true;

			return false;
		}
		#endregion
	}
}