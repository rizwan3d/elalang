using System;

namespace Ela.Compilation
{
	public enum ElaCompilerError
	{
		None = 0,


		VariableAlreadyDeclared = 300, 
		
		YieldNotAllowed = 301,

		//302

		StrictModeMutableGlobal = 303,

		ReturnInGenerator = 304,

		ReturnInGlobal = 305,

		BreakNotAllowed = 306,

		ContinueNotAllowed = 307,

		ForeachNoInitialization = 308,

		BreakExecutionNotAllowed = 309,

		BaseNotAllowed = 310,

		BaseVariableNotFound = 311,

		AssignImmutableVariable = 312,

		UndefinedVariable = 313,

		PlaceholderNotValid = 314,

		UnableAssignExpression = 315,

		OperatorBinaryTwoParams = 316,

		OperatorUnaryOneParam = 317,

		InvalidConstructorBody = 318,

		InvalidFunctionDeclaration = 319,

		InvalidParameterDeclaration = 320,

		MatchNotSupportedInFor = 321,

		MatchEntryTypingFailed = 322,

		MatchOrPatternVariables = 323,

		MatchHeadTailPatternNil = 324,

		OperatorAlreadyDeclared = 325,

		OperatorOnlyInGlobal = 326,

		VariableDeclarationInitMissing = 327
	}
}
