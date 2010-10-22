using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFor : ElaExpression
	{
		#region Construction
		internal ElaFor(Token tok) : base(tok, ElaNodeType.For)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaFor() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public ElaPattern Pattern { get; set; }

		public ElaExpression Target { get; set; }

		public ElaExpression Body { get; set; }

		public ElaExpression InitExpression { get; set; }
		
		public ElaVariableFlags VariableFlags { get; set; }

		public ElaForType ForType { get; set; }
		#endregion
	}
}