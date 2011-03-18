using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBaseReference : ElaExpression
	{
		#region Construction
		internal ElaBaseReference(Token tok) : base(tok, ElaNodeType.BaseReference)
		{

		}


		public ElaBaseReference() : base(ElaNodeType.BaseReference)
		{

		}
		#endregion
		

		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			sb.Append("base");
		}
		#endregion
	}
}