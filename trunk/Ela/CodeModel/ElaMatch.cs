using System;
using Ela.Parsing;
using System.Collections.Generic;

namespace Ela.CodeModel
{
	public sealed class ElaMatch : ElaExpression
	{
		#region Construction
		internal ElaMatch(Token tok) : base(tok, ElaNodeType.Match)
		{
			Entries = new List<ElaMatchEntry>();
		}


		public ElaMatch() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		
		public List<ElaMatchEntry> Entries { get; private set; }
		#endregion
	}
}