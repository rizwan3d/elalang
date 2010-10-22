using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaConstructorPattern : ElaSeqPattern
	{
		#region Construction
		internal ElaConstructorPattern(Token tok, ElaSeqPattern seq) : base(tok, seq != null ? seq.Patterns : null, ElaNodeType.ConstructorPattern)
		{
			
		}


		public ElaConstructorPattern() : base(null, ElaNodeType.ConstructorPattern)
		{
			
		}
		#endregion


		#region Properties
		public string Name { get; internal set; }
		#endregion
	}
}
