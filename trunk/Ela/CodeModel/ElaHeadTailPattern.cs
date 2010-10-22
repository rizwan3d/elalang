using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaHeadTailPattern : ElaSeqPattern
	{
		#region Construction
		internal ElaHeadTailPattern(Token tok) : base(tok, ElaNodeType.HeadTailPattern)
		{
			
		}


		public ElaHeadTailPattern() : this(null)
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
