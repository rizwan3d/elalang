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
				case "SimpleExpr": return ElaParserError.InvalidSimpleExpression;
				case "VariantLiteral": return ElaParserError.InvalidVariant;
				case "Primitive": return ElaParserError.InvalidPrimitive;
				case "VariableReference": return ElaParserError.InvalidVariableReference;
				case "TupleLiteral": return ElaParserError.InvalidTuple;
				case "GroupExpr": return ElaParserError.InvalidGrouping;
				case "MemberAccess": return ElaParserError.InvalidMemberAccess;
				case "ArgumentReference": return ElaParserError.InvalidArgumentReference;
				case "BaseReference": return ElaParserError.InvalidBaseReference;
				case "MatchExpr": return ElaParserError.InvalidMatch;
				case "MatchEntry": return ElaParserError.InvalidMatchEntry;
				case "Guard": return ElaParserError.InvalidGuard;
				case "RootPattern": return ElaParserError.InvalidPattern;
				case "ParenPattern": return ElaParserError.InvalidParenPattern;
				case "VariantPattern": return ElaParserError.InvalidVariantPattern;
				case "OrPattern": return ElaParserError.InvalidPattern;
				case "FuncPattern": return ElaParserError.InvalidFuncPattern;
				case "SinglePattern": return ElaParserError.InvalidPattern;
				case "AsPattern": return ElaParserError.InvalidPattern;
				case "ArrayPattern": return ElaParserError.InvalidArrayPattern;
				case "UnitPattern": return ElaParserError.InvalidUnitPattern;
				case "DefaultPattern": return ElaParserError.InvalidDefaultPattern;
				case "BindingPattern": return ElaParserError.InvalidBindingPattern;
				case "ForeachPattern": return ElaParserError.InvalidForeachPattern;
				case "IsOperatorPattern": return ElaParserError.InvalidIsOperatorPattern;
				case "IsPattern": return ElaParserError.InvalidPattern;
				case "RecordPattern": return ElaParserError.InvalidRecordPattern;
				case "FieldPattern": return ElaParserError.InvalidRecordPattern;
				case "TuplePattern": return ElaParserError.InvalidTuplePattern;
				case "ConsPattern": return ElaParserError.InvalidConsPattern;
				case "LiteralPattern": return ElaParserError.InvalidLiteralPattern;
				case "ListPattern": return ElaParserError.InvalidListPattern;
				case "RecordLiteral": return ElaParserError.InvalidRecord;
				case "RecordField": return ElaParserError.InvalidRecord;
				case "RangeExpr": return ElaParserError.InvalidRange;
				case "ListLiteral": return ElaParserError.InvalidList;
				case "ParamList": return ElaParserError.InvalidParamList;
				case "ArrayLiteral": return ElaParserError.InvalidArray;
				case "LetBinding": return ElaParserError.InvalidBinding;
				case "RootLetBinding": return ElaParserError.InvalidBinding;
				case "VariableAttributes": return ElaParserError.InvalidBinding;
				case "WhereBinding": return ElaParserError.InvalidWhereBinding;
				case "WhereBindingBody": return ElaParserError.InvalidWhereBinding;
				case "VariableDeclarationBody": return ElaParserError.InvalidBinding;
				case "FunExpr": return ElaParserError.InvalidFunction;
				case "FunBodyExpr": return ElaParserError.InvalidFunction;
				case "ChildFunBodyExpr": return ElaParserError.InvalidFunction;
				case "LambdaBodyExpr": return ElaParserError.InvalidLambda;
				case "LambdaExpr": return ElaParserError.InvalidLambda;
				case "IncludeStat": return ElaParserError.InvalidInclude;
				case "IncludeImports": return ElaParserError.InvalidInclude;
				case "WhileExpr": return ElaParserError.InvalidWhile;
				case "ForExpr": return ElaParserError.InvalidFor;
				case "IfExpr": return ElaParserError.InvalidIf;
				case "RaiseExpr": return ElaParserError.InvalidRaise;
				case "FailExpr": return ElaParserError.InvalidFail;
				case "ReturnExpr": return ElaParserError.InvalidReturn;
				case "BreakExpr": return ElaParserError.InvalidBreak;
				case "ContinueExpr": return ElaParserError.InvalidContinue;
				case "TryExpr": return ElaParserError.InvalidTry;
				case "AssignExpr": return ElaParserError.InvalidAssignment;
				case "BackwardPipeExpr": return ElaParserError.InvalidPipe;
				case "ForwardPipeExpr": return ElaParserError.InvalidPipe;
				case "OrExpr": return ElaParserError.InvalidOr;
				case "AndExpr": return ElaParserError.InvalidAnd;
				case "EqExpr": return ElaParserError.InvalidEq;
				case "ShiftExpr": return ElaParserError.InvalidShift;
				case "ConcatExpr": return ElaParserError.InvalidConcat;
				case "ConsExpr": return ElaParserError.InvalidCons;
				case "AddExpr": return ElaParserError.InvalidMath;
				case "MulExpr": return ElaParserError.InvalidMath;
				case "CastExpr": return ElaParserError.InvalidCast;
				case "InfixExpr": return ElaParserError.InvalidInfix;
				case "BitOrExpr": return ElaParserError.InvalidBitOr;
				case "BitXorExpr": return ElaParserError.InvalidBitXor;
				case "BitAndExpr": return ElaParserError.InvalidBitAnd;
				case "BackwardCompExpr": return ElaParserError.InvalidComp;
				case "ForwardCompExpr": return ElaParserError.InvalidComp;
				case "UnaryExpr": return ElaParserError.InvalidUnary;
				case "Application": return ElaParserError.InvalidApplication;
				case "AccessExpr": return ElaParserError.InvalidMemberAccess;
				case "FuncOperator": return ElaParserError.InvalidFuncOperator;
				case "OperatorExpr": return ElaParserError.InvalidFuncOperator;
				case "BinaryExpr": return ElaParserError.InvalidSequence;
				case "LazyExpr": return ElaParserError.InvalidLazy;
				case "ComprehensionExpr": return ElaParserError.InvalidComprehension;
				case "ComprehensionEntry": return ElaParserError.InvalidComprehension;				
				case "Expr": return ElaParserError.InvalidExpression;
				case "EmbExpr": return ElaParserError.InvalidExpression;
				case "IterExpr": return ElaParserError.InvalidExpression;
				case "DeclarationBlock": return ElaParserError.InvalidRoot;
				case "Ela": return ElaParserError.InvalidRoot;
				default: return ElaParserError.InvalidProduction;
			}
		}


		private static ElaParserError ExpectToken(int error, string prod)
		{
			switch (error)
			{
				case Parser._EOF: return ElaParserError.ExpectedEof;
				case Parser._ident: return ElaParserError.ExpectedIdentifierToken;
				case Parser._variantTok: return ElaParserError.ExpectedVariantToken;
				case Parser._argIdent: return ElaParserError.ExpectedArgumentIdentToken;
				case Parser._funcTok: return ElaParserError.ExpectedFunctionToken;				
				case Parser._intTok: return ElaParserError.ExpectedIntToken;
				case Parser._realTok: return ElaParserError.ExpectedRealToken;
				case Parser._stringTok: return ElaParserError.ExpectedStringToken;
				case Parser._charTok: return ElaParserError.ExpectedCharToken;
				case Parser._operatorTok: return ElaParserError.ExpectedOperatorToken;
				case Parser._SEMI: return ElaParserError.ExpectedSemicolon;
				case Parser._LBRA: return ElaParserError.ExpectedCurlyBrace;
				case Parser._RBRA: return ElaParserError.ExpectedCurlyBrace;
				case Parser._LILB: return ElaParserError.ExpectedSquareBrace;
				case Parser._LIRB: return ElaParserError.ExpectedSquareBrace;
				case Parser._ARROW: return ElaParserError.ExpectedArrow;
				case Parser._LAMBDA: return ElaParserError.ExpectedLambda;
				case Parser._EQ: return ElaParserError.ExpectedEq;
				case Parser._SEQ: return ElaParserError.ExpectedSeq;
				case Parser._DOT: return ElaParserError.ExpectedDot;
				case Parser._IN: return ElaParserError.ExpectedKeywordIn;
				case Parser._BASE: return ElaParserError.ExpectedKeywordBase;
				case Parser._MATCH: return ElaParserError.ExpectedKeywordMatch;
				case Parser._ASAMP: return ElaParserError.ExpectedKeywordAsAmp;
				case Parser._COMPH: return ElaParserError.ExpectedComprehensionOp;
				case Parser._IS: return ElaParserError.ExpectedIsOperator;
				case Parser._LET: return ElaParserError.ExpectedKeywordLet;
				case Parser._PRIVATE: return ElaParserError.ExpectedKeywordPrivate;
				case Parser._OPEN: return ElaParserError.ExpectedKeywordOpen;
				case Parser._WITH: return ElaParserError.ExpectedKeywordWith;
				case Parser._IFS: return ElaParserError.ExpectedKeywordIf;
				case Parser._ELSE: return ElaParserError.ExpectedKeywordElse;
				case Parser._THEN: return ElaParserError.ExpectedKeywordThen;				
				case Parser._RAISE: return ElaParserError.ExpectedKeywordRaise;
				case Parser._TRY: return ElaParserError.ExpectedKeywordTry;
				case Parser._TRUE: return ElaParserError.ExpectedBooleanToken;
				case Parser._FALSE: return ElaParserError.ExpectedBooleanToken;				
				case Parser._FAIL: return ElaParserError.ExpectedKeywordFail;
				case Parser._WHERE: return ElaParserError.ExpectedKeywordWhere;
				case Parser._AND: return ElaParserError.ExpectedKeywordAnd;
				case Parser._ENDS: return ElaParserError.ExpectedKeywordEnd;
				case Parser._COMPO: return ElaParserError.ExpectedComprehensionSlash;
				default: return ElaParserError.ExpectedToken;
			}
		}
		#endregion
	}
}
