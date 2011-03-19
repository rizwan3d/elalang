using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaHeadTailPattern : ElaTuplePattern
	{
		#region Construction
		internal ElaHeadTailPattern(Token tok) : base(tok, ElaNodeType.HeadTailPattern)
		{
			
		}


		public ElaHeadTailPattern() : this(null)
		{
			
		}
		#endregion


		#region Methods
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

			var fixedLen = Patterns[Patterns.Count - 1].IsIrrefutable();

			if (pat.Type == ElaNodeType.TuplePattern)
			{
				var tuple = (ElaTuplePattern)pat;

				return CanFollow(tuple.Patterns, Patterns, tuple.Patterns.Count, 
					fixedLen ? Patterns.Count - 1 : Patterns.Count, !fixedLen);
			}

			if (pat.Type == ElaNodeType.HeadTailPattern)
			{
				var ht = (ElaHeadTailPattern)pat;
				var htFixedLen = ht.Patterns[ht.Patterns.Count - 1].IsIrrefutable();

				return CanFollow(ht.Patterns, Patterns, 
					htFixedLen ? ht.Patterns.Count - 1 : ht.Patterns.Count, 
					fixedLen ? Patterns.Count - 1 : Patterns.Count, htFixedLen && fixedLen);
			}

			return true;
		}
		#endregion


		#region Properties
		internal override ElaPatternAffinity Affinity
		{
			get { return ElaPatternAffinity.Fold|ElaPatternAffinity.Any; }
		}
		#endregion
	}
}
