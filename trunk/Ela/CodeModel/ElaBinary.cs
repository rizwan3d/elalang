using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBinary : ElaExpression
	{
		#region Construction
		internal ElaBinary(Token tok)
			: base(tok, ElaNodeType.Binary)
		{

		}


		public ElaBinary()
			: base(ElaNodeType.Binary)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			var str = Left.PutInBracesComplex() + " " +
				(Operator == ElaOperator.Custom ? CustomOperator : Operator.AsString()) +
				" " + Right.PutInBracesComplex();
			return Left.IsHiddenVar() || Right.IsHiddenVar() ? str.PutInBraces() : str;
		}
		#endregion


		#region Properties
		public ElaExpression Left { get; set; }

		public ElaExpression Right { get; set; }

		public ElaOperator Operator { get; set; }

		public string CustomOperator { get; set; }
		#endregion
	}
}