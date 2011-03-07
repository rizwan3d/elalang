using System;

namespace Ela.Compilation
{
	public enum ElaCompilerError
	{
		None = 0,


		VariableAlreadyDeclared = 300,

		// = 301,

		OperatorInvalidDeclaration = 302,

		// = 303,

		// = 304,

		// = 305,

		// = 306,

		// = 307,

		// = 308,

		// = 309,

		BaseNotAllowed = 310,

		BaseVariableNotFound = 311,

		AssignImmutableVariable = 312,

		UndefinedVariable = 313,

		PlaceholderNotValid = 314,

		UnableAssignExpression = 315,

		// = 316,

		PatternNotAllArgs = 317,

		NameReserved = 318,

		CastNotSupported = 319,

		InfiniteRangeOnlyList = 320,

		MatchNotSupportedInFor = 321,

		MatchEntryTypingFailed = 322,

		// = 323,

		// = 324,

		OperatorAlreadyDeclared = 325,

		OperatorOnlyInGlobal = 326,

		VariableDeclarationInitMissing = 327,

		ElseGuardNotValid = 328,

		MatchEntryEmptyNoGuard = 329,

	}
}
