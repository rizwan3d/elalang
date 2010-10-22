using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaValueofPattern : ElaPattern
	{
		#region Construction
		internal ElaValueofPattern(Token tok) : base(tok, ElaNodeType.ValueofPattern)
		{

		}


		public ElaValueofPattern() : base(ElaNodeType.ValueofPattern)
		{

		}
		#endregion


		#region Properties
		public string VariableName { get; set; }
		#endregion
	}
}