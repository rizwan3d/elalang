using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaAndPattern : ElaPattern
	{
		#region Construction
		internal ElaAndPattern(Token tok) : base(tok, ElaNodeType.AndPattern)
		{

		}


		public ElaAndPattern() : base(ElaNodeType.AndPattern)
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