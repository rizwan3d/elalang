using System;

namespace Ela.Compilation
{
	public enum ElaCompilerError
	{
		None = 0,


		VariableAlreadyDeclared = 300,

		RedefinitionNotAllowed = 301,

		UndefinedName = 302,

		AssignImmutableVariable = 303,

		PlaceholderNotValid = 304,

		UnableAssignExpression = 305,

		PatternNotAllArgs = 306,

		// = 307,

		// = 308,

		InfiniteRangeOnlyList = 309,

		VariableDeclarationInitMissing = 310,

		ElseGuardNotValid = 311,

		MatchEntryEmptyNoGuard = 312,

        PrivateOnlyGlobal = 313,

		InvalidVariant = 314,

        InvalidBuiltinBinding = 315,

		ReferNoInit = 316,

        OverloadOnlyGlobal = 317,

        OverloadNotWithAnd = 318,

        OverloadNoPatterns = 319,
	}
}
