using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaArrayPattern : ElaSeqPattern
	{
		#region Construction
		internal ElaArrayPattern(Token tok) : base(tok, ElaNodeType.ArrayLiteral)
		{
			
		}


		public ElaArrayPattern() : this(null)
		{

		}
		#endregion


		#region Properties
		internal override ElaPatternAffinity Affinity
		{
			get { return ElaPatternAffinity.Array; }
		}
		#endregion
	}
}