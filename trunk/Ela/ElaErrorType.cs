using System;

namespace Ela
{
	public enum ElaErrorType
	{
		None = 0,

		Code_User = -1,


		Parser_SyntaxError = 100,

		Parser_InvalidEscapeCode = 102,
		
		Parser_InvalidIntegerLiteral = 103,
		
		Parser_InvalidRealLiteral = 104,
		
		Parser_UnknownType = 105,		
		
		Parser_ExpectedToken = 106,
		
		Parser_ExpectedElse = 107,
		
		Parser_ExpectedCatch = 108,
		
		Parser_InvalidExpression = 109,
		
		Parser_InvalidIdentifierItem = 110,
		
		Parser_InvalidItemAccess = 111,
		
		Parser_InvalidMatchEntry = 112,
		
		Parser_InvalidPattern = 113,
		
		Parser_InvalidStatement = 114,
		
		Parser_InvalidAssignment = 115,
		
		Parser_InvalidVariableDeclaration = 116,
		
		Parser_InvalidTryCatch = 118,
		
		Parser_InvalidUnaryOp = 119,
		
		Parser_InvalidLiteral = 120,
		
		Parser_InvalidUnary = 121,
		
		Parser_InvalidConversion = 122,
		
		Parser_TableLiteralInvalidField = 124,

		Parser_InvalidFunctionDeclaration = 125, 

		Parser_ShortMatchNoParams = 128,
				
		Parser_ExpectedOperator = 129,

		Parser_ExpectedSemicolon = 130,

		Parser_ExpectedOn = 131,

		Parser_ExpectedMatchEntryArrow = 132,


		Compile_BreakNotAllowed = 201,

		Compile_ContinueNotAllowed = 202,

		Compile_ExpressionNotAllowed = 205,

		Compile_VariableAlreadyDeclared = 206,

		Compile_InvalidExpressionForInitializer = 207,

		Compile_UndefinedVariable = 208,

		Compile_UnableAssignExpression = 210,

		Compile_PlaceholderNotValid = 212,

		Compile_AssignImmutableVariable = 213,

		
		Compile_MatchEntryTypingFail = 214,


		Compile_NoInitForVar = 216,

		Compile_InvalidCoroutine = 219,

		Compile_YieldNotAllowed = 220,

		Compile_ReturnInCoroutine = 221,

		Compile_AssignExternalVariable = 226,

		Compile_MatchInvalidHeadTailNil = 228,

		Compile_FunctionLiteralInvalidParams = 229,
		
		Compile_FunctionLiteralInvalidHead = 230,

		Compile_ReturnInGlobal = 231,

		Compile_StrictMutableGlobal = 232,

		Compile_OperatorOnlyGlobal = 233,

		Compile_BinaryOperatorTwoParams = 234,

		Compile_UnaryOperatorOneParam = 235,

		Compile_OperatorAlreadyDeclared = 236,

		Compile_NoLetInConstructor = 237,

		Compile_InvalidBase = 238,

		Compile_DeclarationInEmbedded = 239,

		Compile_NoPatternMatchForTo = 240,

		Compile_NoInitForeach = 241,

		Compile_InvalidCtorBody = 242,

		Compile_BreakExecutionNotAllowed = 243,

		Compile_OrPatternNotAllVarsDeclared = 244,

		
		Runtime_NotFunction = 403,

		Runtime_TooManyParameters = 404,

		Runtime_UnableToCallAsFunction = 405,
				
		Runtime_MatchFailed = 407,

		Runtime_InvalidType = 408,

		Runtime_InvalidOperation = 409,

		Runtime_DivideByZero = 410,

		Runtime_TooFewParameters = 411,

		Runtime_ConversionFailed = 412,

		Runtime_UnableToConvert = 413,

		Runtime_Unknown = 414,

		Runtime_UnknownField = 417,

		Runtime_UnknownNameReference = 419,

		Runtime_IndexOutOfRange = 420,

		Runtime_CallError = 421,

		Runtime_ExternalCallError = 422,

		Runtime_InvalidParameterType = 423,

		Runtime_UndefinedVariable = 424,

		Runtime_PrivateVariable = 425,

		Runtime_UndefinedArgument = 426,

		Runtime_UnknownParameterType = 427,


		Runtime_OperationNotSupported = 428,

		Runtime_ImmutableField = 429,

		


		Linker_UnresolvedModule = 600,

		Linker_InvalidModule = 601,

		Linker_InvalidForeignModule = 602,

		Linker_InvalidModuleName = 603,

		Linker_InvalidForeignModuleType = 604,
		
		Linker_ForeignModuleInitError = 606,

		Linker_AssemblyLoadError = 607,

		Linker_DuplicateModuleInAssembly = 608,

		Linker_ModuleNotFoundInAssembly = 609,

		Linker_ObjectFileError = 610,

		Linker_WarningOutdatedObjectFile = 611,


		Warning_NextMatchEntryIgnored = 1001,

		Warning_ExpressionValueNotUsed = 1002,

		Warning_OperatorAsMutable = 1004,

		Warning_MatchEntryAlwaysFail = 1005,

		Warning_VoidWillAlwaysFail = 1006,
		

		Hint_UseVarInsteadLet = 2003,

		Hint_UseIgnoreToPop = 2004,

		Hint_MatchEntryTypingFailed = 2005,
		
		Hint_DefaultPatternToEnd = 2006,

		Hint_UseYieldToReturn = 2007
	}
}
