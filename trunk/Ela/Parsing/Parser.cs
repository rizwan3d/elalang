
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
	public const int _variantTok = 2;
	public const int _argIdent = 3;
	public const int _funcTok = 4;
	public const int _intTok = 5;
	public const int _realTok = 6;
	public const int _stringTok = 7;
	public const int _charTok = 8;
	public const int _operatorTok = 9;
	public const int _SEMI = 10;
	public const int _LBRA = 11;
	public const int _RBRA = 12;
	public const int _LILB = 13;
	public const int _LIRB = 14;
	public const int _ARLB = 15;
	public const int _ARRB = 16;
	public const int _ARROW = 17;
	public const int _LAMBDA = 18;
	public const int _EQ = 19;
	public const int _SEQ = 20;
	public const int _MINUS = 21;
	public const int _COMPH = 22;
	public const int _COMPO = 23;
	public const int _DOT = 24;
	public const int _IN = 25;
	public const int _UPTO = 26;
	public const int _DOWNTO = 27;
	public const int _BASE = 28;
	public const int _MATCH = 29;
	public const int _ASAMP = 30;
	public const int _IS = 31;
	public const int _LET = 32;
	public const int _PRIVATE = 33;
	public const int _OPEN = 34;
	public const int _WITH = 35;
	public const int _WHILE = 36;
	public const int _DO = 37;
	public const int _FOR = 38;
	public const int _IFS = 39;
	public const int _ELSE = 40;
	public const int _THEN = 41;
	public const int _RAISE = 42;
	public const int _RETURN = 43;
	public const int _BREAK = 44;
	public const int _CONTINUE = 45;
	public const int _TRY = 46;
	public const int _TRUE = 47;
	public const int _FALSE = 48;
	public const int _FAIL = 49;
	public const int _WHERE = 50;
	public const int _MUTABLE = 51;
	public const int _AND = 52;
	public const int _ENDS = 53;
	public const int maxT = 93;

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
		case 5: case 6: case 7: case 8: case 47: case 48: {
			Primitive(out exp);
			break;
		}
		case 11: {
			RecordLiteral(out exp);
			break;
		}
		case 13: {
			ListLiteral(out exp);
			break;
		}
		case 15: {
			ArrayLiteral(out exp);
			break;
		}
		case 55: {
			TupleLiteral(out exp);
			break;
		}
		case 1: case 3: case 28: case 29: case 46: case 54: case 92: {
			SimpleExpr(out exp);
			break;
		}
		default: SynErr(94); break;
		}
	}

	void Primitive(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 5: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseInt(t.val) };	
			break;
		}
		case 6: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseReal(t.val) }; 
			break;
		}
		case 7: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseString(t.val) }; 
			break;
		}
		case 8: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseChar(t.val) }; 
			break;
		}
		case 47: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(true) }; 
			break;
		}
		case 48: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(false) }; 
			break;
		}
		default: SynErr(95); break;
		}
	}

	void RecordLiteral(out ElaExpression exp) {
		exp = null; 
		var fld = default(ElaFieldDeclaration);
		
		Expect(11);
		var rec = new ElaRecordLiteral(t); exp = rec; 
		RecordField(out fld);
		rec.Fields.Add(fld); 
		while (la.kind == 58) {
			Get();
			RecordField(out fld);
			rec.Fields.Add(fld); 
		}
		Expect(12);
	}

	void ListLiteral(out ElaExpression exp) {
		Expect(13);
		var list = default(List<ElaExpression>);
		var comp = default(ElaComprehension);
		var rng = default(ElaRange);
		var ot = t;
		
		exp = null;
		
		if (StartOf(1)) {
			if (StartOf(2)) {
				ParamList(out list, out comp, out rng);
				if (list != null)
				{
					var listExp = new ElaListLiteral(ot) { Values = list };
					exp = listExp;	
				}
				else if (comp != null)
				{
					comp.Initial = new ElaListLiteral(ot);
					exp = comp;
				}
				else if (rng != null)
				{
					rng.Initial = new ElaListLiteral(ot);
					exp = rng;
				}
				
			} else {
				var cexp = default(ElaExpression); 
				Get();
				Expr(out cexp);
				ComprehensionExpr(cexp, out comp);
				comp.Lazy = true;
				comp.Initial = new ElaListLiteral(ot);
				exp = comp;
				
			}
		}
		if (exp == null)
		exp = new ElaListLiteral(ot);
		
		Expect(14);
	}

	void ArrayLiteral(out ElaExpression exp) {
		Expect(15);
		var list = default(List<ElaExpression>);
		var comp = default(ElaComprehension);
		var rng = default(ElaRange);
		var ot = t;
		
		exp = null;
		
		if (StartOf(2)) {
			ParamList(out list, out comp, out rng);
			if (list != null)
			{
				var arrExp = new ElaArrayLiteral(ot) { Values = list };
				exp = arrExp;	
			}
			else if (comp != null)
			{
				comp.Initial = new ElaArrayLiteral(ot);
				exp = comp;
			}
			else if (rng != null)
			{
				rng.Initial = new ElaArrayLiteral(ot);
				exp = rng;
			}
			
		}
		if (exp == null)
		exp = new ElaArrayLiteral(ot);
		
		Expect(16);
	}

	void TupleLiteral(out ElaExpression exp) {
		var ot = default(Token);
		exp = null; 
		
		Expect(55);
		ot = t; 
		if (StartOf(3)) {
			GroupExpr(out exp);
		}
		Expect(56);
		if (exp == null)
		exp = new ElaUnitLiteral(ot);
		
	}

	void SimpleExpr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 3: {
			ArgumentReference(out exp);
			break;
		}
		case 28: {
			BaseReference(out exp);
			break;
		}
		case 1: case 54: {
			VariableReference(out exp);
			break;
		}
		case 29: {
			MatchExpr(out exp);
			break;
		}
		case 46: {
			TryExpr(out exp);
			break;
		}
		case 92: {
			LazyExpr(out exp);
			break;
		}
		default: SynErr(96); break;
		}
	}

	void ArgumentReference(out ElaExpression exp) {
		Expect(3);
		exp = new ElaArgument(t) { ArgumentName = t.val.Substring(1) }; 
	}

	void BaseReference(out ElaExpression exp) {
		var baseRef = default(ElaBaseReference);
		exp = null;
		
		Expect(28);
		baseRef = new ElaBaseReference(t); 
		Expect(24);
		var ot = t; 
		Expect(1);
		exp = new ElaFieldReference(ot) { FieldName = t.val, TargetObject = baseRef }; 
	}

	void VariableReference(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = GetBuiltin(t, t.val); 
		} else if (la.kind == 54) {
			Get();
			exp = new ElaPlaceholder(t); 
		} else SynErr(97);
	}

	void MatchExpr(out ElaExpression exp) {
		Expect(29);
		var match = new ElaMatch(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(35);
		MatchEntry(match);
		while (la.kind == 10) {
			Get();
			if (StartOf(4)) {
				MatchEntry(match);
			}
		}
		Expect(53);
	}

	void TryExpr(out ElaExpression exp) {
		Expect(46);
		var ot = t;
		var match = new ElaTry(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(35);
		MatchEntry(match);
		while (la.kind == 10) {
			Get();
			if (StartOf(4)) {
				MatchEntry(match);
			}
		}
		Expect(53);
	}

	void LazyExpr(out ElaExpression exp) {
		Expect(92);
		var lazy = new ElaLazyLiteral(t); 
		Expr(out exp);
		var m = new ElaMatch(t);
		m.Entries.Add(new ElaMatchEntry { Pattern = new ElaUnitPattern(), Expression = exp });
		lazy.Body = m;
		exp = lazy;
		
		Expect(56);
	}

	void VariantLiteral(out ElaExpression exp) {
		exp = null; 
		var vl = default(ElaVariantLiteral);
		
		Expect(2);
		vl = new ElaVariantLiteral(t) { Tag = t.val.Substring(1) }; 
		if (StartOf(5)) {
			Literal(out exp);
			vl.Expression = exp; 
		}
		exp = vl; 
	}

	void GroupExpr(out ElaExpression exp) {
		exp = null; 
		var cexp = default(ElaExpression);
		var pat = default(ElaPattern); 
		var ot = t;
		
		switch (la.kind) {
		case 4: case 9: case 21: case 62: case 70: case 71: case 72: case 73: case 74: case 75: case 76: case 77: case 78: case 79: case 80: case 81: case 82: case 83: case 84: case 85: case 86: case 87: case 88: case 89: case 90: {
			OperatorExpr(out exp);
			break;
		}
		case 24: {
			MemberAccess(hiddenVar, out exp);
			exp = GetPartialFun(exp); 
			break;
		}
		case 31: {
			Get();
			ot = t; 
			IsOperatorPattern(out pat);
			exp = GetPartialFun(new ElaIs(ot) { Expression = hiddenVar, Pattern = pat }); 
			break;
		}
		case 22: {
			Get();
			ComprehensionOpExpr(hiddenVar, out exp);
			exp = GetPartialFun(exp); 
			break;
		}
		case 57: {
			Get();
			ot = t; 
			Expect(1);
			exp = GetPartialFun(new ElaCast(ot) { CastAffinity = GetType(t.val), Expression = hiddenVar });
			break;
		}
		case 1: case 2: case 3: case 5: case 6: case 7: case 8: case 11: case 13: case 15: case 18: case 28: case 29: case 32: case 36: case 38: case 39: case 42: case 43: case 44: case 45: case 46: case 47: case 48: case 49: case 54: case 55: case 63: case 91: case 92: {
			Expr(out exp);
			if (la.kind == 58) {
				var tuple = new ElaTupleLiteral(ot); 
				tuple.Parameters.Add(exp);
				exp = tuple; 
				
				Get();
				if (StartOf(2)) {
					Expr(out cexp);
					tuple.Parameters.Add(cexp); 
				}
				while (la.kind == 58) {
					Get();
					Expr(out cexp);
					tuple.Parameters.Add(cexp); 
				}
			}
			break;
		}
		default: SynErr(98); break;
		}
	}

	void OperatorExpr(out ElaExpression exp) {
		exp = null; 
		var ot = t;
		var op = ElaOperator.None;
		var opName = String.Empty;
		var funName = String.Empty;
		
		if (StartOf(6)) {
			FuncOperator(out op);
		} else if (la.kind == 9) {
			Get();
			opName = t.val; 
		} else if (la.kind == 4) {
			Get();
			funName = t.val.Trim('`'); 
		} else SynErr(99);
		if (StartOf(5)) {
			var nexp = default(ElaExpression);  
			Literal(out nexp);
			exp = 
			funName.Length != 0 ? GetPrefixFun(funName, nexp, true) :
			opName.Length != 0 ? GetCustomOperatorFun(opName, nexp) :
			GetOperatorFun(nexp, op, true);
			
		}
		if (exp == null)
		{
			if (opName.Length == 0 && funName.Length == 0)
				exp = new ElaBuiltinFunction(ot) 
				{ 
					Kind = ElaBuiltinFunctionKind.Operator,
					Operator = op
				};
			else 
				exp = new ElaCustomOperator(ot) { Operator = opName };
		}
		
	}

	void MemberAccess(ElaExpression target, out ElaExpression exp) {
		exp = null; 
		Expect(24);
		if (la.kind == 13) {
			Get();
			var indExp = new ElaIndexer(t) { TargetObject = target };
			exp = indExp;
			
			var cexp = default(ElaExpression); 
			Expr(out cexp);
			indExp.Index = cexp;	
			Expect(14);
		} else if (la.kind == 1) {
			Get();
			exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = target }; 
		} else SynErr(100);
		if (la.kind == 24) {
			MemberAccess(exp, out exp);
		}
	}

	void IsOperatorPattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 61: {
			TypeCheckPattern(out pat);
			break;
		}
		case 5: case 6: case 7: case 8: case 47: case 48: case 63: {
			LiteralPattern(out pat);
			break;
		}
		case 11: {
			RecordPattern(out pat);
			break;
		}
		case 13: {
			ListPattern(out pat);
			break;
		}
		case 15: {
			ArrayPattern(out pat);
			break;
		}
		case 55: {
			UnitPattern(out pat);
			break;
		}
		default: SynErr(101); break;
		}
	}

	void ComprehensionOpExpr(ElaExpression init, out ElaExpression exp) {
		var list = default(List<ElaExpression>);
		var comp = default(ElaComprehension);
		var rng = default(ElaRange);
		exp = null;
		
		Expect(13);
		ParamList(out list, out comp, out rng);
		if (rng != null) {
		rng.Initial = init;
		exp = rng;
		}
		else if (comp != null) {
			comp.Initial = init;
			exp = comp;
		}
		else {
			AddError(ElaParserError.ComprehensionOpInvalidOperand);
			exp = new ElaRange();
		}
		
		Expect(14);
	}

	void Expr(out ElaExpression exp) {
		exp = null; 
		if (StartOf(7)) {
			EmbExpr(out exp);
		} else if (la.kind == 32) {
			LetBinding(out exp);
		} else SynErr(102);
	}

	void MatchEntry(ElaMatch match) {
		var cexp = default(ElaExpression); 
		var pat = default(ElaPattern); 
		var ot = t; 
		
		if (StartOf(8)) {
			RootPattern(out pat);
		}
		var entry = new ElaMatchEntry(ot);
		entry.Pattern = pat;				
		match.Entries.Add(entry);
		
		if (la.kind == 59) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(19);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 50) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
	}

	void RootPattern(out ElaPattern pat) {
		OrPattern(out pat);
		if (la.kind == 62) {
			ConsPattern(pat, out pat);
		}
	}

	void Guard(out ElaExpression exp) {
		exp = null; 
		Expect(59);
		if (StartOf(9)) {
			BinaryExpr(out exp);
			while (la.kind == 58) {
				var old = exp; 
				Get();
				var ot = t; 
				BinaryExpr(out exp);
				exp = new ElaBinary(t) { Operator = ElaOperator.BooleanAnd, Left = old, Right = exp };
				
			}
		} else if (la.kind == 40) {
			Get();
			exp = new ElaOtherwiseGuard(t); 
		} else SynErr(103);
	}

	void WhereBinding(out ElaExpression exp) {
		Expect(50);
		WhereBindingBody(out exp);
		Expect(53);
	}

	void BinaryExpr(out ElaExpression exp) {
		AssignExpr(out exp);
		while (la.kind == 20) {
			var cexp = default(ElaExpression); 
			Get();
			var ot = t; 
			AssignExpr(out cexp);
			exp = new ElaBinary(ot) { Left = exp, Right = cexp, Operator = ElaOperator.Sequence };
			
		}
	}

	void OrPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 2 || la.kind == 60) {
			VariantPattern(out pat);
		} else if (StartOf(10)) {
			AsPattern(out pat);
		} else SynErr(104);
	}

	void ConsPattern(ElaPattern prev, out ElaPattern exp) {
		var cexp = default(ElaPattern); 
		var ht = new ElaHeadTailPattern(t); 
		ht.Patterns.Add(prev);
		exp = ht;				
		
		Expect(62);
		AsPattern(out cexp);
		ht.Patterns.Add(cexp); 
		while (la.kind == 62) {
			Get();
			AsPattern(out cexp);
			ht.Patterns.Add(cexp); 
		}
	}

	void ParenPattern(out ElaPattern pat) {
		pat = null; 
		OrPattern(out pat);
		if (la.kind == 62) {
			ConsPattern(pat, out pat);
		}
		if (la.kind == 58) {
			TuplePattern(pat, out pat);
		}
	}

	void TuplePattern(ElaPattern prev, out ElaPattern pat) {
		var seq = new ElaTuplePattern(t); 
		seq.Patterns.Add(prev);
		var cpat = default(ElaPattern);
		pat = seq;
		
		Expect(58);
		if (StartOf(10)) {
			AsPattern(out cpat);
			if (la.kind == 62) {
				ConsPattern(cpat, out cpat);
			}
			seq.Patterns.Add(cpat); 
			while (la.kind == 58) {
				Get();
				AsPattern(out cpat);
				if (la.kind == 62) {
					ConsPattern(cpat, out cpat);
				}
				seq.Patterns.Add(cpat); 
			}
		}
	}

	void VariantPattern(out ElaPattern pat) {
		pat = null; 
		var vp = default(ElaVariantPattern);
		var cpat = default(ElaPattern);
		
		if (la.kind == 2) {
			Get();
			vp = new ElaVariantPattern(t) { Tag = t.val.Substring(1) }; 
			if (StartOf(10)) {
				AsPattern(out cpat);
				vp.Pattern = cpat; 
			}
			pat = vp; 
		} else if (la.kind == 60) {
			Get();
			vp = new ElaVariantPattern(t); 
			if (StartOf(10)) {
				AsPattern(out cpat);
				vp.Pattern = cpat; 
			}
			pat = vp; 
		} else SynErr(105);
	}

	void AsPattern(out ElaPattern pat) {
		pat = null; 
		SinglePattern(out pat);
		if (la.kind == 30) {
			Get();
			var asPat = new ElaAsPattern(t) { Pattern = pat }; 
			pat = asPat;				
			
			Expect(1);
			asPat.Name = t.val; 
		}
	}

	void FuncPattern(out ElaPattern pat) {
		AsPattern(out pat);
		if (la.kind == 62) {
			ConsPattern(pat, out pat);
		}
	}

	void SinglePattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 54: {
			DefaultPattern(out pat);
			break;
		}
		case 55: {
			UnitPattern(out pat);
			break;
		}
		case 5: case 6: case 7: case 8: case 47: case 48: case 63: {
			LiteralPattern(out pat);
			break;
		}
		case 13: {
			ListPattern(out pat);
			break;
		}
		case 15: {
			ArrayPattern(out pat);
			break;
		}
		case 11: {
			RecordPattern(out pat);
			break;
		}
		case 1: {
			IdentPattern(out pat);
			break;
		}
		case 61: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(106); break;
		}
	}

	void DefaultPattern(out ElaPattern pat) {
		Expect(54);
		pat = new ElaDefaultPattern(t); 
	}

	void UnitPattern(out ElaPattern pat) {
		var ot = t;
		pat = null;
		
		Expect(55);
		if (StartOf(8)) {
			ParenPattern(out pat);
		}
		Expect(56);
		if (pat == null)
		pat = new ElaUnitPattern(ot); 
		
	}

	void LiteralPattern(out ElaPattern pat) {
		var val = new ElaLiteralPattern(t); 
		pat = val;
		
		switch (la.kind) {
		case 7: {
			Get();
			val.Value = ParseString(t.val); 
			break;
		}
		case 8: {
			Get();
			val.Value = ParseChar(t.val); 
			break;
		}
		case 5: {
			Get();
			val.Value = ParseInt(t.val); 
			break;
		}
		case 6: {
			Get();
			val.Value = ParseReal(t.val); 
			break;
		}
		case 63: {
			Get();
			if (la.kind == 5) {
				Get();
				val.Value = ParseInt(t.val).MakeNegative(); 
			} else if (la.kind == 6) {
				Get();
				val.Value = ParseReal(t.val).MakeNegative(); 
			} else SynErr(107);
			break;
		}
		case 47: {
			Get();
			val.Value = new ElaLiteralValue(true); 
			break;
		}
		case 48: {
			Get();
			val.Value = new ElaLiteralValue(false); 
			break;
		}
		default: SynErr(108); break;
		}
	}

	void ListPattern(out ElaPattern pat) {
		var cexp = default(ElaPattern); 
		var ht = default(ElaHeadTailPattern); 
		pat = null;
		
		Expect(13);
		if (StartOf(10)) {
			ht = new ElaHeadTailPattern(t);			
			
			AsPattern(out cexp);
			ht.Patterns.Add(cexp);  
			while (la.kind == 58) {
				Get();
				AsPattern(out cexp);
				ht.Patterns.Add(cexp); 
			}
			ht.Patterns.Add(new ElaNilPattern(t));
			pat = ht;
			
		}
		if (pat == null)
		pat = new ElaNilPattern(t);
		
		Expect(14);
	}

	void ArrayPattern(out ElaPattern pat) {
		var seq = new ElaArrayPattern(t); 
		var cpat = default(ElaPattern);
		pat = seq;
		
		Expect(15);
		AsPattern(out cpat);
		seq.Patterns.Add(cpat);  
		while (la.kind == 58) {
			Get();
			AsPattern(out cpat);
			seq.Patterns.Add(cpat); 
		}
		Expect(16);
	}

	void RecordPattern(out ElaPattern pat) {
		pat = null; 
		var cpat = default(ElaFieldPattern);
		
		Expect(11);
		var rec = new ElaRecordPattern(t); 
		pat = rec; 
		
		if (la.kind == 1 || la.kind == 7) {
			FieldPattern(out cpat);
			rec.Fields.Add(cpat); 
			while (la.kind == 58) {
				Get();
				FieldPattern(out cpat);
				rec.Fields.Add(cpat); 
			}
		}
		Expect(12);
	}

	void IdentPattern(out ElaPattern pat) {
		pat = null; 
		Expect(1);
		var name = t.val; 
		if (la.kind == 57) {
			Get();
			var cst = new ElaCastPattern(t) { VariableName = name }; pat = cst; 
			Expect(1);
			cst.TypeAffinity = GetType(t.val); 
		} else if (StartOf(11)) {
			pat = new ElaVariablePattern(t) { Name = t.val }; 
		} else SynErr(109);
	}

	void TypeCheckPattern(out ElaPattern pat) {
		var eis = new ElaIsPattern(t);
		var tmp = default(ElaTraits);
		pat = eis; 			
		
		Expect(61);
		if (la.kind == 1) {
			Get();
			eis.TypeAffinity = GetType(t.val); 
		} else if (la.kind == 55) {
			
			Get();
			Expect(1);
			tmp = Builtins.Trait(t.val); 
			
			if (tmp == ElaTraits.None) {
				AddError(ElaParserError.UnknownTrait, t.val);
			}
			
			eis.Traits |= tmp;
			
			while (la.kind == 58) {
				Get();
				Expect(1);
				tmp = Builtins.Trait(t.val); 
				
				if (tmp == ElaTraits.None) {
					AddError(ElaParserError.UnknownTrait, t.val);
				}
				
				eis.Traits |= tmp;
				
			}
			Expect(56);
		} else SynErr(110);
	}

	void BindingPattern(out string name, out ElaPattern pat) {
		pat = null; name = null; 
		switch (la.kind) {
		case 1: {
			Get();
			name = t.val; 
			break;
		}
		case 54: {
			DefaultPattern(out pat);
			break;
		}
		case 11: {
			RecordPattern(out pat);
			break;
		}
		case 13: {
			ListPattern(out pat);
			break;
		}
		case 15: {
			ArrayPattern(out pat);
			break;
		}
		case 55: {
			Get();
			ParenPattern(out pat);
			Expect(56);
			break;
		}
		default: SynErr(111); break;
		}
	}

	void ForeachPattern(out ElaPattern pat) {
		pat = null; string name = null; 
		switch (la.kind) {
		case 1: {
			Get();
			name = t.val; 
			pat = new ElaVariablePattern(t) { Name = name }; 
			break;
		}
		case 5: case 6: case 7: case 8: case 47: case 48: case 63: {
			LiteralPattern(out pat);
			break;
		}
		case 11: {
			RecordPattern(out pat);
			break;
		}
		case 13: {
			ListPattern(out pat);
			break;
		}
		case 15: {
			ArrayPattern(out pat);
			break;
		}
		case 55: {
			UnitPattern(out pat);
			break;
		}
		default: SynErr(112); break;
		}
	}

	void FieldPattern(out ElaFieldPattern fld) {
		fld = null;
		var cpat = default(ElaPattern);
		
		if (la.kind == 7) {
			Get();
			fld = new ElaFieldPattern(t) { Name = ReadString(t.val) }; 
			Expect(19);
			AsPattern(out cpat);
			fld.Value = cpat; 
		} else if (la.kind == 1) {
			Get();
			fld = new ElaFieldPattern(t) { Name = t.val }; 
			if (la.kind == 19) {
				Get();
				AsPattern(out cpat);
			}
			if (cpat == null)
			cpat = new ElaVariablePattern(t) { Name = fld.Name };
			
			fld.Value = cpat; 
			
		} else SynErr(113);
	}

	void RecordField(out ElaFieldDeclaration fld) {
		fld = null; 
		var cexp = default(ElaExpression);
		var mutable = false;
		
		if (la.kind == 51) {
			Get();
			mutable = true; 
		}
		if (la.kind == 1) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = t.val, Mutable = mutable }; 
			Expect(19);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else if (la.kind == 7) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = ReadString(t.val), Mutable = mutable }; 
			Expect(19);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else SynErr(114);
	}

	void RangeExpr(ElaExpression first, ElaExpression sec, out ElaRange rng) {
		rng = new ElaRange(t) { First = first, Second = sec };
		var cexp = default(ElaExpression);
		
		Expect(64);
		if (StartOf(2)) {
			Expr(out cexp);
			rng.Last = cexp; 
		}
	}

	void ParamList(out List<ElaExpression> list, out ElaComprehension comp, out ElaRange rng ) {
		var exp = default(ElaExpression); 
		list = null;
		comp = null;
		rng = null;
		
		Expr(out exp);
		if (la.kind == 23 || la.kind == 58 || la.kind == 64) {
			if (la.kind == 23) {
				ComprehensionExpr(exp, out comp);
			} else if (la.kind == 64) {
				RangeExpr(exp, null, out rng);
			} else {
				var oexp = exp; 
				Get();
				Expr(out exp);
				if (la.kind == 64) {
					RangeExpr(oexp, exp, out rng);
				} else if (la.kind == 14 || la.kind == 16 || la.kind == 58) {
					list = new List<ElaExpression>();
					list.Add(oexp);
					list.Add(exp);
					
					while (la.kind == 58) {
						Get();
						Expr(out exp);
						list.Add(exp); 
					}
				} else SynErr(115);
			}
		}
		if (list == null && comp == null && rng == null && exp != null)
		{
			list = new List<ElaExpression>();
			list.Add(exp);
		}
		
	}

	void ComprehensionExpr(ElaExpression sel, out ElaComprehension exp) {
		var it = default(ElaFor); 
		var ot = t;		
		
		Expect(23);
		ComprehensionEntry(sel, out it);
		exp = new ElaComprehension(ot) { Generator = it }; 
	}

	void LetBinding(out ElaExpression exp) {
		exp = null; 
		var flags = ElaVariableFlags.Immutable;
		var inExp = default(ElaExpression);
		
		Expect(32);
		if (la.kind == 33 || la.kind == 51) {
			VariableAttributes(out flags);
		}
		VariableDeclarationBody(flags, out exp);
		Expect(25);
		Expr(out inExp);
		((ElaBinding)exp).In = inExp; 
	}

	void VariableAttributes(out ElaVariableFlags flags) {
		flags = ElaVariableFlags.Immutable; 
		if (la.kind == 51) {
			Get();
			flags ^= ElaVariableFlags.Immutable; 
			if (la.kind == 33) {
				Get();
				flags |= ElaVariableFlags.Private; 
			}
		} else if (la.kind == 33) {
			Get();
			flags |= ElaVariableFlags.Private; 
			if (la.kind == 51) {
				Get();
				flags ^= ElaVariableFlags.Immutable; 
			}
		} else SynErr(116);
	}

	void VariableDeclarationBody(ElaVariableFlags flags, out ElaExpression exp) {
		var varExp = new ElaBinding(t) { VariableFlags = flags }; 
		exp = varExp;
		var cexp = default(ElaExpression); 
		
		if (StartOf(12)) {
			var pat = default(ElaPattern); string name = null; 
			BindingPattern(out name, out pat);
			if (name != null)
			varExp.VariableName = name;
			else
				varExp.Pattern = pat; 
			
			if (StartOf(13)) {
				if (StartOf(10)) {
					if (name == null) AddError(ElaParserError.InvalidFunctionDeclaration); 
					FunExpr(out cexp);
					varExp.InitExpression = cexp; 
				} else if (la.kind == 59) {
					var gexp = default(ElaExpression);
					var cexp3 = default(ElaExpression);
					var cond = new ElaCondition(t);
					varExp.InitExpression = cond;
					
					Get();
					BindingGuard(out gexp);
					cond.Condition = gexp; 
					Expect(19);
					Expr(out cexp3);
					cond.True = cexp3; 
					Expect(10);
					BindingGuardList(ref cond);
				} else {
					Get();
					Expr(out cexp);
					varExp.InitExpression = cexp; 
					if (la.kind == 50) {
						var cexp2 = default(ElaExpression); 
						WhereBinding(out cexp2);
						varExp.Where = (ElaBinding)cexp2; 
					}
				}
			}
			SetFunMetadata(varExp, cexp, flags); 
		} else if (la.kind == 9) {
			Get();
			varExp.VariableName = t.val; 
			FunExpr(out cexp);
			varExp.InitExpression = cexp; 
			((ElaFunctionLiteral)cexp).FunctionType = ElaFunctionType.Operator; 
			((ElaFunctionLiteral)cexp).Name = varExp.VariableName;
			
		} else SynErr(117);
		if (la.kind == 52) {
			Get();
			VariableDeclarationBody(flags, out exp);
			((ElaBinding)varExp).And = (ElaBinding)exp;
			exp = varExp;
			
		}
	}

	void RootLetBinding(out ElaExpression exp) {
		exp = null; 
		var flags = ElaVariableFlags.Immutable;
		var inExp = default(ElaExpression);
		
		Expect(32);
		if (la.kind == 33 || la.kind == 51) {
			VariableAttributes(out flags);
		}
		VariableDeclarationBody(flags, out exp);
		if (la.kind == 25) {
			Get();
			Expr(out inExp);
			((ElaBinding)exp).In = inExp; 
		}
	}

	void WhereBindingBody(out ElaExpression exp) {
		var varExp = new ElaBinding(t) { VariableFlags = ElaVariableFlags.Immutable }; 
		exp = varExp;
		var cexp = default(ElaExpression); 
		
		var pat = default(ElaPattern); string name = null; 
		BindingPattern(out name, out pat);
		if (name != null)
		varExp.VariableName = name;
		else
			varExp.Pattern = pat; 
		
		if (la.kind == 19) {
			Get();
			Expr(out cexp);
			varExp.InitExpression = cexp; 
		} else if (la.kind == 59) {
			var gexp = default(ElaExpression);
			var cexp3 = default(ElaExpression);
			var cond = new ElaCondition(t);
			varExp.InitExpression = cond;
			
			Get();
			BindingGuard(out gexp);
			cond.Condition = gexp; 
			Expect(19);
			Expr(out cexp3);
			cond.True = cexp3; 
			Expect(10);
			BindingGuardList(ref cond);
		} else if (StartOf(10)) {
			if (name == null) AddError(ElaParserError.InvalidFunctionDeclaration); 
			FunExpr(out cexp);
			varExp.InitExpression = cexp; 
		} else SynErr(118);
		SetFunMetadata(varExp, cexp, ElaVariableFlags.Immutable); 
		if (la.kind == 52) {
			var cd = default(ElaExpression); 
			Get();
			WhereBindingBody(out cd);
			varExp.And = (ElaBinding)cd; 
		}
	}

	void BindingGuard(out ElaExpression exp) {
		exp = null; 
		BinaryExpr(out exp);
		while (la.kind == 58) {
			var old = exp; 
			Get();
			var ot = t; 
			BinaryExpr(out exp);
			exp = new ElaBinary(t) { Operator = ElaOperator.BooleanAnd, Left = old, Right = exp };
			
		}
	}

	void BindingGuardList(ref ElaCondition cond) {
		var gexp = default(ElaExpression);
		var cexp = default(ElaExpression);
		
		Expect(59);
		if (StartOf(9)) {
			var newCond = new ElaCondition(t);
			cond.False = newCond;
			cond = newCond;
			
			BindingGuard(out gexp);
			cond.Condition = gexp; 
			Expect(19);
			Expr(out cexp);
			cond.True = cexp; 
			if (la.kind == 10) {
				Get();
				BindingGuardList(ref cond);
			}
		} else if (la.kind == 40) {
			Get();
			Expect(19);
			Expr(out cexp);
			cond.False = cexp; 
		} else SynErr(119);
	}

	void FunExpr(out ElaExpression exp) {
		var ot = t;
		var mi = new ElaFunctionLiteral(t);
		exp = mi;
		mi.Body = new ElaMatch(t);
		
		FunBodyExpr(mi.Body);
		if (mi.Body.Entries.Count > 1)
		{
			var patterns = default(List<ElaPattern>);
			var pars = 0;
		
			if (mi.Body.Entries[0].Pattern.Type != ElaNodeType.PatternGroup)
				pars = 1;
			else
			{
				patterns = ((ElaPatternGroup)mi.Body.Entries[0].Pattern).Patterns;
				pars = patterns.Count;
			}
						
			var tp = new ElaTupleLiteral(ot);
			
			for (var i = 0; i < pars; i++)
			{
				if (patterns != null && patterns[i].Type == ElaNodeType.VariablePattern)
				{
					tp.Parameters.Add(new ElaVariableReference(ot) {
						VariableName = ((ElaVariablePattern)patterns[i]).Name
					});
				}
				else
					tp.Parameters.Add(new ElaVariableReference(ot) { VariableName = "$" + i });
			}
			
			mi.Body.Expression = tp;
		}
		
	}

	void FunBodyExpr(ElaMatch match) {
		var ot = t;
		var pat = default(ElaPattern);
		var seq = default(ElaPatternGroup);
		var cexp = default(ElaExpression);
		
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		
		FuncPattern(out pat);
		entry.Pattern = pat; 
		while (StartOf(10)) {
			FuncPattern(out pat);
			if (seq == null)
			{
				seq = new ElaPatternGroup(ot);
				seq.Patterns.Add(entry.Pattern);
				entry.Pattern = seq;							
			}
			
				seq.Patterns.Add(pat); 
			
		}
		if (la.kind == 59) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(19);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 50) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (la.kind == 10) {
			Get();
			if (StartOf(13)) {
				ChildFunBodyExpr(match);
			}
		}
	}

	void ChildFunBodyExpr(ElaMatch match) {
		var ot = t;
		var pat = default(ElaPattern);
		var seq = default(ElaPatternGroup);
		var cexp = default(ElaExpression);
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		
		if (StartOf(10)) {
			FuncPattern(out pat);
			entry.Pattern = pat; 
			while (StartOf(10)) {
				FuncPattern(out pat);
				if (seq == null)
				{
					seq = new ElaPatternGroup(ot);
					seq.Patterns.Add(entry.Pattern);
					entry.Pattern = seq;							
				}
				
					seq.Patterns.Add(pat); 
				
			}
		}
		if (la.kind == 59) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(19);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 50) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (la.kind == 10) {
			Get();
			if (StartOf(13)) {
				ChildFunBodyExpr(match);
			}
		}
	}

	void LambdaBodyExpr(ElaMatch match) {
		var ot = t;
		var pat = default(ElaPattern);
		var seq = default(ElaPatternGroup);
		
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		
		FuncPattern(out pat);
		entry.Pattern = pat; 
		while (StartOf(10)) {
			FuncPattern(out pat);
			if (seq == null)
			{
				seq = new ElaPatternGroup(ot);
				seq.Patterns.Add(entry.Pattern);
				entry.Pattern = seq;							
			}
			
				seq.Patterns.Add(pat); 
			
		}
		if (la.kind == 59) {
			var cexp = default(ElaExpression); 
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(17);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
	}

	void IncludeStat(out ElaExpression exp) {
		exp = null; 
		Expect(34);
		var inc = new ElaModuleInclude(t); 
		Qualident(inc.Path);
		var name = inc.Path[inc.Path.Count - 1];				
		inc.Path.RemoveAt(inc.Path.Count - 1);				
		inc.Alias = inc.Name = name;
		exp = inc;
		
		if (la.kind == 13) {
			Get();
			if (la.kind == 1) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 7) {
				Get();
				inc.DllName = ReadString(t.val); 
			} else SynErr(120);
			Expect(14);
		}
		if (la.kind == 30) {
			Get();
			Expect(1);
			inc.Alias = t.val; 
		}
		if (la.kind == 1) {
			IncludeImports(inc);
		}
	}

	void Qualident(List<String> path ) {
		var val = String.Empty; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 7) {
			Get();
			val = ReadString(t.val); 
		} else SynErr(121);
		path.Add(val); 
		if (la.kind == 24) {
			Get();
			Qualident(path);
		}
	}

	void IncludeImports(ElaModuleInclude inc) {
		Expect(1);
		var imp = new ElaImportedName(t) { LocalName = t.val, ExternalName = t.val }; 
		inc.Imports.Add(imp); 
		
		if (la.kind == 19) {
			Get();
			Expect(1);
			imp.ExternalName = t.val; 
		}
		if (la.kind == 58) {
			Get();
			IncludeImports(inc);
		}
	}

	void WhileExpr(out ElaExpression exp) {
		Expect(36);
		var wex = new ElaWhile(t);
		exp = wex;
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		wex.Condition = cexp; 
		Expect(37);
		Expr(out cexp);
		wex.Body = cexp; 
	}

	void ForExpr(out ElaExpression exp) {
		Expect(38);
		var ot = t;
		var it = new ElaFor(t);
		exp = it;
		var cexp = default(ElaExpression);
		var pat = default(ElaPattern);
		
		ForeachPattern(out pat);
		if (la.kind == 19) {
			Get();
			BinaryExpr(out cexp);
			it.InitExpression = cexp; 
		}
		if (la.kind == 59) {
			Guard(out cexp);
			it.Guard = cexp; 
		}
		it.Pattern = pat; 
		if (la.kind == 25) {
			Get();
			it.ForType = ElaForType.Foreach; 
		} else if (la.kind == 26) {
			Get();
			it.ForType = ElaForType.ForTo; 
		} else if (la.kind == 27) {
			Get();
			it.ForType = ElaForType.ForDownto; 
		} else SynErr(122);
		Expr(out cexp);
		it.Target = cexp; 
		Expect(37);
		Expr(out cexp);
		it.Body = cexp; 
	}

	void IfExpr(out ElaExpression exp) {
		Expect(39);
		var cond = new ElaCondition(t); 
		var cexp = default(ElaExpression);	
		exp = cond;
		
		Expr(out cexp);
		cond.Condition = cexp; 
		Expect(41);
		Expr(out cexp);
		cond.True = cexp; 
		Expect(40);
		Expr(out cexp);
		cond.False = cexp; 
	}

	void RaiseExpr(out ElaExpression exp) {
		Expect(42);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		var code = String.Empty;
		
		Expect(1);
		code = t.val; 
		if (la.kind == 55) {
			Get();
			Expr(out cexp);
			Expect(56);
		}
		r.ErrorCode = code;
		r.Expression = cexp; 
		
	}

	void FailExpr(out ElaExpression exp) {
		Expect(49);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		
		Expr(out cexp);
		r.Expression = cexp; 
		r.ErrorCode = "Failure"; 
		
	}

	void ReturnExpr(out ElaExpression exp) {
		Expect(43);
		var cexp = default(ElaExpression);
		Expr(out cexp);
		exp = new ElaReturn(t) { Expression = cexp }; 
	}

	void BreakExpr(out ElaExpression exp) {
		Expect(44);
		exp = new ElaBreak(t); 
	}

	void ContinueExpr(out ElaExpression exp) {
		Expect(45);
		exp = new ElaContinue(t); 
	}

	void AssignExpr(out ElaExpression exp) {
		BackwardPipeExpr(out exp);
		while (la.kind == 66 || la.kind == 67) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 66) {
				Get();
				op = ElaOperator.Assign; 
			} else {
				Get();
				op = ElaOperator.Swap; 
			}
			BackwardPipeExpr(out cexp);
			exp = new ElaBinary(t) { Operator = op, Left = exp, Right = cexp }; 
		}
	}

	void BackwardPipeExpr(out ElaExpression exp) {
		ForwardPipeExpr(out exp);
		while (la.kind == 68) {
			var cexp = default(ElaExpression); 
			var ot = t;
			var mi = default(ElaFunctionCall);  
			
			Get();
			ForwardPipeExpr(out cexp);
			if (mi == null)
			{
				mi = new ElaFunctionCall(ot) { Target = exp };
				exp = mi; 
			}
			
				mi.Parameters.Add(cexp); 
			
		}
	}

	void ForwardPipeExpr(out ElaExpression exp) {
		OrExpr(out exp);
		while (la.kind == 69) {
			var cexp = default(ElaExpression); 
			
			Get();
			OrExpr(out cexp);
			var mi = new ElaFunctionCall(t) { Target = cexp };
			mi.Parameters.Add(exp);
			exp = mi;
			
		}
	}

	void OrExpr(out ElaExpression exp) {
		AndExpr(out exp);
		while (la.kind == 70) {
			var cexp = default(ElaExpression); 
			Get();
			AndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanOr, Right = cexp }; 
			
		}
	}

	void AndExpr(out ElaExpression exp) {
		EqExpr(out exp);
		while (la.kind == 71) {
			var cexp = default(ElaExpression); 
			Get();
			EqExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanAnd, Right = cexp }; 
			
		}
	}

	void EqExpr(out ElaExpression exp) {
		ShiftExpr(out exp);
		while (StartOf(14)) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			switch (la.kind) {
			case 72: {
				Get();
				op = ElaOperator.Equals; 
				break;
			}
			case 73: {
				Get();
				op = ElaOperator.NotEquals; 
				break;
			}
			case 74: {
				Get();
				op = ElaOperator.Greater; 
				break;
			}
			case 75: {
				Get();
				op = ElaOperator.Lesser; 
				break;
			}
			case 76: {
				Get();
				op = ElaOperator.GreaterEqual; 
				break;
			}
			case 77: {
				Get();
				op = ElaOperator.LesserEqual; 
				break;
			}
			}
			if (StartOf(9)) {
				ShiftExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, false);
			else
				exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void ShiftExpr(out ElaExpression exp) {
		ConcatExpr(out exp);
		while (la.kind == 78 || la.kind == 79) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 78) {
				Get();
				op = ElaOperator.ShiftRight; 
			} else {
				Get();
				op = ElaOperator.ShiftLeft; 
			}
			if (StartOf(9)) {
				ConcatExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, false);
			else
				exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void ConcatExpr(out ElaExpression exp) {
		ConsExpr(out exp);
		while (la.kind == 80) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			Get();
			op = ElaOperator.Concat; 
			if (StartOf(9)) {
				ConsExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, false);
			else
				exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void ConsExpr(out ElaExpression exp) {
		AddExpr(out exp);
		while (la.kind == 62) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			Get();
			op = ElaOperator.ConsList; 
			if (StartOf(9)) {
				ConsExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, false);
			else
				exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp };
			
		}
	}

	void AddExpr(out ElaExpression exp) {
		MulExpr(out exp);
		while (la.kind == 21 || la.kind == 81) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 81) {
				Get();
				op = ElaOperator.Add; 
			} else {
				Get();
				op = ElaOperator.Subtract; 
			}
			if (StartOf(9)) {
				MulExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, false);
			else
				exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void MulExpr(out ElaExpression exp) {
		CastExpr(out exp);
		while (StartOf(15)) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 82) {
				Get();
				op = ElaOperator.Multiply; 
			} else if (la.kind == 83) {
				Get();
				op = ElaOperator.Divide; 
			} else if (la.kind == 84) {
				Get();
				op = ElaOperator.Modulus; 
			} else {
				Get();
				op = ElaOperator.Power; 
			}
			if (StartOf(9)) {
				CastExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, false);
			else
				exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void CastExpr(out ElaExpression exp) {
		InfixExpr(out exp);
		while (la.kind == 22 || la.kind == 31 || la.kind == 57) {
			if (la.kind == 57) {
				Get();
				Expect(1);
				exp = new ElaCast(t) { CastAffinity = GetType(t.val), Expression = exp };
			} else if (la.kind == 31) {
				var pat = default(ElaPattern); 
				Get();
				IsOperatorPattern(out pat);
				exp = new ElaIs(t) { Expression = exp, Pattern = pat }; 
			} else {
				Get();
				ComprehensionOpExpr(exp, out exp);
			}
		}
	}

	void InfixExpr(out ElaExpression exp) {
		BitOrExpr(out exp);
		while (la.kind == 4 || la.kind == 9) {
			var cexp = default(ElaExpression); 
			var op = ElaOperator.None;
			var name = String.Empty;
			var ot = t;
			
			if (la.kind == 9) {
				Get();
				op = ElaOperator.Custom; name = t.val; 
			} else {
				Get();
				name = t.val.Trim('`'); 
			}
			if (StartOf(9)) {
				BitOrExpr(out cexp);
			}
			if (op == ElaOperator.None)
			{
				var fc = new ElaFunctionCall(ot) 
				{ 
					Target = GetBuiltin(ot, name)
				};
				fc.Parameters.Add(exp);			
				
				if (cexp != null)
					fc.Parameters.Add(cexp);
								
				exp = fc;
			}
			else
				exp = new ElaBinary(t) { CustomOperator = name, Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void BitOrExpr(out ElaExpression exp) {
		BitXorExpr(out exp);
		while (la.kind == 86) {
			var cexp = default(ElaExpression); 
			Get();
			if (StartOf(9)) {
				BitXorExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, ElaOperator.BitwiseOr, false);
			else
				exp = new ElaBinary(t) { 
					Left = exp, Operator = ElaOperator.BitwiseOr, Right = cexp }; 
			
		}
	}

	void BitXorExpr(out ElaExpression exp) {
		BitAndExpr(out exp);
		while (la.kind == 87) {
			var cexp = default(ElaExpression); 
			Get();
			if (StartOf(9)) {
				BitAndExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, ElaOperator.BitwiseXor, false);
			else
				exp = new ElaBinary(t) { 
					Left = exp, Operator = ElaOperator.BitwiseXor, Right = cexp }; 
			
		}
	}

	void BitAndExpr(out ElaExpression exp) {
		BackwardCompExpr(out exp);
		while (la.kind == 88) {
			var cexp = default(ElaExpression); 
			Get();
			if (StartOf(9)) {
				BackwardCompExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, ElaOperator.BitwiseAnd, false);
			else
				exp = new ElaBinary(t) { 
					Left = exp, Operator = ElaOperator.BitwiseAnd, Right = cexp }; 
			
		}
	}

	void BackwardCompExpr(out ElaExpression exp) {
		ForwardCompExpr(out exp);
		while (la.kind == 89) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			Get();
			op = ElaOperator.CompBackward; 
			if (StartOf(9)) {
				ForwardCompExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, true);
			else
				exp = new ElaBinary(t) { Right = exp, Operator = op, Left = cexp }; 
			
		}
	}

	void ForwardCompExpr(out ElaExpression exp) {
		UnaryExpr(out exp);
		while (la.kind == 90) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			Get();
			op = ElaOperator.CompForward; 
			if (StartOf(9)) {
				ForwardCompExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, false);
			else
				exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void UnaryExpr(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 63) {
			Get();
			if (StartOf(16)) {
				Application(out exp);
			}
			if (exp == null)
			exp = new ElaBuiltinFunction(t) { Kind = ElaBuiltinFunctionKind.Negate };
			else
				exp = new ElaUnary(t) { Expression = exp, Operator = ElaUnaryOperator.Negate }; 
			
		} else if (la.kind == 91) {
			Get();
			if (StartOf(16)) {
				Application(out exp);
			}
			if (exp == null)
			exp = new ElaBuiltinFunction(t) { Kind = ElaBuiltinFunctionKind.Bitnot };
			else
				exp = new ElaUnary(t) { Expression = exp, Operator = ElaUnaryOperator.BitwiseNot };
			
		} else if (StartOf(16)) {
			Application(out exp);
		} else SynErr(123);
	}

	void Application(out ElaExpression exp) {
		exp = null; 
		if (StartOf(5)) {
			AccessExpr(out exp);
			var ot = t;
			var mi = default(ElaFunctionCall); 
			var cexp = default(ElaExpression);
			
			while (StartOf(5)) {
				AccessExpr(out cexp);
				if (mi == null)
				{
					mi = new ElaFunctionCall(ot) { Target = exp };
					exp = mi; 
				}
				
					mi.Parameters.Add(cexp); 
				
			}
		} else if (la.kind == 2) {
			VariantLiteral(out exp);
		} else SynErr(124);
	}

	void AccessExpr(out ElaExpression exp) {
		Literal(out exp);
		while (la.kind == 24) {
			Get();
			if (la.kind == 13) {
				Get();
				var indExp = new ElaIndexer(t) { TargetObject = exp };
				exp = indExp;
				
				var cexp = default(ElaExpression); 
				Expr(out cexp);
				indExp.Index = cexp;	
				Expect(14);
			} else if (la.kind == 1) {
				Get();
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
			} else SynErr(125);
		}
	}

	void FuncOperator(out ElaOperator op) {
		op = ElaOperator.None; 
		switch (la.kind) {
		case 81: {
			Get();
			op = ElaOperator.Add; 
			break;
		}
		case 21: {
			Get();
			op = ElaOperator.Subtract; 
			break;
		}
		case 82: {
			Get();
			op = ElaOperator.Multiply; 
			break;
		}
		case 83: {
			Get();
			op = ElaOperator.Divide; 
			break;
		}
		case 84: {
			Get();
			op = ElaOperator.Modulus; 
			break;
		}
		case 85: {
			Get();
			op = ElaOperator.Power; 
			break;
		}
		case 72: {
			Get();
			op = ElaOperator.Equals; 
			break;
		}
		case 73: {
			Get();
			op = ElaOperator.NotEquals; 
			break;
		}
		case 74: {
			Get();
			op = ElaOperator.Greater; 
			break;
		}
		case 75: {
			Get();
			op = ElaOperator.Lesser; 
			break;
		}
		case 77: {
			Get();
			op = ElaOperator.LesserEqual; 
			break;
		}
		case 76: {
			Get();
			op = ElaOperator.GreaterEqual; 
			break;
		}
		case 90: {
			Get();
			op = ElaOperator.CompForward; 
			break;
		}
		case 89: {
			Get();
			op = ElaOperator.CompBackward; 
			break;
		}
		case 62: {
			Get();
			op = ElaOperator.ConsList; 
			break;
		}
		case 71: {
			Get();
			op = ElaOperator.BooleanAnd; 
			break;
		}
		case 70: {
			Get();
			op = ElaOperator.BooleanOr; 
			break;
		}
		case 88: {
			Get();
			op = ElaOperator.BitwiseAnd; 
			break;
		}
		case 86: {
			Get();
			op = ElaOperator.BitwiseOr; 
			break;
		}
		case 87: {
			Get();
			op = ElaOperator.BitwiseXor; 
			break;
		}
		case 79: {
			Get();
			op = ElaOperator.ShiftLeft; 
			break;
		}
		case 78: {
			Get();
			op = ElaOperator.ShiftRight; 
			break;
		}
		case 80: {
			Get();
			op = ElaOperator.Concat; 
			break;
		}
		default: SynErr(126); break;
		}
	}

	void LambdaExpr(out ElaExpression exp) {
		exp = null; 
		Expect(18);
		var ot = t;
		var mi = new ElaFunctionLiteral(t);
		exp = mi;
		mi.Body = new ElaMatch(t);
		
		LambdaBodyExpr(mi.Body);
	}

	void EmbExpr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 1: case 2: case 3: case 5: case 6: case 7: case 8: case 11: case 13: case 15: case 28: case 29: case 46: case 47: case 48: case 54: case 55: case 63: case 91: case 92: {
			BinaryExpr(out exp);
			break;
		}
		case 39: {
			IfExpr(out exp);
			break;
		}
		case 18: {
			LambdaExpr(out exp);
			break;
		}
		case 36: case 38: {
			IterExpr(out exp);
			break;
		}
		case 43: {
			ReturnExpr(out exp);
			break;
		}
		case 44: {
			BreakExpr(out exp);
			break;
		}
		case 45: {
			ContinueExpr(out exp);
			break;
		}
		case 42: {
			RaiseExpr(out exp);
			break;
		}
		case 49: {
			FailExpr(out exp);
			break;
		}
		default: SynErr(127); break;
		}
	}

	void IterExpr(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 36) {
			WhileExpr(out exp);
		} else if (la.kind == 38) {
			ForExpr(out exp);
		} else SynErr(128);
	}

	void ComprehensionEntry(ElaExpression body, out ElaFor it) {
		it = new ElaFor(t) { 
		ForType = ElaForType.Foreach
		};
		var cexp = default(ElaExpression);
		var pat = default(ElaPattern);
		
		ForeachPattern(out pat);
		Expect(66);
		Expr(out cexp);
		it.Pattern = pat;
		it.Target = cexp;
		
		if (la.kind == 59) {
			Guard(out cexp);
			it.Guard = cexp; 
		}
		it.Body = body; 
		if (la.kind == 58) {
			var cit = default(ElaFor); 
			Get();
			ComprehensionEntry(body, out cit);
			it.Body = cit; 
		}
	}

	void Ela() {
		if (StartOf(7)) {
			var exp = default(ElaExpression); 
			EmbExpr(out exp);
			Expression = exp; 
		} else if (la.kind == 32 || la.kind == 34) {
			var b = new ElaBlock(t);
			Expression = b;
			
			DeclarationBlock(b);
		} else SynErr(129);
	}

	void DeclarationBlock(ElaBlock b) {
		var exp = default(ElaExpression); 
		if (la.kind == 32) {
			RootLetBinding(out exp);
		} else if (la.kind == 34) {
			IncludeStat(out exp);
		} else SynErr(130);
		b.Expressions.Add(exp); 
		if (la.kind == 32 || la.kind == 34) {
			DeclarationBlock(b);
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,T,T, x,T,T,T, T,x,x,T, x,T,x,T, x,x,T,x, x,x,x,x, x,x,x,x, T,T,x,x, T,x,x,x, T,x,T,T, x,x,T,T, T,T,T,T, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x},
		{x,T,T,T, x,T,T,T, T,x,x,T, x,T,x,T, x,x,T,x, x,x,x,x, x,x,x,x, T,T,x,x, T,x,x,x, T,x,T,T, x,x,T,T, T,T,T,T, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x},
		{x,T,T,T, T,T,T,T, T,T,x,T, x,T,x,T, x,x,T,x, x,T,T,x, T,x,x,x, T,T,x,T, T,x,x,x, T,x,T,T, x,x,T,T, T,T,T,T, T,T,x,x, x,x,T,T, x,T,x,x, x,x,T,T, x,x,x,x, x,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,x},
		{x,T,T,x, x,T,T,T, T,x,x,T, x,T,x,T, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,T,T, x,x,x,T, T,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,x,T, x,T,T,T, T,x,x,T, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x},
		{x,T,T,T, x,T,T,T, T,x,x,T, x,T,x,T, x,x,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, T,x,T,T, x,x,T,T, T,T,T,T, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x},
		{x,T,T,x, x,T,T,T, T,x,x,T, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,T,T, x,x,x,x, T,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,T,T, x,T,T,T, T,x,x,T, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x},
		{x,T,x,x, x,T,T,T, T,x,x,T, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,T,T, x,x,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,x,x, x,T,T,T, T,x,x,T, T,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,T,T, T,x,T,T, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,x,x, x,T,T,T, T,x,x,T, x,T,x,T, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,T,T, x,x,x,T, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x},
		{x,T,T,T, x,T,T,T, T,x,x,T, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x}

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
			case 2: s = "variantTok expected"; break;
			case 3: s = "argIdent expected"; break;
			case 4: s = "funcTok expected"; break;
			case 5: s = "intTok expected"; break;
			case 6: s = "realTok expected"; break;
			case 7: s = "stringTok expected"; break;
			case 8: s = "charTok expected"; break;
			case 9: s = "operatorTok expected"; break;
			case 10: s = "SEMI expected"; break;
			case 11: s = "LBRA expected"; break;
			case 12: s = "RBRA expected"; break;
			case 13: s = "LILB expected"; break;
			case 14: s = "LIRB expected"; break;
			case 15: s = "ARLB expected"; break;
			case 16: s = "ARRB expected"; break;
			case 17: s = "ARROW expected"; break;
			case 18: s = "LAMBDA expected"; break;
			case 19: s = "EQ expected"; break;
			case 20: s = "SEQ expected"; break;
			case 21: s = "MINUS expected"; break;
			case 22: s = "COMPH expected"; break;
			case 23: s = "COMPO expected"; break;
			case 24: s = "DOT expected"; break;
			case 25: s = "IN expected"; break;
			case 26: s = "UPTO expected"; break;
			case 27: s = "DOWNTO expected"; break;
			case 28: s = "BASE expected"; break;
			case 29: s = "MATCH expected"; break;
			case 30: s = "ASAMP expected"; break;
			case 31: s = "IS expected"; break;
			case 32: s = "LET expected"; break;
			case 33: s = "PRIVATE expected"; break;
			case 34: s = "OPEN expected"; break;
			case 35: s = "WITH expected"; break;
			case 36: s = "WHILE expected"; break;
			case 37: s = "DO expected"; break;
			case 38: s = "FOR expected"; break;
			case 39: s = "IFS expected"; break;
			case 40: s = "ELSE expected"; break;
			case 41: s = "THEN expected"; break;
			case 42: s = "RAISE expected"; break;
			case 43: s = "RETURN expected"; break;
			case 44: s = "BREAK expected"; break;
			case 45: s = "CONTINUE expected"; break;
			case 46: s = "TRY expected"; break;
			case 47: s = "TRUE expected"; break;
			case 48: s = "FALSE expected"; break;
			case 49: s = "FAIL expected"; break;
			case 50: s = "WHERE expected"; break;
			case 51: s = "MUTABLE expected"; break;
			case 52: s = "AND expected"; break;
			case 53: s = "ENDS expected"; break;
			case 54: s = "\"_\" expected"; break;
			case 55: s = "\"(\" expected"; break;
			case 56: s = "\")\" expected"; break;
			case 57: s = "\":\" expected"; break;
			case 58: s = "\",\" expected"; break;
			case 59: s = "\"|\" expected"; break;
			case 60: s = "\"`\" expected"; break;
			case 61: s = "\"?\" expected"; break;
			case 62: s = "\"::\" expected"; break;
			case 63: s = "\"--\" expected"; break;
			case 64: s = "\"..\" expected"; break;
			case 65: s = "\"&\" expected"; break;
			case 66: s = "\"<-\" expected"; break;
			case 67: s = "\"<->\" expected"; break;
			case 68: s = "\"<|\" expected"; break;
			case 69: s = "\"|>\" expected"; break;
			case 70: s = "\"||\" expected"; break;
			case 71: s = "\"&&\" expected"; break;
			case 72: s = "\"==\" expected"; break;
			case 73: s = "\"<>\" expected"; break;
			case 74: s = "\">\" expected"; break;
			case 75: s = "\"<\" expected"; break;
			case 76: s = "\">=\" expected"; break;
			case 77: s = "\"<=\" expected"; break;
			case 78: s = "\">>>\" expected"; break;
			case 79: s = "\"<<<\" expected"; break;
			case 80: s = "\"++\" expected"; break;
			case 81: s = "\"+\" expected"; break;
			case 82: s = "\"*\" expected"; break;
			case 83: s = "\"/\" expected"; break;
			case 84: s = "\"%\" expected"; break;
			case 85: s = "\"**\" expected"; break;
			case 86: s = "\"|||\" expected"; break;
			case 87: s = "\"^^^\" expected"; break;
			case 88: s = "\"&&&\" expected"; break;
			case 89: s = "\"<<\" expected"; break;
			case 90: s = "\">>\" expected"; break;
			case 91: s = "\"~~~\" expected"; break;
			case 92: s = "\"(&\" expected"; break;
			case 93: s = "??? expected"; break;
			case 94: s = "invalid Literal"; break;
			case 95: s = "invalid Primitive"; break;
			case 96: s = "invalid SimpleExpr"; break;
			case 97: s = "invalid VariableReference"; break;
			case 98: s = "invalid GroupExpr"; break;
			case 99: s = "invalid OperatorExpr"; break;
			case 100: s = "invalid MemberAccess"; break;
			case 101: s = "invalid IsOperatorPattern"; break;
			case 102: s = "invalid Expr"; break;
			case 103: s = "invalid Guard"; break;
			case 104: s = "invalid OrPattern"; break;
			case 105: s = "invalid VariantPattern"; break;
			case 106: s = "invalid SinglePattern"; break;
			case 107: s = "invalid LiteralPattern"; break;
			case 108: s = "invalid LiteralPattern"; break;
			case 109: s = "invalid IdentPattern"; break;
			case 110: s = "invalid TypeCheckPattern"; break;
			case 111: s = "invalid BindingPattern"; break;
			case 112: s = "invalid ForeachPattern"; break;
			case 113: s = "invalid FieldPattern"; break;
			case 114: s = "invalid RecordField"; break;
			case 115: s = "invalid ParamList"; break;
			case 116: s = "invalid VariableAttributes"; break;
			case 117: s = "invalid VariableDeclarationBody"; break;
			case 118: s = "invalid WhereBindingBody"; break;
			case 119: s = "invalid BindingGuardList"; break;
			case 120: s = "invalid IncludeStat"; break;
			case 121: s = "invalid Qualident"; break;
			case 122: s = "invalid ForExpr"; break;
			case 123: s = "invalid UnaryExpr"; break;
			case 124: s = "invalid Application"; break;
			case 125: s = "invalid AccessExpr"; break;
			case 126: s = "invalid FuncOperator"; break;
			case 127: s = "invalid EmbExpr"; break;
			case 128: s = "invalid IterExpr"; break;
			case 129: s = "invalid Ela"; break;
			case 130: s = "invalid DeclarationBlock"; break;

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

}

