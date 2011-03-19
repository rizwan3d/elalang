using System;

namespace Ela.Compilation
{
	public enum ElaCompilerHint
	{
		None = 0,

		
		UseReferenceCell = 500,

		MatchEntryTypingFailed = 501,

		MatchNextEntryIgnored = 502,

		UseIgnoreToPop = 503
	}
}
