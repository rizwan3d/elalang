using System;

namespace Ela.Compilation
{
	public enum ElaCompilerError
	{
		None = 0,


		VariableAlreadyDeclared = 300,

		BaseNotAllowed = 301,

		BaseVariableNotFound = 302,

		AssignImmutableVariable = 303,

		PlaceholderNotValid = 304,

		UnableAssignExpression = 305,

		PatternNotAllArgs = 306,

		NameReserved = 307,

		CastNotSupported = 308,

		InfiniteRangeOnlyList = 309,

		VariableDeclarationInitMissing = 310,

		ElseGuardNotValid = 311,

		MatchEntryEmptyNoGuard = 312,

		InlineOnlyFunctions = 313,

		InlineOnlyGlobal = 314,
	}
}
