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
		
		OperatorAsMutable = 404,

		// = 405, 
		
		ValueNotUsed = 406,


		// = 407,
		
		FunctionInvalidType = 408,

		// = 409,

		FunctionImplicitPartial = 410,
	}
}
