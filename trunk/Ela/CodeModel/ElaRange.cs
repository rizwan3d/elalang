using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaRange : ElaExpression
	{
		#region Construction
		internal ElaRange(Token tok) : base(tok, ElaNodeType.Range)
		{

		}


		public ElaRange() : base(ElaNodeType.Range)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			var format = "[{0}]";

			return String.Format(format, First.ToString() +
				(Second != null ? "," + Second : String.Empty) +
				".." +
				(Last != null ? Last.ToString() : String.Empty));
		}
		#endregion


		#region Properties
		public ElaExpression First { get; set; }

		public ElaExpression Second { get; set; }

		public ElaExpression Last { get; set; }

		public ElaExpression Initial { get; set; }
		#endregion
	}
}