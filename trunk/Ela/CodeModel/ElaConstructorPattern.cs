using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaConstructorPattern : ElaSeqPattern
	{
		#region Construction
		internal ElaConstructorPattern(Token tok) : base(tok, ElaNodeType.ConstructorPattern)
		{
			
		}


		public ElaConstructorPattern() : base(null, ElaNodeType.ConstructorPattern)
		{
			
		}
		#endregion


		#region Properties
		public string Name { get; set; }

		public bool HasParameterList { get; set; }
		#endregion
	}
}
