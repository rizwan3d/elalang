using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFieldDeclaration : ElaExpression
	{
		#region Construction
		internal ElaFieldDeclaration(Token tok) : base(tok, ElaNodeType.FieldDeclaration)
		{

		}


		public ElaFieldDeclaration() : base(ElaNodeType.FieldDeclaration)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)		
		{
			sb.Append(FieldName);
			sb.Append('=');
			FieldValue.ToString(sb, fmt);
		}
		#endregion


		#region Properties
		public string FieldName { get; set; }

		public ElaExpression FieldValue { get; set; }
		#endregion
	}
}