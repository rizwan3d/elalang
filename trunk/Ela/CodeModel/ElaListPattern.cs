using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaListPattern : ElaSeqPattern
	{
		#region Construction
		internal ElaListPattern(Token tok, ElaSeqPattern seq) : base(tok, seq != null ? seq.Patterns : null, ElaNodeType.ListPattern)
		{
			
		}


		public ElaListPattern() : base(null, ElaNodeType.ListPattern)
		{
			
		}
		#endregion


		#region Properties
		internal override ElaPatternAffinity Affinity
		{
			get { return ElaPatternAffinity.List; }
		}
		#endregion
	}
}
