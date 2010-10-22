using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariableDeclaration : ElaExpression
	{
		#region Construction
		internal ElaVariableDeclaration(Token tok) : base(tok, ElaNodeType.VariableDeclaration)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaVariableDeclaration() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public string VariableName { get; set; }

		public ElaVariableFlags VariableFlags { get; set; }

		public ElaExpression InitExpression { get; set; }

		public ElaPattern Pattern { get; set; }
		#endregion
	}
}