using System;

namespace Ela.Compilation
{
	public enum ElaCompilerHint
	{
		None = 0,

		
		UseReferenceCell = 500,

		UseIgnoreToPop = 501,

		MatchEntryNotReachable = 502,

        AddElse = 503,
	}
}
