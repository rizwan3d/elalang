using System;

namespace Ela.Compilation
{
	public enum ElaCompilerWarning
	{
		None = 0,

		
		UnitAlwaysFail = 400,

		EmbeddedBinding = 401,

		MatchEntryAlwaysFail = 402,

		MatchEntryNotReachable = 403, 
		
		ValueNotUsed = 404,
		
		FunctionInvalidType = 405,

		FunctionImplicitPartial = 406,
	}
}
