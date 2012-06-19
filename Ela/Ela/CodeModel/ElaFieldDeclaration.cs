using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFieldDeclaration : ElaExpression
	{
		internal ElaFieldDeclaration(Token tok) : base(tok, ElaNodeType.FieldDeclaration)
		{

		}
        
		public ElaFieldDeclaration() : base(ElaNodeType.FieldDeclaration)
		{

		}
        
        internal override bool Safe()
        {
            return FieldValue != null && FieldValue.Safe();
        }
		
		internal override void ToString(StringBuilder sb, Fmt fmt)		
		{
			sb.Append(FieldName);
			sb.Append('=');
			FieldValue.ToString(sb, fmt);
		}
		
        public string FieldName { get; set; }

		public ElaExpression FieldValue { get; set; }
	}
}