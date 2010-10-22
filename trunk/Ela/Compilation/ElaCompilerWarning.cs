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

		ConstructorAsMutable = 405, 
		
		ValueNotUsed = 406,
		


		FunctionTooManyParams = 407,

		FunctionInvalidType = 408,

		FunctionZeroParams = 409,

		FunctionImplicitPartial = 410,
	}
}
