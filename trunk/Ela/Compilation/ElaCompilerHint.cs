using System;

namespace Ela.Compilation
{
	public enum ElaCompilerHint
	{
		None = 0,

		
		UseYieldInGenerator = 500,

		UseVarInsteadOfLet = 501,

		MatchEntryTypingFailed = 502,

		MatchNextEntryIgnored = 503,

		UseIgnoreToPop = 504
	}
}
