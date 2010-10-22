using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariableReference : ElaExpression
	{
		#region Construction
		internal ElaVariableReference(Token tok) : base(tok, ElaNodeType.VariableReference)
		{
			Flags = ElaExpressionFlags.Assignable;
		}


		public ElaVariableReference() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public string VariableName { get; set; }
		#endregion
	}
}