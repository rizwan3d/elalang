using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public abstract class ElaPattern : ElaExpression
	{
		#region Construction
		internal ElaPattern(Token tok, ElaNodeType type) : base(tok, type)
		{

		}


		protected ElaPattern(ElaNodeType type) : base(null, type)
		{

		}
		#endregion


		#region Properties
		internal virtual ElaPatternAffinity Affinity { get { return ElaPatternAffinity.Any; } }

		public ElaExpression Guard { get; set; }
		#endregion
	}
}