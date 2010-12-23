using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaRaise : ElaExpression
	{
		#region Construction
		internal ElaRaise(Token tok) : base(tok, ElaNodeType.Raise)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaRaise() : this(null)
		{
			
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return "raise " + ErrorCode + 
				(Expression != null ? "(" + Expression.ToString() + ")" : String.Empty);
		}
		#endregion


		#region Properties
		public string ErrorCode { get; set; }

		public ElaExpression Expression { get; set; }	
		#endregion
	}
}