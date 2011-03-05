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


		#region Methods
		public override string ToString()
		{
			return (Mutable ? "mutable " : String.Empty) +
                FieldName + "=" + (Format.IsSimpleExpression(FieldValue) ? FieldValue.ToString() : Format.PutInBraces(FieldValue));
		}
		#endregion


		#region Properties
		public string FieldName { get; set; }

		public bool Mutable { get; set; }

		public ElaExpression FieldValue { get; set; }
		#endregion
	}
}