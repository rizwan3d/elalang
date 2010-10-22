using System;
using Ela.CodeModel;

namespace Ela.Parsing
{
	internal static class ErrorReporter
	{
		#region Methods
		internal static ElaMessage CreateMessage(int error, string message, int line, int col)
		{
			var arr = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var arg = String.Empty;
			var err = ElaParserError.InvalidSyntax;
			var msg = String.Empty;
			
			if (arr.Length == 2)
			{
				var head = arr[0].Trim('\"');
				var tail = arr[1].Trim('\"');
				var exp = tail == "expected";
				msg = exp ? head : tail;
				err = exp ? ExpectToken(error, head) : InvalidToken(error, tail);
			}

			return new ElaMessage(Strings.GetMessage(err, msg), MessageType.Error, (Int32)err, line, col);
		}


		private static ElaParserError InvalidToken(int error, string prod)
		{
			switch (prod)
			{
				case "Literal": return ElaParserError.InvalidLiteral;
				case "Primitive": return ElaParserError.InvalidPrimitiveLiteral;
				case "Expr": return ElaParserError.InvalidExpression;
				case "SinglePattern": return ElaParserError.InvalidPattern;
				case "LiteralPattern": return ElaParserError.InvalidLiteralPattern;
				case "BoolPattern": return ElaParserError.InvalidBooleanPattern;
				case "BindingPattern": return ElaParserError.InvalidBindingPattern;
				case "ForeachPattern": return ElaParserError.InvalidForeachPattern;
				case "IsOperatorPattern": return ElaParserError.InvalidIsOperatorPattern;
				case "RootExpr": return ElaParserError.InvalidRootExpression;
				case "IterExpr": return ElaParserError.InvalidIterationExpression;
				case "Assignment": return ElaParserError.InvalidAssignment;
				case "VariableDeclaration": return ElaParserError.InvalidVariableDeclaration;
				case "VariableDeclarationBody": return ElaParserError.InvalidVariableDeclaration;
				case "FunExpr": return ElaParserError.InvalidFunctionExpression;
				case "WhenExpr": return ElaParserError.InvalidWhenExpression;
				case "IncludeStat": return ElaParserError.InvalidIncludeExpression;
				case "WhileExpr": return ElaParserError.InvalidWhileExpression;
				case "DoWhileExpr": return ElaParserError.InvalidDoWhileExpression;
				case "ForExpr": return ElaParserError.InvalidForExpression;
				case "TryCatchExpr": return ElaParserError.InvalidTryCatchExpression;
				case "UnaryOp": return ElaParserError.InvalidUnaryOperator;
				case "SpecUnaryOp": return ElaParserError.InvalidUnaryOperator;
				case "PostUnaryOp": return ElaParserError.InvalidUnaryOperator;
				case "UnaryExpr": return ElaParserError.InvalidUnaryExpression;
				default: return ElaParserError.InvalidProduction;
			}
		}


		private static ElaParserError ExpectToken(int error, string prod)
		{
			switch (error)
			{
				case Parser._EOF: return ElaParserError.ExpectedEof;
				case Parser._ident: return ElaParserError.ExpectedIdentifierToken;
				case Parser._intTok: return ElaParserError.ExpectedIntToken;
				case Parser._realTok: return ElaParserError.ExpectedRealToken;
				case Parser._stringTok: return ElaParserError.ExpectedStringToken;
				case Parser._charTok: return ElaParserError.ExpectedCharToken;
				case Parser._operatorTok: return ElaParserError.ExpectedOperatorToken;
				case Parser._variantTok: return ElaParserError.ExpectedVariantToken;
				case Parser._SEMI: return ElaParserError.ExpectedSemicolon;
				case Parser._LBRA: return ElaParserError.ExpectedCurlyBrace;
				case Parser._RBRA: return ElaParserError.ExpectedCurlyBrace;
				case Parser._MATCH_ARR: return ElaParserError.ExpectedMatchOperator;
				case Parser._IN: return ElaParserError.ExpectedKeywordIn;
				case Parser._FTO: return ElaParserError.ExpectedKeywordTo;
				case Parser._DOWNTO: return ElaParserError.ExpectedKeywordDownto;
				case Parser._ON: return ElaParserError.ExpectedKeywordOn;
				case Parser._BASE: return ElaParserError.ExpectedKeywordBase;
				case Parser._MATCH: return ElaParserError.ExpectedKeywordMatch;
				case Parser._WHEN: return ElaParserError.ExpectedKeywordWhen;
				case Parser._AS: return ElaParserError.ExpectedKeywordAs;
				case Parser._IS: return ElaParserError.ExpectedKeywordIs;
				case Parser._VAR: return ElaParserError.ExpectedKeywordVar;
				case Parser._LET: return ElaParserError.ExpectedKeywordLet;
				case Parser._PRIVATE: return ElaParserError.ExpectedKeywordPrivate;
				case Parser._OPEN: return ElaParserError.ExpectedKeywordOpen;
				case Parser._AT: return ElaParserError.ExpectedKeywordAt;
				case Parser._WITH: return ElaParserError.ExpectedKeywordWith;
				case Parser._COUT: return ElaParserError.ExpectedKeywordCout;
				case Parser._TYPEOF: return ElaParserError.ExpectedKeywordTypeof;
				case Parser._WHILE: return ElaParserError.ExpectedKeywordWhile;
				case Parser._UNTIL: return ElaParserError.ExpectedKeywordUntil;
				case Parser._DO: return ElaParserError.ExpectedKeywordDo;
				case Parser._FOR: return ElaParserError.ExpectedKeywordFor;
				case Parser._CIF: return ElaParserError.ExpectedKeywordIf;
				case Parser._THROW: return ElaParserError.ExpectedKeywordThrow;
				case Parser._YIELD: return ElaParserError.ExpectedKeywordYield;
				case Parser._RETURN: return ElaParserError.ExpectedKeywordReturn;
				case Parser._BREAK: return ElaParserError.ExpectedKeywordBreak;
				case Parser._CONTINUE: return ElaParserError.ExpectedKeywordContinue;
				case Parser._TRY: return ElaParserError.ExpectedKeywordTry;
				case Parser._CATCH: return ElaParserError.ExpectedKeywordCatch;
				case Parser._TRUE: return ElaParserError.ExpectedBooleanToken;
				case Parser._FALSE: return ElaParserError.ExpectedBooleanToken;
				case Parser._ASYNC: return ElaParserError.ExpectedKeywordAsync;
				case Parser._LAZY: return ElaParserError.ExpectedKeywordLazy;
				case Parser._IGNOR: return ElaParserError.ExpectedKeywordIgnore;
				default: return ElaParserError.ExpectedToken;
			}
		}
		#endregion
	}
}
