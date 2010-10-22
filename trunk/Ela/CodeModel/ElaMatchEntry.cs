using System;
using Ela.Parsing;
using System.Collections.Generic;

namespace Ela.CodeModel
{
	public sealed class ElaMatchEntry : ElaExpression
	{
		#region Construction
		internal ElaMatchEntry(Token tok) : base(tok, ElaNodeType.MatchEntry)
		{
			
		}


		public ElaMatchEntry() : base(ElaNodeType.MatchEntry)
		{
			
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }

		public ElaPattern Pattern { get; set; }
		#endregion
	}
}