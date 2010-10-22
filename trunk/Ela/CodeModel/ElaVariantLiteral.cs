using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariantLiteral : ElaExpression
	{
		#region Construction
		internal ElaVariantLiteral(Token tok) : base(tok, ElaNodeType.VariantLiteral)
		{
			
		}


		public ElaVariantLiteral() : base(ElaNodeType.VariantLiteral)
		{
			
		}
		#endregion


		#region Properties
		public int Length { get; set; }

		public string Name { get; set; }
		#endregion
	}
}