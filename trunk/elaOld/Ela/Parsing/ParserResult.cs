using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Parsing
{
	public sealed class ParserResult : TranslationResult
	{
		#region Construction
		internal ParserResult(ElaExpression expr, bool success, IEnumerable<ElaMessage> messages) :
			base(success, messages)
		{
			Expression = expr;
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; private set; }
		#endregion
	}
}
