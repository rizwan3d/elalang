using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVoidPattern : ElaPattern
	{
		#region Construction
		internal ElaVoidPattern(Token tok) : base(tok, ElaNodeType.VoidPattern)
		{

		}


		public ElaVoidPattern() : base(ElaNodeType.VoidPattern)
		{

		}
		#endregion
	}
}
