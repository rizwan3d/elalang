using System;

namespace Ela.Parsing
{
	public enum ElaParserError
	{
		None = 0,

		InvalidSyntax = 1,

        InvalidFunName = 2,

		InvalidIntegerSyntax = 3,

		InvalidRealSyntax = 4,

		InvalidEscapeCode = 5,

		InvalidAttribute = 6,

		InvalidFunctionDeclaration = 7,

		// = 8,

		ComprehensionOpInvalidOperand = 9,

		TabNotAllowed = 10,

		IncorrectIndentation = 11,

		InvalidSyntaxUnexpectedSymbol = 12,

		InvalidIndentationUnexpectedSymbol = 13,



		ExpectedToken = 100,

		ExpectedEof = 101,

		ExpectedIdentifierToken = 102,

		ExpectedIntToken = 103,

		ExpectedRealToken = 104,

		ExpectedStringToken = 105,

		ExpectedCharToken = 106,

		ExpectedOperatorToken = 107,

		ExpectedOn = 108,

		ExpectedBooleanToken = 109,

		// = 110,

		ExpectedCurlyBrace = 111,

		ExpectedArrow = 112,

		ExpectedKeywordIn = 113,

		// = 114,

		// = 115,

		ExpectedKeywordWith = 116,

		ExpectedKeywordOn = 117,

		// = 118,

		ExpectedKeywordMatch = 119,

		ExpectedSquareBrace = 120,

		ExpectedComprehensionSlash = 121,

		ExpectedIsOperator = 122,

		ExpectedKeywordLet = 123,

		ExpectedKeywordPrivate = 124,

		ExpectedKeywordOpen = 125,

		ExpectedKeywordCout = 126,

		ExpectedKeywordTypeof = 127,

		// = 128,

		// = 129,

		// = 130,

		// = 131,

		// = 132,

		// = 133,
		
		// = 134,

		ExpectedKeywordIf = 135,

		ExpectedKeywordElse = 136,

		ExpectedKeywordThrow = 137,

		ExpectedKeywordYield = 138,

		// = 139,

		// = 140,

		// = 141,

		ExpectedKeywordRaise = 142,

		ExpectedKeywordCatch = 143,

		// = 144,

		// = 145,

		ExpectedKeywordIgnore = 146,

		ExpectedLambda = 147,

		ExpectedDot = 148,

		ExpectedKeywordThen = 149,

		ExpectedKeywordTry = 150,

		ExpectedKeywordFail = 151,

		ExpectedKeywordWhere = 152,

		ExpectedKeywordEt = 153,

		// = 154,

		// = 155,

		ExpectedKeywordAsAmp = 156,

		ExpectedComprehensionOp = 157,



		InvalidProduction = 200,

		InvalidLiteral = 201,

		InvalidPrimitive = 202,

		InvalidExpression = 203,

		InvalidPattern = 204,

		InvalidLiteralPattern = 205,

		InvalidOperator = 206,

		InvalidBindingPattern = 207,

		InvalidGeneratorPattern = 208,

		InvalidIsOperatorPattern = 209,

		InvalidRoot = 210,

		InvalidOverload = 211,

		InvalidAssignment = 212,

		InvalidIf = 213,

		InvalidFunction = 214,

		InvalidRaise = 215,

		InvalidLazy = 216,

		InvalidFail = 217,

		InvalidTry = 218,

		// = 219,

		// = 220,

		// = 221,

		// = 222,

		InvalidSimpleExpression = 223,

		InvalidVariant = 224,

		InvalidVariableReference = 225,

		InvalidTuple = 226,

		InvalidGrouping = 227,

		InvalidMemberAccess = 228,

		InvalidArgumentReference = 229,

		InvalidBaseReference = 230,

		InvalidMatch = 231,

		InvalidMatchEntry = 232,

		InvalidParenPattern = 233,

		InvalidVariantPattern = 234,

		InvalidFuncPattern = 235,

		// = 236,

		InvalidUnitPattern = 237,

		InvalidDefaultPattern = 238,

		InvalidRecordPattern = 239,

		InvalidTuplePattern = 240,

		InvalidConsPattern = 241,

		InvalidListPattern = 242,

		InvalidRecord = 243,

		InvalidRange = 244,

		InvalidList = 245,

		InvalidParamList = 246,

		InvalidOperation = 247,

		InvalidBinding = 248,

		InvalidWhereBinding = 249,

		InvalidLambda = 250,

		InvalidInclude = 251,

		InvalidPipe = 252,

		InvalidOr = 253,

		InvalidAnd = 254,

		// = 255,

		InvalidShift = 256,

		// = 257,

		// = 258,

		// = 259,

		InvalidCast = 260,

		InvalidInfix = 261,

		// = 262,

		// = 263,

		// = 264,

		// = 265,

		InvalidApplication = 266,

		InvalidSequence = 267,

		InvalidComprehension = 268,

		InvalidGuard = 269
	}
}
