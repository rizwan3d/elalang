using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaUnary : ElaExpression
	{
		#region Construction
		internal ElaUnary(Token tok) : base(tok, ElaNodeType.Unary)
		{

		}


		public ElaUnary() : base(ElaNodeType.Unary)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
            return Format.OperatorAsString(Operator) + Format.PutInBracesComplex(Expression);
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }

		public ElaUnaryOperator Operator { get; set; }
		#endregion
	}
}