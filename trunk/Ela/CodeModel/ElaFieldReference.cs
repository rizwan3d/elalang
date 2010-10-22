using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFieldReference : ElaExpression
	{
		#region Construction
		internal ElaFieldReference(Token tok) : base(tok, ElaNodeType.FieldReference)
		{
			Flags = ElaExpressionFlags.Assignable;
		}


		public ElaFieldReference() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public string FieldName { get; set; }

		public ElaExpression TargetObject { get; set; }

		public override int Placeholders { get { return TargetObject.Placeholders; } }
		#endregion
	}
}