using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBinary : ElaExpression
	{
		#region Construction
		internal ElaBinary(Token tok) : base(tok, ElaNodeType.Binary)
		{

		}


		public ElaBinary() : base(ElaNodeType.Binary)
		{

		}
		#endregion


		#region Properties
		public ElaExpression Left { get; set; }

		public ElaExpression Right { get; set; }

		public ElaBinaryOperator Operator { get; set; }

		public string CustomOperator { get; set; }

		public override int Placeholders { get { return Left.Placeholders + (Right != null ? Right.Placeholders : 0); } }
		#endregion
	}
}