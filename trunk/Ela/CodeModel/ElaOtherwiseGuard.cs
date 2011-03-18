using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaOtherwiseGuard : ElaExpression
	{
		#region Construction
		internal ElaOtherwiseGuard(Token t) : base(t, ElaNodeType.OtherwiseGuard)
		{

		}


		public ElaOtherwiseGuard() : base(ElaNodeType.OtherwiseGuard)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			sb.Append("else");
		}
		#endregion
	}
}
