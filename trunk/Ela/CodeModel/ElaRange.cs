using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaRange : ElaExpression
	{
		#region Construction
		internal ElaRange(Token tok) : base(tok, ElaNodeType.Range)
		{

		}


		public ElaRange() : base(ElaNodeType.Range)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			sb.Append('[');
			First.ToString(sb);

			if (Second != null)
			{
				sb.Append(',');
				Second.ToString(sb);
			}

			sb.Append("..");

			if (Last != null)
				Last.ToString(sb);

			sb.Append(']');
		}
		#endregion


		#region Properties
		public ElaExpression First { get; set; }

		public ElaExpression Second { get; set; }

		public ElaExpression Last { get; set; }

		public ElaExpression Initial { get; set; }
		#endregion
	}
}