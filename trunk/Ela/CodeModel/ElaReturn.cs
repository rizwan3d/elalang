using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaReturn : ElaExpression
	{
		#region Construction
		internal ElaReturn(Token tok) : base(tok, ElaNodeType.Return)
		{
			Flags = ElaExpressionFlags.BreaksExecution;
		}


		public ElaReturn() : this(null)
		{
			
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return "return " + Expression.ToString();
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		#endregion
	}
}