using System;

namespace Ela.Compilation
{
	public enum ElaCompilerWarning
	{
		None = 0,

		
		UnitAlwaysFail = 400,

		EmbeddedBinding = 401,

		MatchNextEntryIgnored = 402,

		MatchEntryAlwaysFail = 403,

		MatchEntryTypingFailed = 404,

		MatchLessSpecific = 405, 
		
		ValueNotUsed = 406,
		
		FunctionInvalidType = 407,

		FunctionImplicitPartial = 408,
	}
}
