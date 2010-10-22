using System;

namespace Ela.Parsing
{
	public enum ElaParserError
	{
		None = 0,

		InvalidSyntax = 1,

		UnknownConversionType = 2,

		InvalidIntegerSyntax = 3,

		InvalidRealSyntax = 4,

		InvalidEscapeCode = 5,

		ShortMatchWithoutParams = 6,

		InvalidFunctionDeclaration = 7,

		InvalidRecordFieldDeclaration = 8,



		ExpectedToken = 100,

		ExpectedEof = 101,

		ExpectedIdentifierToken = 102,

		ExpectedIntToken = 103,

		ExpectedRealToken = 104,

		ExpectedStringToken = 105,

		ExpectedCharToken = 106,

		ExpectedOperatorToken = 107,

		ExpectedVariantToken = 108,

		ExpectedBooleanToken = 109,

		ExpectedArgSigma = 110,
		
		ExpectedSemicolon = 111,

		ExpectedCurlyBrace = 112,

		ExpectedMatchOperator = 113,

		ExpectedKeywordIn = 114,

		ExpectedKeywordTo = 115,

		ExpectedKeywordDownto = 116,

		ExpectedKeywordOn = 117,

		ExpectedKeywordBase = 118,

		ExpectedKeywordMatch = 119,

		ExpectedKeywordWhen = 120,
		
		ExpectedKeywordAs = 121,
		
		ExpectedKeywordIs = 122,

		ExpectedKeywordVar = 123,

		ExpectedKeywordLet = 124,

		ExpectedKeywordPrivate = 125,

		ExpectedKeywordOpen = 126,

		ExpectedKeywordAt = 127,

		ExpectedKeywordWith = 128,

		ExpectedKeywordCout = 129,

		ExpectedKeywordTypeof = 130,

		ExpectedKeywordWhile = 131,

		ExpectedKeywordUntil = 132,

		ExpectedKeywordDo = 133,

		ExpectedKeywordFor = 134,

		ExpectedKeywordIf = 135,

		ExpectedKeywordElse = 136,

		ExpectedKeywordThrow = 137,

		ExpectedKeywordYield = 138,

		ExpectedKeywordReturn = 139,

		ExpectedKeywordBreak = 140,

		ExpectedKeywordContinue = 141,

		ExpectedKeywordTry = 142,

		ExpectedKeywordCatch = 143,

		ExpectedKeywordAsync = 144,

		ExpectedKeywordLazy = 145,

		ExpectedKeywordIgnore = 146,


		
		InvalidProduction = 200, 
		
		InvalidLiteral = 201,

		InvalidPrimitiveLiteral = 202,

		InvalidExpression = 203,

		InvalidPattern = 204,

		InvalidLiteralPattern = 205,

		InvalidBooleanPattern = 206,

		InvalidBindingPattern = 207,

		InvalidForeachPattern = 208,

		InvalidIsOperatorPattern = 209,

		InvalidRootExpression = 210,

		InvalidIterationExpression = 211,

		InvalidAssignment = 212,

		InvalidVariableDeclaration = 213,

		InvalidFunctionExpression = 214,

		InvalidWhenExpression = 215,

		InvalidIncludeExpression = 216,

		InvalidWhileExpression = 217,

		InvalidDoWhileExpression = 218,

		InvalidForExpression = 219,

		InvalidTryCatchExpression = 220,

		InvalidUnaryExpression = 221,

		InvalidUnaryOperator = 222,
	}
}
