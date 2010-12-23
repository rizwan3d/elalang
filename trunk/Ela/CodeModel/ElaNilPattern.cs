using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaNilPattern : ElaPattern
	{
		#region Construction
		internal ElaNilPattern(Token tok) : base(tok, ElaNodeType.NilPattern)
		{

		}


		public ElaNilPattern() : base(null, ElaNodeType.NilPattern)
		{

		}
		#endregion
		

		#region Methods
		public override string ToString()
		{
			return "[]";
		}
		#endregion
	}
}
