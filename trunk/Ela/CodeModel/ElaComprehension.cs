using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaComprehension : ElaExpression
	{
		#region Construction
		internal ElaComprehension(Token tok) : base(tok, ElaNodeType.Comprehension)
		{

		}


		public ElaComprehension() : base(null, ElaNodeType.Comprehension)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			if (Initial.Type != ElaNodeType.ListLiteral)
			{
				Format.PutInBraces(Initial, sb, fmt);
				sb.Append(" @@ ");
			}

			sb.Append('[');

			if (Lazy)
				sb.Append("& ");

			Generator.ToString(sb, fmt);
			sb.Append(']');
		}
		#endregion


		#region Properties
		public ElaGenerator Generator { get; set; }

		public ElaExpression Initial { get; set; }

		public bool Lazy { get; set; }
		#endregion
	}
}
