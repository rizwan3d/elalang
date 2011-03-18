using System;
using System.Text;
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
		internal override void ToString(StringBuilder sb)
		{
			sb.Append(Operator);
		}
		#endregion


		#region Properties
		public string Operator { get; set; }
		#endregion
	}
}