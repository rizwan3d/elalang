using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaListPattern : ElaSeqPattern
	{
		#region Construction
		internal ElaListPattern(Token tok) : base(tok, ElaNodeType.ListPattern)
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
