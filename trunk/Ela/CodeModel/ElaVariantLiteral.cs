using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaVariantLiteral : ElaExpression
	{
		#region Construction
		internal ElaVariantLiteral(Token tok)
			: base(tok, ElaNodeType.VariantLiteral)
		{

		}


		public ElaVariantLiteral()
			: this(null)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return "`" + Tag + " " + Expression;
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }

		public string Tag { get; set; }
		#endregion
	}
}