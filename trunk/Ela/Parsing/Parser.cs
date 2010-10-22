
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Ela.CodeModel;


namespace Ela.Parsing {

internal sealed partial class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _intTok = 2;
	public const int _realTok = 3;
	public const int _stringTok = 4;
	public const int _charTok = 5;
	public const int _operatorTok = 6;
	public const int _variantTok = 7;
	public const int _SEMI = 8;
	public const int _LBRA = 9;
	public const int _RBRA = 10;
	public const int _MATCH_ARR = 11;
	public const int _ARG = 12;
	public const int _IN = 13;
	public const int _FTO = 14;
	public const int _DOWNTO = 15;
	public const int _ON = 16;
	public const int _BASE = 17;
	public const int _MATCH = 18;
	public const int _WHEN = 19;
	public const int _AS = 20;
	public const int _IS = 21;
	public const int _VAR = 22;
	public const int _LET = 23;
	public const int _PRIVATE = 24;
	public const int _OPEN = 25;
	public const int _AT = 26;
	public const int _WITH = 27;
	public const int _COUT = 28;
	public const int _TYPEOF = 29;
	public const int _WHILE = 30;
	public const int _UNTIL = 31;
	public const int _DO = 32;
	public const int _FOR = 33;
	public const int _CIF = 34;
	public const int _ELSE = 35;
	public const int _THROW = 36;
	public const int _YIELD = 37;
	public const int _RETURN = 38;
	public const int _BREAK = 39;
	public const int _CONTINUE = 40;
	public const int _TRY = 41;
	public const int _CATCH = 42;
	public const int _TRUE = 43;
	public const int _FALSE = 44;
	public const int _ASYNC = 45;
	public const int _LAZY = 46;
	public const int _IGNOR = 47;
	public const int maxT = 103;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;
	internal int errorCount;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors(this);
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}
	
	
	void AddError(ElaParserError error, params object[] args)
	{
		errors.AddErr(la.line, la.col, error, args);
	}
	
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void Literal(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 2: case 3: case 4: case 5: case 43: case 44: {
			Primitive(out exp);
			while (la.kind == 49 || la.kind == 53) {
				if (la.kind == 53) {
					Indexer(exp, out exp);
				} else {
					Invoke(exp, out exp);
				}
			}
			if (la.kind == 51) {
				MemberAccess(exp, out exp);
			}
			break;
		}
		case 53: {
			ListCreateExpr(out exp);
			while (la.kind == 49 || la.kind == 53) {
				if (la.kind == 53) {
					Indexer(exp, out exp);
				} else {
					Invoke(exp, out exp);
				}
			}
			if (la.kind == 51) {
				MemberAccess(exp, out exp);
			}
			break;
		}
		case 57: {
			ArrayCreateExpr(out exp);
			while (la.kind == 49 || la.kind == 53) {
				if (la.kind == 53) {
					Indexer(exp, out exp);
				} else {
					Invoke(exp, out exp);
				}
			}
			if (la.kind == 51) {
				MemberAccess(exp, out exp);
			}
			break;
		}
		case 17: {
			Get();
			exp = new ElaBaseReference(t); 
			if (la.kind == 51) {
				MemberAccess(exp, out exp);
			}
			break;
		}
		case 1: {
			Get();
			exp = new ElaVariableReference(t) { VariableName = t.val }; 
			while (la.kind == 49 || la.kind == 53) {
				if (la.kind == 53) {
					Indexer(exp, out exp);
				} else {
					Invoke(exp, out exp);
				}
			}
			if (la.kind == 51) {
				MemberAccess(exp, out exp);
			}
			break;
		}
		case 12: {
			Get();
			Expect(1);
			exp = new ElaArgument(t) { ArgumentName = t.val }; 
			while (la.kind == 49 || la.kind == 53) {
				if (la.kind == 53) {
					Indexer(exp, out exp);
				} else {
					Invoke(exp, out exp);
				}
			}
			if (la.kind == 51) {
				MemberAccess(exp, out exp);
			}
			break;
		}
		case 7: {
			Get();
			var ev = new ElaVariantLiteral(t) { Name = t.val.TrimStart('`') }; 
			exp = ev;
			
			if (la.kind == 49) {
				Invoke(exp, out exp);
				ev.Length = ((ElaFunctionCall)exp).Parameters.Count; 
			} else if (StartOf(1)) {
				exp = new ElaFunctionCall(t) { Target = exp }; 
			} else SynErr(104);
			if (la.kind == 53) {
				Indexer(exp, out exp);
				while (la.kind == 49 || la.kind == 53) {
					if (la.kind == 53) {
						Indexer(exp, out exp);
					} else {
						Invoke(exp, out exp);
					}
				}
			}
			if (la.kind == 51) {
				MemberAccess(exp, out exp);
			}
			break;
		}
		case 48: {
			Get();
			exp = new ElaPlaceholder(t); 
			while (la.kind == 49 || la.kind == 53) {
				if (la.kind == 53) {
					Indexer(exp, out exp);
				} else {
					Invoke(exp, out exp);
				}
			}
			if (la.kind == 51) {
				MemberAccess(exp, out exp);
			}
			break;
		}
		default: SynErr(105); break;
		}
	}

	void Primitive(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 2: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseInt(t.val) };	
			break;
		}
		case 3: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseReal(t.val) }; 
			break;
		}
		case 4: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseString(t.val) }; 
			break;
		}
		case 5: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseChar(t.val) }; 
			break;
		}
		case 43: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(true) }; 
			break;
		}
		case 44: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(false) }; 
			break;
		}
		default: SynErr(106); break;
		}
	}

	void Indexer(ElaExpression target, out ElaExpression exp) {
		Expect(53);
		var indExp = new ElaIndexer(t) { TargetObject = target };
		exp = indExp;
		
		var cexp = default(ElaExpression); 
		Expr(out cexp);
		indExp.Index = cexp;	
		Expect(54);
	}

	void Invoke(ElaExpression target, out ElaExpression exp) {
		Expect(49);
		var mi = new ElaFunctionCall(t) { Target = target };				
		exp = mi;
		
		if (StartOf(2)) {
			InvokeParameterList(mi);
		}
		Expect(50);
	}

	void MemberAccess(ElaExpression target, out ElaExpression exp) {
		Expect(51);
		exp = default(ElaExpression); 
		Expect(1);
		exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = target }; 
		while (la.kind == 49 || la.kind == 53) {
			if (la.kind == 53) {
				Indexer(exp, out exp);
			} else {
				Invoke(exp, out exp);
			}
		}
		if (la.kind == 51) {
			MemberAccess(exp, out exp);
		}
	}

	void ListCreateExpr(out ElaExpression exp) {
		Expect(53);
		var listExp = new ElaListLiteral(t);
		exp = listExp;
		
		if (StartOf(3)) {
			if (StartOf(4)) {
				ListParamList(listExp.Values);
			} else {
				var cexp = default(ElaExpression); 
				IterExpr(out cexp);
				listExp.Comprehension = cexp; 
			}
		}
		Expect(54);
	}

	void ArrayCreateExpr(out ElaExpression exp) {
		Expect(57);
		var arrExp = new ElaArrayLiteral(t);
		exp = arrExp;
		
		if (StartOf(3)) {
			if (StartOf(4)) {
				ListParamList(arrExp.Values);
			} else {
				var cexp = default(ElaExpression); 
				IterExpr(out cexp);
				arrExp.Comprehension = cexp; 
			}
		}
		Expect(58);
	}

	void InvokeParameterList(ElaFunctionCall call ) {
		var exp = default(ElaExpression); 
		Expr(out exp);
		call.Parameters.Add(exp); 
		if (la.kind == 52) {
			Get();
			call.ConvertParametersToTuple = true; 
			if (StartOf(2)) {
				call.ConvertParametersToTuple = false; 
				InvokeParameterList(call);
			}
		}
	}

	void Expr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 9: {
			Block(out exp);
			break;
		}
		case 47: {
			IgnoreExpr(out exp);
			break;
		}
		case 1: case 2: case 3: case 4: case 5: case 6: case 7: case 12: case 17: case 43: case 44: case 48: case 49: case 53: case 57: case 66: case 83: case 85: case 86: case 87: case 88: case 89: {
			BinaryExpr(out exp);
			break;
		}
		case 19: case 84: {
			WhenExpr(out exp);
			break;
		}
		case 34: {
			IfExpr(out exp);
			break;
		}
		case 18: {
			MatchExpr(out exp);
			break;
		}
		case 30: case 31: case 32: case 33: {
			IterExpr(out exp);
			break;
		}
		case 22: case 23: {
			VariableDeclaration(out exp);
			break;
		}
		case 36: {
			ThrowExpr(out exp);
			break;
		}
		case 28: {
			CoutExpr(out exp);
			break;
		}
		case 29: {
			TypeofExpr(out exp);
			break;
		}
		case 38: {
			ReturnExpr(out exp);
			break;
		}
		case 37: {
			YieldExpr(out exp);
			break;
		}
		case 39: {
			BreakExpr(out exp);
			break;
		}
		case 40: {
			ContinueExpr(out exp);
			break;
		}
		case 41: {
			TryCatchExpr(out exp);
			break;
		}
		case 45: {
			AsyncExpr(out exp);
			break;
		}
		case 46: {
			LazyExpr(out exp);
			break;
		}
		default: SynErr(107); break;
		}
		exp = GetFun(exp); 
	}

	void MatchExpr(out ElaExpression exp) {
		Expect(18);
		var match = new ElaMatch(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expect(49);
		Expr(out cexp);
		if (la.kind == 52) {
			Get();
			TupleExpr(ref cexp);
		}
		Expect(50);
		match.Expression = cexp; 
		MatchEntry(match);
		while (la.kind == 16) {
			MatchEntry(match);
		}
	}

	void TupleExpr(ref ElaExpression exp) {
		var pexp = new ElaTupleLiteral(t, false);
		pexp.Parameters.Add(exp);
		exp = pexp;				
		var cexp = default(ElaExpression);				
		
		if (StartOf(2)) {
			Expr(out cexp);
			pexp.Parameters.Add(cexp); 
		}
		while (la.kind == 52) {
			Get();
			Expr(out cexp);
			pexp.Parameters.Add(cexp); 
		}
	}

	void MatchEntry(ElaMatch match) {
		var cexp = default(ElaExpression); 
		Expect(16);
		var pat = default(ElaPattern); 
		RootPattern(out pat);
		var entry = new ElaMatchEntry(t);
		entry.Pattern = pat;
		match.Entries.Add(entry);
		
		if (la.kind == 19) {
			Guard(out cexp);
			pat.Guard = cexp; 
		}
		Expect(11);
		Expr(out cexp);
		entry.Expression = cexp; 
	}

	void RootPattern(out ElaPattern pat) {
		OrPattern(out pat);
		if (la.kind == 59) {
			ConsPattern(pat, out pat);
		}
		if (la.kind == 52) {
			SeqPattern(pat, out pat);
		}
	}

	void Guard(out ElaExpression exp) {
		Expect(19);
		Expect(49);
		Expr(out exp);
		Expect(50);
	}

	void OrPattern(out ElaPattern pat) {
		AndPattern(out pat);
		while (la.kind == 55) {
			Get();
			var orPat = new ElaOrPattern(t) { Left = pat }; 
			AndPattern(out pat);
			orPat.Right = pat;
			pat = orPat;
			
		}
	}

	void ConsPattern(ElaPattern prev, out ElaPattern exp) {
		var cexp = default(ElaPattern); 
		var ht = new ElaHeadTailPattern(t); 
		ht.Patterns.Add(prev);
		exp = ht;				
		
		Expect(59);
		SinglePattern(out cexp);
		ht.Patterns.Add(cexp); 
		while (la.kind == 59) {
			Get();
			SinglePattern(out cexp);
			ht.Patterns.Add(cexp); 
		}
	}

	void SeqPattern(ElaPattern prev, out ElaPattern pat) {
		var seq = new ElaSeqPattern(t); 
		seq.Patterns.Add(prev);
		var cpat = default(ElaPattern);
		pat = seq;
		
		Expect(52);
		if (StartOf(5)) {
			OrPattern(out cpat);
			if (la.kind == 59) {
				ConsPattern(cpat, out cpat);
			}
			seq.Patterns.Add(cpat); 
			while (la.kind == 52) {
				Get();
				OrPattern(out cpat);
				if (la.kind == 59) {
					ConsPattern(cpat, out cpat);
				}
				seq.Patterns.Add(cpat); 
			}
		}
	}

	void AndPattern(out ElaPattern pat) {
		SinglePattern(out pat);
		while (la.kind == 56) {
			Get();
			var andPat = new ElaAndPattern(t) { Left = pat }; 
			SinglePattern(out pat);
			andPat.Right = pat;
			pat = andPat;
			
		}
	}

	void SinglePattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 48: {
			Get();
			pat = new ElaDefaultPattern(t); 
			break;
		}
		case 2: case 3: case 4: case 5: case 43: case 44: {
			LiteralPattern(out pat);
			break;
		}
		case 60: case 61: case 62: case 63: case 64: case 65: {
			BoolPattern(null, out pat);
			break;
		}
		case 68: {
			FieldTestPattern(out pat);
			break;
		}
		case 7: {
			CtorPattern(out pat);
			break;
		}
		case 69: {
			AnyCtorPattern(out pat);
			break;
		}
		case 21: {
			IsPattern(null, out pat);
			break;
		}
		case 1: {
			Get();
			var name = t.val; 
			if (la.kind == 21) {
				IsPattern(name, out pat);
			} else if (StartOf(6)) {
				BoolPattern(name, out pat);
			} else if (la.kind == 67) {
				FieldPattern(name, out pat);
			} else if (StartOf(7)) {
				pat = new ElaVariablePattern(t) { Name = name };  
			} else SynErr(108);
			break;
		}
		case 66: {
			ValueofPattern(out pat);
			break;
		}
		case 53: {
			ListPattern(out pat);
			break;
		}
		case 57: {
			ArrayPattern(out pat);
			break;
		}
		case 49: {
			Get();
			if (la.kind == 50) {
				VoidPattern(out pat);
			} else if (StartOf(5)) {
				RootPattern(out pat);
				Expect(50);
			} else SynErr(109);
			break;
		}
		default: SynErr(110); break;
		}
		if (la.kind == 20) {
			Get();
			var asPat = new ElaAsPattern(t) { Pattern = pat }; 
			pat = asPat;				
			
			Expect(1);
			asPat.Name = t.val; 
		}
	}

	void LiteralPattern(out ElaPattern pat) {
		var val = new ElaLiteralPattern(t); 
		pat = val;
		
		switch (la.kind) {
		case 4: {
			Get();
			val.Value = ParseString(t.val); 
			break;
		}
		case 5: {
			Get();
			val.Value = ParseChar(t.val); 
			break;
		}
		case 2: {
			Get();
			val.Value = ParseInt(t.val); 
			break;
		}
		case 3: {
			Get();
			val.Value = ParseReal(t.val); 
			break;
		}
		case 43: {
			Get();
			val.Value = new ElaLiteralValue(true); 
			break;
		}
		case 44: {
			Get();
			val.Value = new ElaLiteralValue(false); 
			break;
		}
		default: SynErr(111); break;
		}
	}

	void BoolPattern(string name, out ElaPattern pat) {
		pat = null; 
		var op = ElaBinaryOperator.None; 
		var right = default(ElaPattern);
		
		switch (la.kind) {
		case 60: {
			Get();
			op = ElaBinaryOperator.Greater; 
			break;
		}
		case 61: {
			Get();
			op = ElaBinaryOperator.Lesser; 
			break;
		}
		case 62: {
			Get();
			op = ElaBinaryOperator.GreaterEqual; 
			break;
		}
		case 63: {
			Get();
			op = ElaBinaryOperator.LesserEqual; 
			break;
		}
		case 64: {
			Get();
			op = ElaBinaryOperator.Equals; 
			break;
		}
		case 65: {
			Get();
			op = ElaBinaryOperator.NotEquals; 
			break;
		}
		default: SynErr(112); break;
		}
		if (la.kind == 66) {
			ValueofPattern(out right);
		} else if (StartOf(8)) {
			LiteralPattern(out right);
		} else SynErr(113);
		var grd = new ElaBoolPattern(t) { Left = name, Right = right, Operator = op }; 
		pat = grd; 
		
	}

	void FieldTestPattern(out ElaPattern pat) {
		var fld = new ElaFieldPattern(t); 
		Expect(68);
		Expect(1);
		fld.Name = t.val; pat = fld; 
	}

	void CtorPattern(out ElaPattern pat) {
		var cons = default(ElaConstructorPattern);
		var consTok = t;
		var cp = default(ElaPattern);
		var seq = default(ElaSeqPattern);
		var name = String.Empty;
		pat = null;
		
		Expect(7);
		name = t.val.TrimStart('`'); 
		if (la.kind == 49) {
			Get();
			if (StartOf(5)) {
				RootPattern(out cp);
				seq = cp as ElaSeqPattern; 
			}
			Expect(50);
		}
		cons = new ElaConstructorPattern(consTok, seq) { Name = name };
		pat = cons;
			
		if (seq == null && cp != null)
			cons.Patterns.Add(cp);
		
	}

	void AnyCtorPattern(out ElaPattern pat) {
		var cons = default(ElaConstructorPattern);
		var consTok = t;
		var cp = default(ElaPattern);
		var seq = default(ElaSeqPattern);
		pat = null;
		
		Expect(69);
		if (la.kind == 49) {
			Get();
			if (StartOf(5)) {
				RootPattern(out cp);
				seq = cp as ElaSeqPattern; 
			}
			Expect(50);
		}
		cons = new ElaConstructorPattern(consTok, seq);
		pat = cons;
			
		if (seq == null && cp != null)
			cons.Patterns.Add(cp);
		
	}

	void IsPattern(string name, out ElaPattern pat) {
		pat = null; 
		Expect(21);
		var typ = new ElaIsPattern(t) { VariableName = name }; pat = typ; 
		Expect(1);
		typ.TypeAffinity = GetType(t.val); 
	}

	void FieldPattern(string name, out ElaPattern pat) {
		var fld = new ElaFieldPattern(t) { Name = name }; 
		Expect(67);
		SinglePattern(out pat);
		fld.Value = pat; pat = fld; 
	}

	void ValueofPattern(out ElaPattern pat) {
		pat = null; 
		Expect(66);
		var val = new ElaValueofPattern(t); pat = val; 
		Expect(1);
		val.VariableName = t.val; 
	}

	void ListPattern(out ElaPattern pat) {
		pat = null; 
		Expect(53);
		var listPat = default(ElaListPattern); 
		var listTok = t;					
		var cp = default(ElaPattern);
		
		if (StartOf(5)) {
			RootPattern(out cp);
		}
		var seq = cp as ElaSeqPattern;
		listPat = new ElaListPattern(listTok, seq);
		pat = listPat;
			
		if (seq == null && cp != null)
			listPat.Patterns.Add(cp);
		
		Expect(54);
	}

	void ArrayPattern(out ElaPattern pat) {
		var seq = new ElaArrayPattern(t); 
		var cpat = default(ElaPattern);
		pat = seq;
		
		Expect(57);
		SinglePattern(out cpat);
		seq.Patterns.Add(cpat);  
		while (la.kind == 52) {
			Get();
			SinglePattern(out cpat);
			seq.Patterns.Add(cpat); 
		}
		Expect(58);
	}

	void VoidPattern(out ElaPattern pat) {
		pat = new ElaVoidPattern(t); 
		Expect(50);
	}

	void BindingPattern(out string name, out ElaPattern pat) {
		pat = null; name = null; 
		if (la.kind == 1) {
			Get();
			name = t.val; 
			if (StartOf(6)) {
				name = null; 
				BoolPattern(null, out pat);
			}
		} else if (la.kind == 53) {
			ListPattern(out pat);
		} else if (la.kind == 57) {
			ArrayPattern(out pat);
		} else if (la.kind == 49) {
			Get();
			RootPattern(out pat);
			Expect(50);
		} else SynErr(114);
	}

	void ForeachPattern(out ElaPattern pat) {
		pat = null; string name = null; 
		switch (la.kind) {
		case 1: {
			Get();
			name = t.val; 
			if (StartOf(6)) {
				BoolPattern(name, out pat);
			} else if (StartOf(9)) {
				pat = new ElaVariablePattern(t) { Name = name }; 
			} else SynErr(115);
			break;
		}
		case 7: {
			CtorPattern(out pat);
			break;
		}
		case 69: {
			AnyCtorPattern(out pat);
			break;
		}
		case 2: case 3: case 4: case 5: case 43: case 44: {
			LiteralPattern(out pat);
			break;
		}
		case 53: {
			ListPattern(out pat);
			break;
		}
		case 57: {
			ArrayPattern(out pat);
			break;
		}
		case 49: {
			Get();
			RootPattern(out pat);
			Expect(50);
			break;
		}
		default: SynErr(116); break;
		}
	}

	void IsOperatorPattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 1: {
			Get();
			pat = new ElaIsPattern(t) { TypeAffinity = GetType(t.val) }; 
			break;
		}
		case 7: {
			CtorPattern(out pat);
			break;
		}
		case 69: {
			AnyCtorPattern(out pat);
			break;
		}
		case 2: case 3: case 4: case 5: case 43: case 44: {
			LiteralPattern(out pat);
			break;
		}
		case 53: {
			ListPattern(out pat);
			break;
		}
		case 57: {
			ArrayPattern(out pat);
			break;
		}
		case 49: {
			Get();
			if (la.kind == 50) {
				VoidPattern(out pat);
			} else if (StartOf(5)) {
				RootPattern(out pat);
				Expect(50);
			} else SynErr(117);
			break;
		}
		default: SynErr(118); break;
		}
	}

	void Block(out ElaExpression exp) {
		Expect(9);
		var block = new ElaBlock(t); 
		exp = block;
		
		while (StartOf(10)) {
			RootExpr(block.Expressions);
		}
		Expect(10);
	}

	void RootExpr(List<ElaExpression> list ) {
		var exp = default(ElaExpression); 
		if (StartOf(2)) {
			Expr(out exp);
			if (la.kind != _RBRA && t.kind != _RBRA && t.kind != _SEMI) 
			if (la.kind == 8) {
				Get();
			} else if (la.kind == 0) {
				Get();
			} else SynErr(119);
		} else if (la.kind == 8) {
			Get();
		} else if (la.kind == 25) {
			IncludeStat(out exp);
			Expect(8);
		} else SynErr(120);
		if (exp != null) list.Add(exp); 
	}

	void ListParamList(List<ElaExpression> list ) {
		var exp = default(ElaExpression); 
		BinaryExpr(out exp);
		list.Add(exp); 
		if (la.kind == 52) {
			Get();
			ListParamList(list);
		}
	}

	void BinaryExpr(out ElaExpression exp) {
		OrExpr(out exp);
		if (StartOf(11)) {
			if (la.kind == 11) {
				Get();
				FunExpr(ref exp);
			} else {
				Assignment(exp, out exp);
			}
		}
	}

	void IterExpr(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 30 || la.kind == 31) {
			WhileExpr(out exp);
		} else if (la.kind == 32) {
			DoWhileExpr(out exp);
		} else if (la.kind == 33) {
			ForExpr(out exp);
		} else SynErr(121);
	}

	void Assignment(ElaExpression left, out ElaExpression exp) {
		var right = default(ElaExpression); exp = null; 
		if (StartOf(12)) {
			switch (la.kind) {
			case 67: {
				Get();
				Expr(out right);
				break;
			}
			case 70: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.Add, Left = left, Right = right }; 
				break;
			}
			case 71: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.Subtract, Left = left, Right = right }; 
				break;
			}
			case 72: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.Multiply, Left = left, Right = right }; 
				break;
			}
			case 73: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.Divide, Left = left, Right = right };
				break;
			}
			case 74: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.Modulus, Left = left, Right = right }; 
				break;
			}
			case 75: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.BitwiseOr, Left = left, Right = right }; 
				break;
			}
			case 76: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.BitwiseAnd, Left = left, Right = right }; 
				break;
			}
			case 77: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.BitwiseXor, Left = left, Right = right }; 
				break;
			}
			case 78: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.ShiftRight, Left = left, Right = right}; 
				break;
			}
			case 79: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.ShiftLeft, Left = left, Right = right}; 
				break;
			}
			case 80: {
				Get();
				Expr(out right);
				right = new ElaBinary(t) { Operator = ElaBinaryOperator.ConsList, Left = right, Right = left}; 
				break;
			}
			}
			exp	= new ElaAssign(t) { Left = left, Right = right }; 
		} else if (la.kind == 81) {
			Get();
			Expr(out right);
			exp = new ElaBinary(t) { Operator = ElaBinaryOperator.Swap, Left = left, Right = right }; 
		} else SynErr(122);
	}

	void VariableDeclaration(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 22) {
			Get();
			VariableDeclarationBody(ElaVariableFlags.None, out exp);
		} else if (la.kind == 23) {
			Get();
			VariableDeclarationBody(ElaVariableFlags.Immutable, out exp);
		} else SynErr(123);
	}

	void VariableDeclarationBody(ElaVariableFlags flags, out ElaExpression exp) {
		var varExp = new ElaVariableDeclaration(t) { VariableFlags = flags }; 
		exp = varExp;
		var cexp = default(ElaExpression); 
		
		if (la.kind == 24) {
			Get();
			varExp.VariableFlags |= ElaVariableFlags.Private; 
		}
		if (StartOf(13)) {
			var pat = default(ElaPattern); string name = null; 
			BindingPattern(out name, out pat);
			if (name != null)
			varExp.VariableName = name;
			else
				varExp.Pattern = pat; 
			
			if (la.kind == 19) {
				Guard(out cexp);
				if (pat != null) pat.Guard = cexp; 
			}
			if (la.kind == 49 || la.kind == 67) {
				if (la.kind == 49) {
					if (name == null) AddError(ElaParserError.InvalidFunctionDeclaration); 
					FunDecl(out cexp);
				} else {
					Get();
					Expr(out cexp);
				}
			}
			if (cexp != null) 
			{
				if (cexp.Type == ElaNodeType.FunctionLiteral &&  (flags & ElaVariableFlags.Immutable) == ElaVariableFlags.Immutable && name != null)
				{
					((ElaFunctionLiteral)cexp).Name = name;
					varExp.VariableFlags |= ElaVariableFlags.Function; 
				}
				else if (cexp.Type < ElaNodeType.FunctionLiteral)
					 varExp.VariableFlags |= ElaVariableFlags.ObjectLiteral;
			}
			
		} else if (la.kind == 7) {
			Get();
			varExp.VariableName = t.val.Substring(1);
			varExp.VariableFlags |= ElaVariableFlags.Constructor;
			
			FunDecl(out cexp);
			var fl = (ElaFunctionLiteral)cexp;
			fl.FunctionType = ElaFunctionType.Constructor; 
			fl.Name = varExp.VariableName;
			
		} else if (la.kind == 82) {
			Get();
			Expect(49);
			Expect(6);
			varExp.VariableName = t.val; 
			Expect(50);
			FunDecl(out cexp);
			((ElaFunctionLiteral)cexp).FunctionType = ElaFunctionType.BinaryOperator; 
		} else if (la.kind == 83) {
			Get();
			Expect(49);
			Expect(6);
			varExp.VariableName = t.val; 
			Expect(50);
			FunDecl(out cexp);
			((ElaFunctionLiteral)cexp).FunctionType = ElaFunctionType.UnaryOperator; 
		} else SynErr(124);
		varExp.InitExpression = cexp; 
	}

	void FunDecl(out ElaExpression exp) {
		Expect(49);
		var tp = new ElaTupleLiteral(t, false); exp = tp; 
		if (la.kind == 1) {
			Get();
			tp.Parameters.Add(new ElaVariableReference(t) { VariableName = t.val }); 
			while (la.kind == 52) {
				Get();
				Expect(1);
				tp.Parameters.Add(new ElaVariableReference(t) { VariableName = t.val }); 
			}
		}
		Expect(50);
		FunExpr(ref exp);
	}

	void FunExpr(ref ElaExpression exp) {
		var mi = new ElaFunctionLiteral(t);
		mi.Parameters = exp;
		funs.Push(mi);
		
		if (la.kind == 16) {
			FunMatchExpr(mi);
		} else if (StartOf(2)) {
			var fexp = default(ElaExpression); 
			Expr(out fexp);
			mi.Expression = fexp; 
		} else if (la.kind == 8) {
			Get();
		} else SynErr(125);
		exp = mi; 
		funs.Pop();
		
	}

	void WhenExpr(out ElaExpression exp) {
		var unless = false; 
		if (la.kind == 19) {
			Get();
		} else if (la.kind == 84) {
			Get();
			unless = true; 
		} else SynErr(126);
		var cond = new ElaCondition(t) { Unless = unless }; 
		var cexp = default(ElaExpression);	
		exp = cond;
		
		Expect(49);
		Expr(out cexp);
		cond.Condition = cexp; 
		Expect(50);
		Expr(out cexp);
		cond.True = cexp; 
	}

	void IncludeStat(out ElaExpression exp) {
		exp = null; 
		Expect(25);
		var inc = new ElaModuleInclude(t); 
		Expect(1);
		inc.Alias = inc.Name = t.val;
		exp = inc;
		
		if (la.kind == 53) {
			Get();
			if (la.kind == 1) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 4) {
				Get();
				inc.DllName = ReadString(t.val); 
			} else SynErr(127);
			Expect(54);
		}
		if (la.kind == 26) {
			Get();
			Expect(4);
			inc.Folder = ReadString(t.val); 
		}
		if (la.kind == 20) {
			Get();
			Expect(1);
			inc.Alias = t.val; 
		}
		if (la.kind == 27) {
			Get();
			IncludeImports(inc);
		}
	}

	void IncludeImports(ElaModuleInclude inc) {
		Expect(1);
		var imp = new ElaImportedName(t) { LocalName = t.val, ExternalName = t.val }; 
		inc.Imports.Add(imp); 
		
		if (la.kind == 67) {
			Get();
			Expect(1);
			imp.ExternalName = t.val; 
		}
		if (la.kind == 52) {
			Get();
			IncludeImports(inc);
		}
	}

	void CoutExpr(out ElaExpression exp) {
		exp = null; 
		var cexp = default(ElaExpression); 
		Expect(28);
		Expr(out cexp);
		exp = new ElaCout(t) { Expression = cexp }; 
	}

	void TypeofExpr(out ElaExpression exp) {
		exp = null; 
		var cexp = default(ElaExpression); 
		Expect(29);
		Expect(49);
		Expr(out cexp);
		Expect(50);
		exp = new ElaTypeof(t) { Expression = cexp }; 
	}

	void WhileExpr(out ElaExpression exp) {
		var wt = ElaWhileType.While; 
		if (la.kind == 30) {
			Get();
		} else if (la.kind == 31) {
			Get();
			wt = ElaWhileType.Until; 
		} else SynErr(128);
		var wex = new ElaWhile(t);
		exp = wex;
		var cexp = default(ElaExpression);
		wex.WhileType = wt;
		
		Expect(49);
		Expr(out cexp);
		wex.Condition = cexp; 
		Expect(50);
		Expr(out cexp);
		wex.Body = cexp; 
	}

	void DoWhileExpr(out ElaExpression exp) {
		Expect(32);
		var wex = new ElaWhile(t);
		exp = wex;
		var cexp = default(ElaExpression);
		wex.WhileType = ElaWhileType.DoWhile;
		
		Expr(out cexp);
		wex.Body = cexp; 
		if (la.kind == 8) {
			Get();
		}
		if (la.kind == 30) {
			Get();
		} else if (la.kind == 31) {
			Get();
			wex.WhileType = ElaWhileType.DoUntil; 
		} else SynErr(129);
		Expect(49);
		Expr(out cexp);
		wex.Condition = cexp; 
		Expect(50);
	}

	void ForExpr(out ElaExpression exp) {
		Expect(33);
		var ot = t;
		var it = new ElaFor(t);
		exp = it;
		var cexp = default(ElaExpression);
		var pat = default(ElaPattern);
		
		Expect(49);
		if (la.kind == 22 || la.kind == 23) {
			if (la.kind == 22) {
				Get();
			} else {
				Get();
				it.VariableFlags = ElaVariableFlags.Immutable; 
			}
		}
		ForeachPattern(out pat);
		if (la.kind == 67) {
			Get();
			OrExpr(out cexp);
			it.InitExpression = cexp; 
		}
		if (la.kind == 19) {
			Get();
			Expr(out cexp);
			pat.Guard = cexp; 
		}
		it.Pattern = pat; 
		if (la.kind == 13) {
			Get();
			it.ForType = ElaForType.Foreach; 
		} else if (la.kind == 14) {
			Get();
			it.ForType = ElaForType.ForTo; 
		} else if (la.kind == 15) {
			Get();
			it.ForType = ElaForType.ForDownto; 
		} else SynErr(130);
		Expr(out cexp);
		it.Target = cexp; 
		Expect(50);
		Expr(out cexp);
		it.Body = cexp; 
	}

	void OrExpr(out ElaExpression exp) {
		AndExpr(out exp);
		while (la.kind == 55) {
			var cexp = default(ElaExpression); 
			Get();
			AndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaBinaryOperator.BooleanOr, Right = cexp }; 
			
		}
	}

	void IfExpr(out ElaExpression exp) {
		Expect(34);
		var cond = new ElaCondition(t); 
		var cexp = default(ElaExpression);	
		exp = cond;
		
		Expect(49);
		Expr(out cexp);
		cond.Condition = cexp; 
		Expect(50);
		Expr(out cexp);
		cond.True = cexp; 
		if (la.kind == 8) {
			Get();
		}
		Expect(35);
		Expr(out cexp);
		cond.False = cexp; 
	}

	void ThrowExpr(out ElaExpression exp) {
		Expect(36);
		exp = null;
		var cexp = default(ElaExpression); 
		
		Expr(out cexp);
		exp = new ElaThrow(t) { Expression = cexp }; 
	}

	void YieldExpr(out ElaExpression exp) {
		Expect(37);
		var cexp = default(ElaExpression);
		Expr(out cexp);
		exp = new ElaYield(t) { Expression = cexp }; 
		
		if (funs.Count > 0)
			funs.Peek().Flags |= ElaExpressionFlags.HasYield;
		
	}

	void ReturnExpr(out ElaExpression exp) {
		Expect(38);
		var cexp = default(ElaExpression);
		Expr(out cexp);
		exp = new ElaReturn(t) { Expression = cexp }; 
	}

	void BreakExpr(out ElaExpression exp) {
		Expect(39);
		exp = new ElaBreak(t); 
	}

	void ContinueExpr(out ElaExpression exp) {
		Expect(40);
		exp = new ElaContinue(t); 
	}

	void TryCatchExpr(out ElaExpression exp) {
		exp = null; 
		var cexp = default(ElaExpression);			
		
		Expect(41);
		var ty = new ElaTryCatch(t); exp = ty; 
		Expr(out cexp);
		ty.Try = cexp; 
		if (la.kind == 8) {
			Get();
		}
		Expect(42);
		Expect(49);
		Expect(1);
		ty.Variable = new ElaVariableReference(t) { VariableName = t.val }; 
		Expect(50);
		if (la.kind == 16) {
			TryCatchMatchExpr(ty);
		} else if (StartOf(2)) {
			Expr(out cexp);
			ty.Catch = cexp; 
		} else SynErr(131);
	}

	void TryCatchMatchExpr(ElaTryCatch exp) {
		var match = new ElaMatch(t);
		match.Expression = exp.Variable;
		exp.Catch = match;
		
		MatchEntry(match);
		while (la.kind == 16) {
			MatchEntry(match);
		}
	}

	void UnaryOp(out ElaUnaryOperator op) {
		op = ElaUnaryOperator.None; 
		switch (la.kind) {
		case 85: {
			Get();
			op = ElaUnaryOperator.Negate; 
			break;
		}
		case 86: {
			Get();
			op = ElaUnaryOperator.Positive; 
			break;
		}
		case 87: {
			Get();
			op = ElaUnaryOperator.BooleanNot; 
			break;
		}
		case 83: {
			Get();
			op = ElaUnaryOperator.BitwiseNot; 
			break;
		}
		case 66: {
			Get();
			op = ElaUnaryOperator.ValueOf; 
			break;
		}
		case 6: {
			Get();
			op = ElaUnaryOperator.Custom; 
			break;
		}
		default: SynErr(132); break;
		}
	}

	void SpecUnaryOp(out ElaUnaryOperator op) {
		op = ElaUnaryOperator.None; 
		if (la.kind == 88) {
			Get();
			op = ElaUnaryOperator.Decrement; 
		} else if (la.kind == 89) {
			Get();
			op = ElaUnaryOperator.Increment; 
		} else SynErr(133);
	}

	void PostUnaryOp(out ElaUnaryOperator op) {
		op = ElaUnaryOperator.None; 
		if (la.kind == 88) {
			Get();
			op = ElaUnaryOperator.PostDecrement; 
		} else if (la.kind == 89) {
			Get();
			op = ElaUnaryOperator.PostIncrement; 
		} else SynErr(134);
	}

	void AndExpr(out ElaExpression exp) {
		IsExpr(out exp);
		while (la.kind == 56) {
			var cexp = default(ElaExpression); 
			Get();
			IsExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaBinaryOperator.BooleanAnd, Right = cexp }; 
			
		}
	}

	void IsExpr(out ElaExpression exp) {
		CompExpr(out exp);
		var pat = default(ElaPattern); 
		
		while (la.kind == 21) {
			Get();
			IsOperatorPattern(out pat);
			exp = new ElaIs(t) { Expression = exp, Pattern = pat }; 
		}
	}

	void CompExpr(out ElaExpression exp) {
		ConsExpr(out exp);
		while (la.kind == 90 || la.kind == 91) {
			var cexp = default(ElaExpression); 
			var op = default(ElaBinaryOperator);
			
			if (la.kind == 90) {
				Get();
				op = ElaBinaryOperator.CompForward; 
				CompExpr(out cexp);
				exp = new ElaBinary(t) { Left = GetFun(exp), Operator = op, Right = GetFun(cexp) }; 
			} else {
				Get();
				op = ElaBinaryOperator.CompBackward; 
				CompExpr(out cexp);
				exp = new ElaBinary(t) { Right = GetFun(exp), Operator = op, Left = GetFun(cexp) }; 
			}
		}
	}

	void ConsExpr(out ElaExpression exp) {
		PipeExpr(out exp);
		while (la.kind == 59) {
			var cexp = default(ElaExpression); 
			var op = default(ElaBinaryOperator);
			
			Get();
			op = ElaBinaryOperator.ConsList; 
			ConsExpr(out cexp);
			exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
		}
	}

	void PipeExpr(out ElaExpression exp) {
		BitOrExpr(out exp);
		while (la.kind == 92 || la.kind == 93) {
			var cexp = default(ElaExpression); 
			var op = default(ElaBinaryOperator);
			
			if (la.kind == 92) {
				Get();
				op = ElaBinaryOperator.PipeForward; 
				BitOrExpr(out cexp);
				exp = new ElaBinary(t) { Left = GetFun(exp), Operator = op, Right = GetFun(cexp) }; 
			} else {
				Get();
				op = ElaBinaryOperator.PipeBackward; 
				PipeExpr(out cexp);
				exp = new ElaBinary(t) { Right = GetFun(exp), Operator = op, Left = GetFun(cexp) }; 
			}
		}
	}

	void BitOrExpr(out ElaExpression exp) {
		BitXorExpr(out exp);
		while (la.kind == 94) {
			var cexp = default(ElaExpression); 
			Get();
			BitXorExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaBinaryOperator.BitwiseOr, Right = cexp }; 
			
		}
	}

	void BitXorExpr(out ElaExpression exp) {
		BitAndExpr(out exp);
		while (la.kind == 95) {
			var cexp = default(ElaExpression); 
			Get();
			BitAndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaBinaryOperator.BitwiseXor, Right = cexp }; 
			
		}
	}

	void BitAndExpr(out ElaExpression exp) {
		EqExpr(out exp);
		while (la.kind == 66) {
			var cexp = default(ElaExpression); 
			Get();
			EqExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaBinaryOperator.BitwiseAnd, Right = cexp }; 
			
		}
	}

	void EqExpr(out ElaExpression exp) {
		RelExpr(out exp);
		while (la.kind == 64 || la.kind == 65) {
			var cexp = default(ElaExpression); 
			var op = default(ElaBinaryOperator);
			
			if (la.kind == 64) {
				Get();
				op = ElaBinaryOperator.Equals; 
			} else {
				Get();
				op = ElaBinaryOperator.NotEquals; 
			}
			RelExpr(out cexp);
			exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
		}
	}

	void RelExpr(out ElaExpression exp) {
		ShiftExpr(out exp);
		while (StartOf(14)) {
			var cexp = default(ElaExpression); 
			var op = default(ElaBinaryOperator);
			
			if (la.kind == 60) {
				Get();
				op = ElaBinaryOperator.Greater; 
			} else if (la.kind == 61) {
				Get();
				op = ElaBinaryOperator.Lesser; 
			} else if (la.kind == 62) {
				Get();
				op = ElaBinaryOperator.GreaterEqual; 
			} else {
				Get();
				op = ElaBinaryOperator.LesserEqual; 
			}
			ShiftExpr(out cexp);
			exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void ShiftExpr(out ElaExpression exp) {
		AddExpr(out exp);
		while (la.kind == 96 || la.kind == 97) {
			var cexp = default(ElaExpression); 
			var op = default(ElaBinaryOperator);
			
			if (la.kind == 96) {
				Get();
				op = ElaBinaryOperator.ShiftRight; 
			} else {
				Get();
				op = ElaBinaryOperator.ShiftLeft; 
			}
			AddExpr(out cexp);
			exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void AddExpr(out ElaExpression exp) {
		MulExpr(out exp);
		while (la.kind == 85 || la.kind == 86) {
			var cexp = default(ElaExpression); 
			var op = default(ElaBinaryOperator);
			
			if (la.kind == 86) {
				Get();
				op = ElaBinaryOperator.Add; 
			} else {
				Get();
				op = ElaBinaryOperator.Subtract; 
			}
			MulExpr(out cexp);
			exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void MulExpr(out ElaExpression exp) {
		CastExpr(out exp);
		while (StartOf(15)) {
			var cexp = default(ElaExpression); 
			var op = default(ElaBinaryOperator);
			
			if (la.kind == 98) {
				Get();
				op = ElaBinaryOperator.Multiply; 
			} else if (la.kind == 99) {
				Get();
				op = ElaBinaryOperator.Divide; 
			} else if (la.kind == 100) {
				Get();
				op = ElaBinaryOperator.Modulus; 
			} else {
				Get();
				op = ElaBinaryOperator.Power; 
			}
			CastExpr(out cexp);
			exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void CastExpr(out ElaExpression exp) {
		CustomOpExpr(out exp);
		while (la.kind == 102) {
			var typ = ObjectType.None; 
			Get();
			Expect(1);
			typ = GetType(t.val); 
			exp = new ElaCast(t) { 
			CastAffinity = typ, Expression = exp }; 
			
		}
	}

	void CustomOpExpr(out ElaExpression exp) {
		UnaryExpr(out exp);
		while (la.kind == 6) {
			var cexp = default(ElaExpression); 
			var op = default(ElaBinaryOperator);
			var name = String.Empty;
			
			Get();
			op = ElaBinaryOperator.Custom; name = t.val; 
			UnaryExpr(out cexp);
			exp = new ElaBinary(t) { CustomOperator = name, Left = GetFun(exp), Operator = op, Right = GetFun(cexp) }; 
		}
	}

	void UnaryExpr(out ElaExpression exp) {
		exp = null; 
		if (StartOf(16)) {
			Literal(out exp);
			if (la.kind == 88 || la.kind == 89) {
				var op = default(ElaUnaryOperator); 
				PostUnaryOp(out op);
				exp = new ElaUnary(t) { Operator = op, Expression = exp }; 
			}
		} else if (StartOf(17)) {
			var op = default(ElaUnaryOperator); 
			UnaryOp(out op);
			var name = t.val; 
			var nexp = default(ElaExpression); 
			
			if (StartOf(16)) {
				Literal(out nexp);
			} else if (la.kind == 49) {
				Get();
				Expr(out nexp);
				Expect(50);
			} else SynErr(135);
			exp = new ElaUnary(t)
			{ 
				Operator = op,
				Expression = nexp,
				CustomOperator = op == ElaUnaryOperator.Custom ? name : null
			};
			
		} else if (la.kind == 88 || la.kind == 89) {
			var op = default(ElaUnaryOperator); 
			SpecUnaryOp(out op);
			var nexp = default(ElaExpression); 
			Literal(out nexp);
			exp = new ElaUnary(t) { Operator = op, Expression = nexp }; 
		} else if (la.kind == 49) {
			Get();
			if (la.kind == 50) {
				Get();
				exp = new ElaTupleLiteral(t, true); 
			} else if (StartOf(2)) {
				Expr(out exp);
				if (la.kind == 50) {
					Get();
					while (la.kind == 49 || la.kind == 53) {
						if (la.kind == 53) {
							Indexer(exp, out exp);
						} else {
							Invoke(exp, out exp);
						}
					}
					if (la.kind == 51) {
						MemberAccess(exp, out exp);
					}
				} else if (la.kind == 68) {
					Get();
					ObjectExpr(exp, out exp);
					while (la.kind == 49 || la.kind == 53) {
						if (la.kind == 53) {
							Indexer(exp, out exp);
						} else {
							Invoke(exp, out exp);
						}
					}
					if (la.kind == 51) {
						MemberAccess(exp, out exp);
					}
				} else if (la.kind == 52) {
					Get();
					TupleExpr(ref exp);
					Expect(50);
					while (la.kind == 49 || la.kind == 53) {
						if (la.kind == 53) {
							Indexer(exp, out exp);
						} else {
							Invoke(exp, out exp);
						}
					}
					if (la.kind == 51) {
						MemberAccess(exp, out exp);
					}
				} else SynErr(136);
			} else SynErr(137);
		} else SynErr(138);
	}

	void ObjectExpr(ElaExpression fl, out ElaExpression exp) {
		var obj = new ElaRecordLiteral(t); 
		exp = obj;
		var cexp = default(ElaExpression);
		var newFl = new ElaFieldDeclaration(t);
		
		if (fl.Type != ElaNodeType.VariableReference)
			AddError(ElaParserError.InvalidRecordFieldDeclaration);
		else
		{
			newFl.FieldName = ((ElaVariableReference)fl).VariableName;
			obj.Fields.Add(newFl);
		}
		
		Expr(out cexp);
		newFl.FieldValue = cexp; 
		if (la.kind == 52) {
			Get();
			FieldExpr(obj.Fields);
		}
		Expect(50);
	}

	void FieldExpr(List<ElaFieldDeclaration> list ) {
		var cexp = default(ElaExpression); 
		Expect(1);
		var fl = new ElaFieldDeclaration(t) { FieldName = t.val }; 
		list.Add(fl);
		
		Expect(68);
		Expr(out cexp);
		fl.FieldValue = cexp; 
		if (la.kind == 52) {
			Get();
			FieldExpr(list);
		}
	}

	void FunMatchExpr(ElaFunctionLiteral mi) {
		var match = new ElaMatch(t);
		match.Expression = mi.Parameters;
		mi.Expression = match;
		
		if (mi.Parameters.Type == ElaNodeType.TupleLiteral &&
			((ElaTupleLiteral)mi.Parameters).Parameters.Count == 0)
			AddError(ElaParserError.ShortMatchWithoutParams);
		
		MatchEntry(match);
		while (la.kind == 16) {
			MatchEntry(match);
		}
	}

	void AsyncExpr(out ElaExpression exp) {
		Expect(45);
		var async = new ElaAsyncLiteral(t); 
		Expr(out exp);
		async.Expression = exp;
		exp = async;
		
	}

	void LazyExpr(out ElaExpression exp) {
		Expect(46);
		var lazy = new ElaLazyLiteral(t); 
		Expr(out exp);
		lazy.Expression = exp;
		exp = lazy;
		
	}

	void IgnoreExpr(out ElaExpression exp) {
		Expect(47);
		var ign = new ElaIgnore(t); 
		Expr(out exp);
		ign.Expression = exp;
		exp = ign;
		
	}

	void Ela() {
		InitializeCodeUnit(); 
		RootExpr(CodeUnit.Expressions);
		while (StartOf(10)) {
			RootExpr(CodeUnit.Expressions);
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Ela();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,x,x,x, x,x,T,x, T,x,x,T, x,T,T,T, T,x,x,T, x,T,x,x, x,x,x,x, x,x,T,T, x,x,x,T, x,x,x,x, x,x,T,x, x,x,x,x, x,x,T,T, T,T,T,T, T,x,T,T, T,T,T,T, T,T,T,T, T,x,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x},
		{x,T,T,T, T,T,T,T, x,T,x,x, T,x,x,x, x,T,T,T, x,x,T,T, x,x,x,x, T,T,T,T, T,T,T,x, T,T,T,T, T,T,x,T, T,T,T,T, T,T,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,T,x,x, x,T,x,x, T,T,T,T, T,T,T,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,T,T, T,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, T,T,x,x, T,x,x,x, x,T,T,T, x,x,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,T, T,T,x,T, T,T,T,T, T,T,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, T,T,T,T, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x},
		{x,T,T,T, T,T,x,T, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x}

	};
} // end Parser


internal sealed class Errors {
	private Parser parser;

	internal Errors(Parser parser)
	{
		this.parser = parser;
		ErrorList = new List<ElaMessage>();
	}
  
	internal void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "intTok expected"; break;
			case 3: s = "realTok expected"; break;
			case 4: s = "stringTok expected"; break;
			case 5: s = "charTok expected"; break;
			case 6: s = "operatorTok expected"; break;
			case 7: s = "variantTok expected"; break;
			case 8: s = "SEMI expected"; break;
			case 9: s = "LBRA expected"; break;
			case 10: s = "RBRA expected"; break;
			case 11: s = "MATCH_ARR expected"; break;
			case 12: s = "ARG expected"; break;
			case 13: s = "IN expected"; break;
			case 14: s = "FTO expected"; break;
			case 15: s = "DOWNTO expected"; break;
			case 16: s = "ON expected"; break;
			case 17: s = "BASE expected"; break;
			case 18: s = "MATCH expected"; break;
			case 19: s = "WHEN expected"; break;
			case 20: s = "AS expected"; break;
			case 21: s = "IS expected"; break;
			case 22: s = "VAR expected"; break;
			case 23: s = "LET expected"; break;
			case 24: s = "PRIVATE expected"; break;
			case 25: s = "OPEN expected"; break;
			case 26: s = "AT expected"; break;
			case 27: s = "WITH expected"; break;
			case 28: s = "COUT expected"; break;
			case 29: s = "TYPEOF expected"; break;
			case 30: s = "WHILE expected"; break;
			case 31: s = "UNTIL expected"; break;
			case 32: s = "DO expected"; break;
			case 33: s = "FOR expected"; break;
			case 34: s = "CIF expected"; break;
			case 35: s = "ELSE expected"; break;
			case 36: s = "THROW expected"; break;
			case 37: s = "YIELD expected"; break;
			case 38: s = "RETURN expected"; break;
			case 39: s = "BREAK expected"; break;
			case 40: s = "CONTINUE expected"; break;
			case 41: s = "TRY expected"; break;
			case 42: s = "CATCH expected"; break;
			case 43: s = "TRUE expected"; break;
			case 44: s = "FALSE expected"; break;
			case 45: s = "ASYNC expected"; break;
			case 46: s = "LAZY expected"; break;
			case 47: s = "IGNOR expected"; break;
			case 48: s = "\"_\" expected"; break;
			case 49: s = "\"(\" expected"; break;
			case 50: s = "\")\" expected"; break;
			case 51: s = "\".\" expected"; break;
			case 52: s = "\",\" expected"; break;
			case 53: s = "\"[\" expected"; break;
			case 54: s = "\"]\" expected"; break;
			case 55: s = "\"||\" expected"; break;
			case 56: s = "\"&&\" expected"; break;
			case 57: s = "\"[|\" expected"; break;
			case 58: s = "\"|]\" expected"; break;
			case 59: s = "\"::\" expected"; break;
			case 60: s = "\">\" expected"; break;
			case 61: s = "\"<\" expected"; break;
			case 62: s = "\">=\" expected"; break;
			case 63: s = "\"<=\" expected"; break;
			case 64: s = "\"==\" expected"; break;
			case 65: s = "\"!=\" expected"; break;
			case 66: s = "\"&\" expected"; break;
			case 67: s = "\"=\" expected"; break;
			case 68: s = "\":\" expected"; break;
			case 69: s = "\"`\" expected"; break;
			case 70: s = "\"+=\" expected"; break;
			case 71: s = "\"-=\" expected"; break;
			case 72: s = "\"*=\" expected"; break;
			case 73: s = "\"/=\" expected"; break;
			case 74: s = "\"%=\" expected"; break;
			case 75: s = "\"|=\" expected"; break;
			case 76: s = "\"&=\" expected"; break;
			case 77: s = "\"^=\" expected"; break;
			case 78: s = "\">>=\" expected"; break;
			case 79: s = "\"<<=\" expected"; break;
			case 80: s = "\"::=\" expected"; break;
			case 81: s = "\"<=>\" expected"; break;
			case 82: s = "\"@\" expected"; break;
			case 83: s = "\"~\" expected"; break;
			case 84: s = "\"unless\" expected"; break;
			case 85: s = "\"-\" expected"; break;
			case 86: s = "\"+\" expected"; break;
			case 87: s = "\"!\" expected"; break;
			case 88: s = "\"--\" expected"; break;
			case 89: s = "\"++\" expected"; break;
			case 90: s = "\"<<<\" expected"; break;
			case 91: s = "\">>>\" expected"; break;
			case 92: s = "\"|>\" expected"; break;
			case 93: s = "\"<|\" expected"; break;
			case 94: s = "\"|\" expected"; break;
			case 95: s = "\"^\" expected"; break;
			case 96: s = "\">>\" expected"; break;
			case 97: s = "\"<<\" expected"; break;
			case 98: s = "\"*\" expected"; break;
			case 99: s = "\"/\" expected"; break;
			case 100: s = "\"%\" expected"; break;
			case 101: s = "\"**\" expected"; break;
			case 102: s = "\":>\" expected"; break;
			case 103: s = "??? expected"; break;
			case 104: s = "invalid Literal"; break;
			case 105: s = "invalid Literal"; break;
			case 106: s = "invalid Primitive"; break;
			case 107: s = "invalid Expr"; break;
			case 108: s = "invalid SinglePattern"; break;
			case 109: s = "invalid SinglePattern"; break;
			case 110: s = "invalid SinglePattern"; break;
			case 111: s = "invalid LiteralPattern"; break;
			case 112: s = "invalid BoolPattern"; break;
			case 113: s = "invalid BoolPattern"; break;
			case 114: s = "invalid BindingPattern"; break;
			case 115: s = "invalid ForeachPattern"; break;
			case 116: s = "invalid ForeachPattern"; break;
			case 117: s = "invalid IsOperatorPattern"; break;
			case 118: s = "invalid IsOperatorPattern"; break;
			case 119: s = "invalid RootExpr"; break;
			case 120: s = "invalid RootExpr"; break;
			case 121: s = "invalid IterExpr"; break;
			case 122: s = "invalid Assignment"; break;
			case 123: s = "invalid VariableDeclaration"; break;
			case 124: s = "invalid VariableDeclarationBody"; break;
			case 125: s = "invalid FunExpr"; break;
			case 126: s = "invalid WhenExpr"; break;
			case 127: s = "invalid IncludeStat"; break;
			case 128: s = "invalid WhileExpr"; break;
			case 129: s = "invalid DoWhileExpr"; break;
			case 130: s = "invalid ForExpr"; break;
			case 131: s = "invalid TryCatchExpr"; break;
			case 132: s = "invalid UnaryOp"; break;
			case 133: s = "invalid SpecUnaryOp"; break;
			case 134: s = "invalid PostUnaryOp"; break;
			case 135: s = "invalid UnaryExpr"; break;
			case 136: s = "invalid UnaryExpr"; break;
			case 137: s = "invalid UnaryExpr"; break;
			case 138: s = "invalid UnaryExpr"; break;

			default: s = "error " + n; break;
		}
		
		parser.errorCount++;
		ErrorList.Add(ErrorReporter.CreateMessage(n, s, line, col));
	}
	
	
	internal void AddErr (int line, int col, ElaParserError err, params object[] args) {
		parser.errorCount++;
		var str = Strings.GetMessage(err, args);		
		ErrorList.Add(new ElaMessage(str, MessageType.Error, (Int32)err, line, col));
	}

	/*internal void SemErr (int line, int col, string s) {
		parser.errorCount++;
		ErrorList.Add(new ElaMessage(s, MessageType.Error, (Int32)ElaParserError.InvalidSyntax, line, col));
	}
	
	internal void SemErr (string s) {
		parser.errorCount++;
		ErrorList.Add(new ElaMessage(s, MessageType.Error, (Int32)ElaParserError.InvalidSyntax, 0, 0));
	}*/
	
	public List<ElaMessage> ErrorList { get; private set; }
} // Errors


internal class FatalError: ElaFatalException {
	public FatalError(string m): base(m) {}
}

}

