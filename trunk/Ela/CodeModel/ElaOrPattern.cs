using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaOrPattern : ElaPattern
	{
		#region Construction
		internal ElaOrPattern(Token tok) : base(tok, ElaNodeType.OrPattern)
		{

		}


		public ElaOrPattern() : base(ElaNodeType.OrPattern)
		{

		}
		#endregion


		#region Properties
		public ElaPattern Left { get; set; }

		public ElaPattern Right { get; set; }
		
		internal override ElaPatternAffinity Affinity
		{
			get { return Left.Affinity; }
		}
		#endregion
	}
}