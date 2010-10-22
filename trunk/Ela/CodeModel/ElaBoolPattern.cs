using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBoolPattern : ElaPattern
	{
		#region Construction
		internal ElaBoolPattern(Token tok) : base(tok, ElaNodeType.BoolPattern)
		{

		}


		public ElaBoolPattern() : base(ElaNodeType.BoolPattern)
		{

		}
		#endregion


		#region Properties
		public ElaBinaryOperator Operator { get; set; }

		public ElaPattern Right { get; set; }
		
		public string Left { get; set; }

		internal override ElaPatternAffinity Affinity
		{
			get { return Right.Affinity; }
		}
		#endregion
	}
}
