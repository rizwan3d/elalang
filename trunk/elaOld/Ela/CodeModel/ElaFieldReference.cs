using System;
using System.Text;
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
		internal override void ToString(StringBuilder sb, Fmt fmt)		
		{
            Format.PutInBraces(TargetObject, sb, fmt);
			sb.Append('.');
			sb.Append(FieldName);
		}
		#endregion


		#region Properties
		public string FieldName { get; set; }

		public ElaExpression TargetObject { get; set; }
		#endregion
	}
}