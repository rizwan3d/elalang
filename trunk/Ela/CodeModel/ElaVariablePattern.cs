using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariablePattern : ElaPattern
	{
		#region Construction
		internal ElaVariablePattern(Token tok) : base(tok, ElaNodeType.VariablePattern)
		{

		}


		public ElaVariablePattern() : base(ElaNodeType.VariablePattern)
		{

		}
		#endregion


		#region Properties
		public string Name { get; set; }
		#endregion
	}
}
