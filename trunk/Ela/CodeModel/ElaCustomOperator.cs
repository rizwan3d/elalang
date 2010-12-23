using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaCustomOperator : ElaExpression
	{
		#region Construction
		internal ElaCustomOperator(Token tok) : base(tok, ElaNodeType.CustomOperator)
		{

		}


		public ElaCustomOperator() : base(ElaNodeType.CustomOperator)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return "(" + Operator + ")";
		}
		#endregion


		#region Properties
		public string Operator { get; set; }
		#endregion
	}
}