using System;

namespace Ela.Compilation
{
	public enum ElaCompilerError
	{
		None = 0,


		VariableAlreadyDeclared = 300,

		OperatorInvalidDeclaration = 301,

		BaseNotAllowed = 302,

		BaseVariableNotFound = 303,

		AssignImmutableVariable = 304,

		VariableShadowsExternal = 305,

		PlaceholderNotValid = 306,

		UnableAssignExpression = 307,

		PatternNotAllArgs = 308,

		NameReserved = 309,

		CastNotSupported = 310,

		InfiniteRangeOnlyList = 311,

		OperatorAlreadyDeclared = 312,

		OperatorOnlyInGlobal = 313,

		VariableDeclarationInitMissing = 314,

		ElseGuardNotValid = 315,

		MatchEntryEmptyNoGuard = 316,
	}
}
