using System;
using System.Text;
using Ela.Parsing;
using System.Collections.Generic;

namespace Ela.CodeModel
{
	public sealed class ElaTemplateReference : ElaExpression
	{
		#region Construction
		internal ElaTemplateReference(Token tok) : base(tok, ElaNodeType.TemplateReference)
		{
			Parameters = new List<ElaExpression>();
		}


		public ElaTemplateReference() : this(null)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append(FunctionName);
			sb.Append('!');
			sb.Append('[');

			foreach (var p in Parameters)
			{
				sb.Append(p);
				sb.Append(' ');
			}

			sb.Append(']');
		}
		#endregion


		#region Properties
		public string FunctionName { get; set; }

		public List<ElaExpression> Parameters { get; private set; }
		#endregion
	}
}