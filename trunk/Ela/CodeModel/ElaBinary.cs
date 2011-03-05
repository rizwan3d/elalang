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
            var str = Format.PutInBracesComplex(Left) + " " +
                (Operator == ElaOperator.Custom ? CustomOperator : Format.OperatorAsString(Operator)) +
                " " + Format.PutInBracesComplex(Right);
            return Format.IsHiddenVar(Left) || Format.IsHiddenVar(Right) ? Format.PutInBraces(str) : str;
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