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


		#region Properties
		public ElaExpression Expression { get; set; }

		public ElaUnaryOperator Operator { get; set; }

		public string CustomOperator { get; set; }

		public override int Placeholders { get { return Expression != null ? Expression.Placeholders : 0; } }		
		#endregion
	}
}