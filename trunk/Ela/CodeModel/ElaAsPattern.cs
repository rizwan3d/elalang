using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaAsPattern : ElaPattern
	{
		#region Construction
		internal ElaAsPattern(Token tok) : base(tok, ElaNodeType.AsPattern)
		{

		}


		public ElaAsPattern() : base(ElaNodeType.AsPattern)
		{

		}
		#endregion


		#region Properties
		public ElaPattern Pattern { get; set; }

		public string Name { get; set; }
		#endregion
	}
}