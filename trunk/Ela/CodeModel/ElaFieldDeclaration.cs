using System;
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


		#region Properties
		public string FieldName { get; set; }

		public ElaExpression FieldValue { get; set; }
		#endregion
	}
}