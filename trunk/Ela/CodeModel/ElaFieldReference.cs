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


		#region Methods
		public override string ToString()
		{
			return (TargetObject.IsSimpleExpression() ? TargetObject.ToString() : TargetObject.PutInBraces()) +
				"." + FieldName;
		}
		#endregion


		#region Properties
		public string FieldName { get; set; }

		public ElaExpression TargetObject { get; set; }
		#endregion
	}
}