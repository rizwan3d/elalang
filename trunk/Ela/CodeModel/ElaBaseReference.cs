using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBaseReference : ElaExpression
	{
		#region Construction
		internal ElaBaseReference(Token tok) : base(tok, ElaNodeType.BaseReference)
		{

		}


		public ElaBaseReference() : base(ElaNodeType.BaseReference)
		{

		}
		#endregion
	}
}