using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Parsing
{
	public sealed class ParserResult : TranslationResult
	{
		#region Construction
		internal ParserResult(ElaCodeUnit unit, bool success, IEnumerable<ElaMessage> messages) :
			base(success, messages)
		{
			CodeUnit = unit;
		}
		#endregion


		#region Properties
		public ElaCodeUnit CodeUnit { get; private set; }
		#endregion
	}
}
