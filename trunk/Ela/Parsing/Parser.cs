
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
	public const int _specVariantTok = 3;
	public const int _intTok = 4;
	public const int _realTok = 5;
	public const int _stringTok = 6;
	public const int _charTok = 7;
	public const int _operatorTok1 = 8;
	public const int _operatorTok2 = 9;
	public const int _operatorTok3 = 10;
	public const int _operatorTok4 = 11;
	public const int _operatorTok5 = 12;
	public const int _operatorTok6 = 13;
	public const int _operatorTok7 = 14;
	public const int _operatorTok8 = 15;
	public const int _LBRA = 16;
	public const int _RBRA = 17;
	public const int _LILB = 18;
	public const int _LIRB = 19;
	public const int _PIPE = 20;
	public const int _ARROW = 21;
	public const int _LAMBDA = 22;
	public const int _COMPH = 23;
	public const int _COMPO = 24;
	public const int _DOT = 25;
	public const int _IN = 26;
	public const int _MATCH = 27;
	public const int _ASAMP = 28;
	public const int _IS = 29;
	public const int _LET = 30;
	public const int _PRIVATE = 31;
	public const int _OPEN = 32;
	public const int _WITH = 33;
	public const int _IFS = 34;
	public const int _ELSE = 35;
	public const int _THEN = 36;
	public const int _RAISE = 37;
	public const int _TRY = 38;
	public const int _TRUE = 39;
	public const int _FALSE = 40;
	public const int _FAIL = 41;
	public const int _WHERE = 42;
	public const int _QUALIFIED = 43;
	public const int _ET = 44;
	public const int _ON = 45;
	public const int _EBLOCK = 46;
	public const int maxT = 67;

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

	
	void EndBlock() {
		Expect(46);
		if (!t.virt) scanner.PopIndent(); 
	}

	void Literal(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 4: case 5: case 6: case 7: case 39: case 40: {
			Primitive(out exp);
			break;
		}
		case 16: {
			RecordLiteral(out exp);
			break;
		}
		case 18: {
			ListLiteral(out exp);
			break;
		}
		case 48: {
			TupleLiteral(out exp);
			break;
		}
		case 1: case 47: {
			SimpleExpr(out exp);
			break;
		}
		case 2: {
			VariantLiteral(out exp);
			break;
		}
		default: SynErr(68); break;
		}
	}

	void Primitive(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 4: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseInt(t.val) };	
			break;
		}
		case 5: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseReal(t.val) }; 
			break;
		}
		case 6: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseString(t.val) }; 
			break;
		}
		case 7: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseChar(t.val) }; 
			break;
		}
		case 39: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(true) }; 
			break;
		}
		case 40: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(false) }; 
			break;
		}
		default: SynErr(69); break;
		}
	}

	void RecordLiteral(out ElaExpression exp) {
		exp = null; 
		var fld = default(ElaFieldDeclaration);
		
		Expect(16);
		var rec = new ElaRecordLiteral(t); exp = rec; 
		RecordField(out fld);
		rec.Fields.Add(fld); 
		while (la.kind == 50) {
			Get();
			RecordField(out fld);
			rec.Fields.Add(fld); 
		}
		Expect(17);
	}

	void ListLiteral(out ElaExpression exp) {
		Expect(18);
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
		
		Expect(19);
	}

	void TupleLiteral(out ElaExpression exp) {
		var ot = default(Token);
		exp = null; 
		
		Expect(48);
		ot = t; 
		if (StartOf(1)) {
			if (StartOf(2)) {
				GroupExpr(out exp);
			} else {
				LazyExpr(out exp);
			}
		}
		Expect(49);
		if (exp == null)
		exp = new ElaUnitLiteral(ot);
		
	}

	void SimpleExpr(out ElaExpression exp) {
		exp = null; 
		VariableReference(out exp);
	}

	void VariantLiteral(out ElaExpression exp) {
		Expect(2);
		exp = new ElaVariantLiteral(t) { Tag = t.val }; 
	}

	void VariableReference(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = new ElaVariableReference(t) { VariableName = t.val }; 
		} else if (la.kind == 47) {
			Get();
			exp = new ElaPlaceholder(t); 
		} else SynErr(70);
	}

	void GroupExpr(out ElaExpression exp) {
		exp = null; 
		var cexp = default(ElaExpression);
		var ot = t;
		
		Expr(out exp);
		if (la.kind == 50) {
			var tuple = new ElaTupleLiteral(ot); 
			tuple.Parameters.Add(exp);
			exp = tuple; 
			
			Get();
			if (StartOf(2)) {
				Expr(out cexp);
				tuple.Parameters.Add(cexp); 
			}
			while (la.kind == 50) {
				Get();
				Expr(out cexp);
				tuple.Parameters.Add(cexp); 
			}
		}
	}

	void LazyExpr(out ElaExpression exp) {
		Expect(55);
		var lazy = new ElaLazyLiteral(t); 
		Expr(out exp);
		var m = new ElaMatch(t);
		m.Entries.Add(new ElaMatchEntry { Pattern = new ElaUnitPattern(), Expression = exp });
		lazy.Body = m;
		exp = lazy;
		
	}

	void Expr(out ElaExpression exp) {
		exp = null; 
		if (StartOf(3)) {
			EmbExpr(out exp);
		} else if (la.kind == 30) {
			LetBinding(out exp);
		} else SynErr(71);
	}

	void MatchExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 27)) {SynErr(72); Get();}
		Expect(27);
		var match = new ElaMatch(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(33);
		MatchEntry(match);
		while (StartOf(4)) {
			if (StartOf(5)) {
				MatchEntry(match);
			} else {
				ChildMatchEntry(match);
			}
		}
		EndBlock();
	}

	void MatchEntry(ElaMatch match) {
		var cexp = default(ElaExpression); 
		scanner.InjectBlock(); 
		var pat = default(ElaPattern); 
		var ot = t; 
		
		RootPattern(out pat);
		var entry = new ElaMatchEntry(ot);
		entry.Pattern = pat;				
		match.Entries.Add(entry);
		
		if (la.kind == 20) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(51);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 42) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
	}

	void ChildMatchEntry(ElaMatch match) {
		var cexp = default(ElaExpression); 
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		
		Guard(out cexp);
		entry.Guard = cexp; 
		Expect(51);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 42) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
	}

	void RootPattern(out ElaPattern pat) {
		OrPattern(out pat);
		if (la.kind == 52) {
			ConsPattern(pat, out pat);
		}
	}

	void Guard(out ElaExpression exp) {
		exp = null; 
		Expect(20);
		if (StartOf(6)) {
			BinaryExpr(out exp);
			while (la.kind == 50) {
				var old = exp; 
				Get();
				var ot = t; 
				BinaryExpr(out exp);
				exp = new ElaBinary(t) { Operator = ElaOperator.BooleanAnd, Left = old, Right = exp };
				
			}
		} else if (la.kind == 35) {
			Get();
			exp = new ElaOtherwiseGuard(t); 
		} else SynErr(73);
	}

	void WhereBinding(out ElaExpression exp) {
		var flags = default(ElaVariableFlags); 
		scanner.InjectBlock(); 
		
		while (!(la.kind == 0 || la.kind == 42)) {SynErr(74); Get();}
		Expect(42);
		if (la.kind == 31 || la.kind == 56) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		EndBlock();
	}

	void BinaryExpr(out ElaExpression exp) {
		exp = null;
		var ot = t; 
		
		AssignExpr(out exp);
		while (la.kind == 66) {
			var cexp = default(ElaExpression); 
			Get();
			AssignExpr(out cexp);
			exp = new ElaBinary(t) { Operator = ElaOperator.Sequence, Left = exp, Right = cexp };
			
		}
	}

	void OrPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 2 || la.kind == 3) {
			VariantPattern(out pat);
		} else if (StartOf(7)) {
			AsPattern(out pat);
		} else SynErr(75);
	}

	void ConsPattern(ElaPattern prev, out ElaPattern exp) {
		var cexp = default(ElaPattern); 
		var ht = new ElaHeadTailPattern(t); 
		ht.Patterns.Add(prev);
		exp = ht;				
		
		Expect(52);
		AsPattern(out cexp);
		ht.Patterns.Add(cexp); 
		while (la.kind == 52) {
			Get();
			AsPattern(out cexp);
			ht.Patterns.Add(cexp); 
		}
	}

	void ParenPattern(out ElaPattern pat) {
		pat = null; 
		OrPattern(out pat);
		if (la.kind == 52) {
			ConsPattern(pat, out pat);
		}
		if (la.kind == 50) {
			TuplePattern(pat, out pat);
		}
	}

	void TuplePattern(ElaPattern prev, out ElaPattern pat) {
		var seq = new ElaTuplePattern(t); 
		seq.Patterns.Add(prev);
		var cpat = default(ElaPattern);
		pat = seq;
		
		Expect(50);
		if (StartOf(7)) {
			AsPattern(out cpat);
			if (la.kind == 52) {
				ConsPattern(cpat, out cpat);
			}
			seq.Patterns.Add(cpat); 
			while (la.kind == 50) {
				Get();
				AsPattern(out cpat);
				if (la.kind == 52) {
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
		} else if (la.kind == 3) {
			Get();
		} else SynErr(76);
		vp = new ElaVariantPattern(t);
		vp.Tag = t.val; 
		
		if (StartOf(7)) {
			AsPattern(out cpat);
			vp.Pattern = cpat; 
		}
		pat = vp; 
	}

	void AsPattern(out ElaPattern pat) {
		pat = null; 
		SinglePattern(out pat);
		if (la.kind == 28) {
			Get();
			var asPat = new ElaAsPattern(t) { Pattern = pat }; 
			pat = asPat;				
			
			if (la.kind == 1) {
				Get();
				asPat.Name = t.val; 
			} else if (la.kind == 48) {
				Get();
				Operators();
				asPat.Name = t.val; 
				Expect(49);
			} else SynErr(77);
		}
	}

	void FuncPattern(out ElaPattern pat) {
		AsPattern(out pat);
	}

	void FuncPattern2(out ElaPattern pat) {
		AsPattern(out pat);
		if (la.kind == 52) {
			ConsPattern(pat, out pat);
		}
	}

	void SinglePattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 47: {
			DefaultPattern(out pat);
			break;
		}
		case 48: {
			UnitPattern(out pat);
			break;
		}
		case 4: case 5: case 6: case 7: case 39: case 40: {
			LiteralPattern(out pat);
			break;
		}
		case 18: {
			ListPattern(out pat);
			break;
		}
		case 16: {
			RecordPattern(out pat);
			break;
		}
		case 1: {
			IdentPattern(out pat);
			break;
		}
		default: SynErr(78); break;
		}
	}

	void DefaultPattern(out ElaPattern pat) {
		Expect(47);
		pat = new ElaDefaultPattern(t); 
	}

	void UnitPattern(out ElaPattern pat) {
		var ot = t;
		pat = null;
		
		Expect(48);
		if (StartOf(8)) {
			if (StartOf(5)) {
				ParenPattern(out pat);
			} else {
				SymbolicIdentPattern(out pat);
			}
		}
		Expect(49);
		if (pat == null)
		pat = new ElaUnitPattern(ot); 
		
	}

	void LiteralPattern(out ElaPattern pat) {
		var lit = default(ElaLiteralValue);
		pat = null;
		
		switch (la.kind) {
		case 6: {
			Get();
			lit = ParseString(t.val); 
			break;
		}
		case 7: {
			Get();
			lit = ParseChar(t.val); 
			break;
		}
		case 4: {
			Get();
			lit = ParseInt(t.val); 
			break;
		}
		case 5: {
			Get();
			lit = ParseReal(t.val); 
			break;
		}
		case 39: {
			Get();
			lit = new ElaLiteralValue(true); 
			break;
		}
		case 40: {
			Get();
			lit = new ElaLiteralValue(false); 
			break;
		}
		default: SynErr(79); break;
		}
		pat = new ElaLiteralPattern(t) { Value = lit };				
		
	}

	void ListPattern(out ElaPattern pat) {
		var cexp = default(ElaPattern); 
		var ht = default(ElaHeadTailPattern); 
		pat = null;
		
		Expect(18);
		if (StartOf(7)) {
			ht = new ElaHeadTailPattern(t);			
			
			AsPattern(out cexp);
			ht.Patterns.Add(cexp);  
			while (la.kind == 50) {
				Get();
				AsPattern(out cexp);
				ht.Patterns.Add(cexp); 
			}
			ht.Patterns.Add(new ElaNilPattern(t));
			pat = ht;
			
		}
		if (pat == null)
		pat = new ElaNilPattern(t);
		
		Expect(19);
	}

	void RecordPattern(out ElaPattern pat) {
		pat = null; 
		var cpat = default(ElaFieldPattern);
		
		Expect(16);
		var rec = new ElaRecordPattern(t); 
		pat = rec; 
		
		if (la.kind == 1 || la.kind == 6) {
			FieldPattern(out cpat);
			rec.Fields.Add(cpat); 
			while (la.kind == 50) {
				Get();
				FieldPattern(out cpat);
				rec.Fields.Add(cpat); 
			}
		}
		Expect(17);
	}

	void IdentPattern(out ElaPattern pat) {
		Expect(1);
		pat = new ElaVariablePattern(t) { Name = t.val }; 
	}

	void Operators() {
		switch (la.kind) {
		case 8: {
			Get();
			break;
		}
		case 9: {
			Get();
			break;
		}
		case 10: {
			Get();
			break;
		}
		case 11: {
			Get();
			break;
		}
		case 12: {
			Get();
			break;
		}
		case 13: {
			Get();
			break;
		}
		case 14: {
			Get();
			break;
		}
		case 15: {
			Get();
			break;
		}
		case 52: {
			Get();
			break;
		}
		case 53: {
			Get();
			break;
		}
		default: SynErr(80); break;
		}
	}

	void SymbolicIdentPattern(out ElaPattern pat) {
		Operators();
		pat = new ElaVariablePattern(t) { Name = t.val }; 
	}

	void BindingPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 47) {
			DefaultPattern(out pat);
		} else if (la.kind == 48) {
			UnitPattern(out pat);
		} else if (StartOf(9)) {
			LiteralPattern(out pat);
		} else if (la.kind == 18) {
			ListPattern(out pat);
		} else if (la.kind == 16) {
			RecordPattern(out pat);
		} else SynErr(81);
		if (la.kind == 28) {
			Get();
			var asPat = new ElaAsPattern(t) { Pattern = pat }; 
			pat = asPat;				
			
			if (la.kind == 1) {
				Get();
				asPat.Name = t.val; 
			} else if (la.kind == 48) {
				Get();
				Operators();
				asPat.Name = t.val; 
				Expect(49);
			} else SynErr(82);
		}
	}

	void GeneratorPattern(out ElaPattern pat) {
		pat = null; string name = null; 
		if (la.kind == 1) {
			IdentPattern(out pat);
			name = pat.GetName(); 
		} else if (StartOf(9)) {
			LiteralPattern(out pat);
		} else if (la.kind == 16) {
			RecordPattern(out pat);
		} else if (la.kind == 18) {
			ListPattern(out pat);
		} else if (la.kind == 48) {
			UnitPattern(out pat);
		} else SynErr(83);
	}

	void IsOperatorPattern(out ElaPattern pat) {
		pat = null; 
		if (StartOf(9)) {
			LiteralPattern(out pat);
		} else if (la.kind == 16) {
			RecordPattern(out pat);
		} else if (la.kind == 18) {
			ListPattern(out pat);
		} else if (la.kind == 48) {
			UnitPattern(out pat);
		} else if (la.kind == 2 || la.kind == 3) {
			VariantPattern(out pat);
		} else SynErr(84);
	}

	void FieldPattern(out ElaFieldPattern fld) {
		fld = null;
		var cpat = default(ElaPattern);
		
		if (la.kind == 6) {
			Get();
			fld = new ElaFieldPattern(t) { Name = ReadString(t.val) }; 
			Expect(51);
			AsPattern(out cpat);
			fld.Value = cpat; 
		} else if (la.kind == 1) {
			Get();
			fld = new ElaFieldPattern(t) { Name = t.val }; 
			if (la.kind == 51) {
				Get();
				AsPattern(out cpat);
			}
			if (cpat == null)
			cpat = new ElaVariablePattern(t) { Name = fld.Name };
			
			fld.Value = cpat; 
			
		} else SynErr(85);
	}

	void RecordField(out ElaFieldDeclaration fld) {
		fld = null; 
		var cexp = default(ElaExpression);
		var mutable = false;
		
		if (la.kind == 53) {
			Get();
			mutable = true; 
		}
		if (la.kind == 1 || la.kind == 2) {
			if (la.kind == 1) {
				Get();
			} else {
				Get();
			}
			fld = new ElaFieldDeclaration(t) { FieldName = t.val, Mutable = mutable }; 
			if (la.kind == 51) {
				Get();
				Expr(out cexp);
				fld.FieldValue = cexp; 
			}
			if (fld.FieldValue == null)
			fld.FieldValue = new ElaVariableReference(t) { VariableName = t.val };
			
		} else if (la.kind == 6) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = ReadString(t.val), Mutable = mutable }; 
			Expect(51);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else SynErr(86);
	}

	void RangeExpr(ElaExpression first, ElaExpression sec, out ElaRange rng) {
		rng = new ElaRange(t) { First = first, Second = sec };
		var cexp = default(ElaExpression);
		
		Expect(54);
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
		if (la.kind == 24 || la.kind == 50 || la.kind == 54) {
			if (la.kind == 24) {
				ComprehensionExpr(exp, out comp);
			} else if (la.kind == 54) {
				RangeExpr(exp, null, out rng);
			} else {
				var oexp = exp; 
				Get();
				Expr(out exp);
				if (la.kind == 54) {
					RangeExpr(oexp, exp, out rng);
				} else if (la.kind == 19 || la.kind == 50) {
					list = new List<ElaExpression>();
					list.Add(oexp);
					list.Add(exp);
					
					while (la.kind == 50) {
						Get();
						Expr(out exp);
						list.Add(exp); 
					}
				} else SynErr(87);
			}
		}
		if (list == null && comp == null && rng == null && exp != null)
		{
			list = new List<ElaExpression>();
			list.Add(exp);
		}
		
	}

	void ComprehensionExpr(ElaExpression sel, out ElaComprehension exp) {
		var it = default(ElaGenerator); 
		var ot = t;		
		
		Expect(24);
		ComprehensionEntry(sel, out it);
		exp = new ElaComprehension(ot) { Generator = it }; 
	}

	void LetBinding(out ElaExpression exp) {
		exp = null; 
		var inExp = default(ElaExpression);
		var flags = default(ElaVariableFlags);
		
		while (!(la.kind == 0 || la.kind == 30)) {SynErr(88); Get();}
		Expect(30);
		if (la.kind == 31 || la.kind == 56) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		ExpectWeak(26, 10);
		Expr(out inExp);
		((ElaBinding)exp).In = inExp; 
	}

	void VariableAttributes(out ElaVariableFlags flags) {
		flags = ElaVariableFlags.None; 
		if (la.kind == 31) {
			Get();
			flags |= ElaVariableFlags.Private; 
			if (la.kind == 56) {
				Get();
				flags |= ElaVariableFlags.Inline; 
			}
		} else if (la.kind == 56) {
			Get();
			flags |= ElaVariableFlags.Inline; 
			if (la.kind == 31) {
				Get();
				flags |= ElaVariableFlags.Private; 
			}
		} else SynErr(89);
	}

	void BindingBody(ElaVariableFlags flags, out ElaExpression exp) {
		var varExp = new ElaBinding(t) { VariableFlags = flags }; 
		exp = varExp;
		var cexp = default(ElaExpression); 
		var pat = default(ElaPattern);
		
		if (la.kind == 1) {
			Get();
			varExp.VariableName = t.val; 
			if (StartOf(11)) {
				if (StartOf(7)) {
					FunExpr(varExp);
				} else if (la.kind == 20) {
					BindingBodyGuards(varExp);
				} else if (la.kind == 51) {
					BindingBodyInit(varExp);
				} else {
					InfixFunExpr(new ElaVariablePattern(t) { Name = t.val },varExp);
				}
			}
			SetObjectMetadata(varExp, cexp); 
		} else if (StartOf(12)) {
			BindingPattern(out pat);
			if (pat.Type != ElaNodeType.VariablePattern)
			varExp.Pattern = pat; 
			else
				varExp.VariableName = pat.GetName();
			
			if (StartOf(13)) {
				if (la.kind == 51) {
					BindingBodyInit(varExp);
				} else if (StartOf(14)) {
					varExp.Pattern = null; 
					InfixFunExpr(pat,varExp);
				} else {
					if (pat.Type != ElaNodeType.VariablePattern)
					AddError(ElaParserError.InvalidFunctionDeclaration, pat);
					
					FunExpr(varExp);
				}
			}
		} else if (StartOf(15)) {
			BindingBodyOperator(varExp);
		} else SynErr(90);
		if (la.kind == 44) {
			ExpectWeak(44, 16);
			BindingBody(flags, out exp);
			((ElaBinding)varExp).And = (ElaBinding)exp;
			exp = varExp;
			
		}
	}

	void RootLetBinding(out ElaExpression exp) {
		exp = null; 
		var inExp = default(ElaExpression);
		var flags = default(ElaVariableFlags);
		
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 30)) {SynErr(91); Get();}
		Expect(30);
		if (la.kind == 31 || la.kind == 56) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		if (la.kind == 26) {
			ExpectWeak(26, 10);
			Expr(out inExp);
			((ElaBinding)exp).In = inExp; 
			EndBlock();
		} else if (la.kind == 46) {
			EndBlock();
		} else SynErr(92);
	}

	void BindingGuard(out ElaExpression exp) {
		exp = null; 
		BinaryExpr(out exp);
		while (la.kind == 50) {
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
		
		Expect(20);
		if (StartOf(6)) {
			var newCond = new ElaCondition(t);
			cond.False = newCond;
			cond = newCond;
			
			BindingGuard(out gexp);
			cond.Condition = gexp; 
			Expect(51);
			Expr(out cexp);
			cond.True = cexp; 
			if (la.kind == 20) {
				BindingGuardList(ref cond);
			}
		} else if (la.kind == 35) {
			Get();
			Expect(51);
			Expr(out cexp);
			cond.False = cexp; 
		} else SynErr(93);
	}

	void FunExpr(ElaBinding varExp) {
		var ot = t;
		var mi = new ElaFunctionLiteral(t);
		mi.Name = varExp.VariableName;
		varExp.InitExpression = mi;
		mi.Body = new ElaMatch(t);
		scanner.InjectBlock(t.col);				
		
		FunBodyExpr(mi);
		ProcessFunctionParameters(mi, ot); 
		if (la.kind == 45) {
			OverloadExpr(varExp);
		}
	}

	void BindingBodyGuards(ElaBinding varExp) {
		var gexp = default(ElaExpression);
		var cexp3 = default(ElaExpression);
		var cond = new ElaCondition(t);
		varExp.InitExpression = cond;
		
		Expect(20);
		BindingGuard(out gexp);
		cond.Condition = gexp; 
		Expect(51);
		Expr(out cexp3);
		cond.True = cexp3; 
		BindingGuardList(ref cond);
	}

	void BindingBodyInit(ElaBinding varExp) {
		var cexp = default(ElaExpression); 
		var cexp2 = default(ElaExpression);
		
		Expect(51);
		if (StartOf(2)) {
			Expr(out cexp);
		} else if (la.kind == 57) {
			Get();
			Expect(1);
			cexp = new ElaBuiltin(t) { Kind = Builtins.Kind(t.val) }; 
		} else SynErr(94);
		varExp.InitExpression = cexp; 
		if (la.kind == 42) {
			WhereBinding(out cexp2);
			varExp.Where = (ElaBinding)cexp2; 
		}
	}

	void InfixFunExpr(ElaPattern pat, ElaBinding varExp) {
		var ot = t;
		var mi = new ElaFunctionLiteral(t);
		varExp.InitExpression = mi;
		mi.Body = new ElaMatch(t);            
		scanner.InjectBlock(t.col);				
		
		InfixFunBodyExprFirst(pat,mi);
		varExp.VariableName = mi.Name;
		ProcessFunctionParameters(mi, ot); 
		
		if (la.kind == 45) {
			OverloadExpr(varExp);
		}
	}

	void BindingBodyOperator(ElaBinding varExp) {
		Operators();
		varExp.VariableName = t.val; 
		if (StartOf(7)) {
			FunExpr(varExp);
			var fun = (ElaFunctionLiteral)varExp.InitExpression;
			fun.FunctionType = ElaFunctionType.Operator;
			
		} else if (la.kind == 51) {
			BindingBodyInit(varExp);
			if (varExp.InitExpression.Type == ElaNodeType.FunctionLiteral)
			((ElaFunctionLiteral)varExp.InitExpression).FunctionType = ElaFunctionType.Operator;
			
		} else SynErr(95);
	}

	void InfixFunBodyExprFirst(ElaPattern pat, ElaFunctionLiteral fun) {
		var ot = t;
		var match = fun.Body;
		var cexp = default(ElaExpression);			
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		entry.Pattern = pat;
		
		InfixFunName(fun);
		if (StartOf(7)) {
			FuncPattern(out pat);
			var seq = new ElaPatternGroup(ot);
			seq.Patterns.Add(entry.Pattern);
			seq.Patterns.Add(pat);
			entry.Pattern = seq;
			
		}
		if (la.kind == 20) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(51);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 42) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(17)) {
			if (StartOf(7)) {
				scanner.InjectBlock(); 
				InfixFunBodyExpr(fun);
			} else {
				InfixChildFunBodyExpr(fun);
			}
		}
	}

	void OverloadExpr(ElaBinding varExp) {
		Expect(45);
		if (la.kind == 2) {
			Get();
		} else if (la.kind == 3) {
			Get();
		} else SynErr(96);
		varExp.OverloadNames.Add(t.val); 
		while (la.kind == 21) {
			Get();
			if (la.kind == 2) {
				Get();
			} else if (la.kind == 3) {
				Get();
			} else SynErr(97);
			varExp.OverloadNames.Add(t.val); 
		}
	}

	void InfixFunName(ElaFunctionLiteral fun) {
		var name = String.Empty; 
		if (la.kind == 58) {
			Get();
			Expect(1);
			name = t.val; 
			Expect(58);
		} else if (StartOf(15)) {
			Operators();
			name = t.val; 
		} else SynErr(98);
		if (String.IsNullOrEmpty(fun.Name))
		fun.Name = name;
		else if (name != fun.Name)
			AddError(ElaParserError.InvalidFunctionDeclaration, name);
		
	}

	void InfixFunBodyExpr(ElaFunctionLiteral fun) {
		var ot = t;
		var match = fun.Body;
		var pat = default(ElaPattern);
		var cexp = default(ElaExpression);			
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		
		FuncPattern(out pat);
		entry.Pattern = pat; 
		InfixFunName(fun);
		if (StartOf(7)) {
			FuncPattern(out pat);
			var seq = new ElaPatternGroup(ot);
			seq.Patterns.Add(entry.Pattern);
			seq.Patterns.Add(pat);
			entry.Pattern = seq;
			
		}
		if (la.kind == 20) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(51);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 42) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(17)) {
			if (StartOf(7)) {
				scanner.InjectBlock(); 
				InfixFunBodyExpr(fun);
			} else {
				InfixChildFunBodyExpr(fun);
			}
		}
	}

	void InfixChildFunBodyExpr(ElaFunctionLiteral fun) {
		var ot = t;
		var match = fun.Body;
		var cexp = default(ElaExpression);
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		
		Guard(out cexp);
		entry.Guard = cexp; 
		Expect(51);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 42) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(17)) {
			if (StartOf(7)) {
				scanner.InjectBlock(); 
				InfixFunBodyExpr(fun);
			} else {
				InfixChildFunBodyExpr(fun);
			}
		}
	}

	void FunName(ElaFunctionLiteral fun) {
		if (la.kind == 1) {
			Get();
		} else if (StartOf(15)) {
			Operators();
		} else SynErr(99);
		if (t.val != fun.Name)
		AddError(ElaParserError.InvalidFunctionDeclaration, t.val);
		
	}

	void FunBodyExpr(ElaFunctionLiteral fun) {
		var ot = t;
		var match = fun.Body;
		var pat = default(ElaPattern);
		var seq = default(ElaPatternGroup);
		var cexp = default(ElaExpression);			
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		
		FuncPattern2(out pat);
		entry.Pattern = pat; 
		while (StartOf(7)) {
			FuncPattern2(out pat);
			if (seq == null)
			{
				seq = new ElaPatternGroup(ot);
				seq.Patterns.Add(entry.Pattern);
				entry.Pattern = seq;							
			}
			
				seq.Patterns.Add(pat); 
			
		}
		if (la.kind == 20) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(51);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 42) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(18)) {
			if (StartOf(19)) {
				scanner.InjectBlock(); 
				FunName(fun);
				FunBodyExpr(fun);
			} else {
				ChildFunBodyExpr(fun);
			}
		}
	}

	void ChildFunBodyExpr(ElaFunctionLiteral fun) {
		var ot = t;
		var match = fun.Body;
		var cexp = default(ElaExpression);
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		
		Guard(out cexp);
		entry.Guard = cexp; 
		Expect(51);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 42) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(18)) {
			if (StartOf(19)) {
				scanner.InjectBlock(); 
				FunName(fun);
				FunBodyExpr(fun);
			} else {
				ChildFunBodyExpr(fun);
			}
		}
	}

	void LambdaExpr(out ElaExpression exp) {
		while (!(la.kind == 0 || la.kind == 22)) {SynErr(100); Get();}
		Expect(22);
		var ot = t;
		var pat = default(ElaPattern);
		var seq = default(ElaPatternGroup);
		
		var mi = new ElaFunctionLiteral(t);
		exp = mi;
		var match = new ElaMatch(t);
		mi.Body = match;
		
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		
		FuncPattern2(out pat);
		entry.Pattern = pat; 
		while (StartOf(7)) {
			FuncPattern2(out pat);
			if (seq == null)
			{
				seq = new ElaPatternGroup(ot);
				seq.Patterns.Add(entry.Pattern);
				entry.Pattern = seq;							
			}
			
				seq.Patterns.Add(pat); 
			
		}
		if (la.kind == 20) {
			var cexp = default(ElaExpression); 
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(21);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
	}

	void IncludeStat(out ElaExpression exp) {
		exp = null; 
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 32)) {SynErr(101); Get();}
		Expect(32);
		var inc = new ElaModuleInclude(t); 
		if (la.kind == 43) {
			Get();
			inc.RequireQuailified = true; 
		}
		Qualident(inc.Path);
		var name = inc.Path[inc.Path.Count - 1];				
		inc.Path.RemoveAt(inc.Path.Count - 1);				
		inc.Alias = inc.Name = name;
		exp = inc;
		
		if (la.kind == 59) {
			Get();
			if (la.kind == 1) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 2) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 6) {
				Get();
				inc.DllName = ReadString(t.val); 
			} else SynErr(102);
		}
		if (la.kind == 28) {
			Get();
			Expect(1);
			inc.Alias = t.val; 
		}
		if (la.kind == 48) {
			var imp = default(ElaImportedVariable); 
			Get();
			ImportName(out imp);
			inc.ImportList.Add(imp); 
			while (la.kind == 50) {
				Get();
				ImportName(out imp);
				inc.ImportList.Add(imp); 
			}
			Expect(49);
		}
		Expect(46);
	}

	void Qualident(List<String> path ) {
		var val = String.Empty; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 2) {
			Get();
			val = t.val; 
		} else if (la.kind == 6) {
			Get();
			val = ReadString(t.val); 
		} else SynErr(103);
		path.Add(val); 
		if (la.kind == 25) {
			Get();
			Qualident(path);
		}
	}

	void ImportName(out ElaImportedVariable imp) {
		imp = new ElaImportedVariable(t); 
		if (la.kind == 31) {
			Get();
			imp.Private = true; 
		}
		Expect(1);
		imp.Name = imp.LocalName = t.val; 
		if (la.kind == 51) {
			Get();
			Expect(1);
			imp.Name = t.val; 
		}
	}

	void IfExpr(out ElaExpression exp) {
		Expect(34);
		var cond = new ElaCondition(t); 
		var cexp = default(ElaExpression);	
		exp = cond;
		
		Expr(out cexp);
		cond.Condition = cexp; 
		Expect(36);
		Expr(out cexp);
		cond.True = cexp; 
		ExpectWeak(35, 10);
		Expr(out cexp);
		cond.False = cexp; 
	}

	void RaiseExpr(out ElaExpression exp) {
		Expect(37);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		var code = String.Empty;
		
		Expect(2);
		code = t.val; 
		if (la.kind == 48) {
			Get();
			Expr(out cexp);
			Expect(49);
		}
		r.ErrorCode = code;
		r.Expression = cexp; 
		
	}

	void FailExpr(out ElaExpression exp) {
		Expect(41);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		
		Expr(out cexp);
		r.Expression = cexp; 
		r.ErrorCode = "Failure"; 
		
	}

	void TryExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		Expect(38);
		var ot = t;
		var match = new ElaTry(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(33);
		scanner.InjectBlock(); 
		MatchEntry(match);
		EndBlock();
		while (StartOf(5)) {
			scanner.InjectBlock(); 
			MatchEntry(match);
			EndBlock();
		}
		EndBlock();
	}

	void AssignExpr(out ElaExpression exp) {
		BackwardPipeExpr(out exp);
		while (la.kind == 60) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			Get();
			op = ElaOperator.Assign; 
			BackwardPipeExpr(out cexp);
			exp = new ElaBinary(t) { Operator = op, Left = exp, Right = cexp }; 
		}
	}

	void BackwardPipeExpr(out ElaExpression exp) {
		ForwardPipeExpr(out exp);
		while (la.kind == 61) {
			var cexp = default(ElaExpression); 
			var ot = t;
			var mi = default(ElaFunctionCall);  
			
			Get();
			BackwardPipeExpr(out cexp);
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
		while (la.kind == 62) {
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
		while (la.kind == 63) {
			var cexp = default(ElaExpression); 
			Get();
			AndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanOr, Right = cexp }; 
			
		}
	}

	void AndExpr(out ElaExpression exp) {
		OpExpr1(out exp);
		while (la.kind == 64) {
			var cexp = default(ElaExpression); 
			Get();
			OpExpr1(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanAnd, Right = cexp }; 
			
		}
	}

	void OpExpr1(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(20)) {
			OpExpr2(out exp);
			while (la.kind == 8) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(20)) {
					OpExpr2(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 8) {
			Get();
			op = t.val; 
			if (StartOf(20)) {
				OpExpr2(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = op };
			
		} else SynErr(104);
	}

	void OpExpr2(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(21)) {
			OpExpr3(out exp);
			while (la.kind == 9) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(21)) {
					OpExpr3(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 9) {
			Get();
			op = t.val; 
			if (StartOf(21)) {
				OpExpr3(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(105);
	}

	void OpExpr3(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(22)) {
			OpExpr4(out exp);
			while (la.kind == 10 || la.kind == 52) {
				var cexp = default(ElaExpression); 
				if (la.kind == 10) {
					Get();
				} else {
					Get();
				}
				op = t.val; 
				if (StartOf(21)) {
					OpExpr3(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 10 || la.kind == 52) {
			if (la.kind == 10) {
				Get();
			} else {
				Get();
			}
			op = t.val; 
			if (StartOf(22)) {
				OpExpr4(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(106);
	}

	void OpExpr4(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(23)) {
			OpExpr5(out exp);
			while (la.kind == 11) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(23)) {
					OpExpr5(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 11) {
			Get();
			op = t.val; 
			if (StartOf(23)) {
				OpExpr5(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(107);
	}

	void OpExpr5(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(24)) {
			CastExpr(out exp);
			while (la.kind == 12) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(24)) {
					CastExpr(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 12) {
			Get();
			op = t.val; 
			if (StartOf(24)) {
				CastExpr(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(108);
	}

	void CastExpr(out ElaExpression exp) {
		InfixExpr(out exp);
		while (la.kind == 23 || la.kind == 29) {
			if (la.kind == 29) {
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
		exp = null;
		var ot = t; 
		var funexp = default(ElaExpression);
		
		if (StartOf(25)) {
			OpExpr6(out exp);
			while (la.kind == 58) {
				var cexp = default(ElaExpression); 
				ot = t;
				
				Get();
				Literal(out funexp);
				Expect(58);
				if (StartOf(25)) {
					OpExpr6(out cexp);
				}
				var fc = new ElaFunctionCall(ot) { 
					Target = funexp
				};
				fc.Parameters.Add(exp);			
				
				if (cexp != null)
					fc.Parameters.Add(cexp);
								
				exp = fc;
				
			}
		} else if (la.kind == 58) {
			Get();
			Literal(out funexp);
			Expect(58);
			if (StartOf(25)) {
				OpExpr6(out exp);
				exp = GetPrefixFun(funexp, exp, true);	
			}
			if (exp == null)
			exp = funexp;
			
		} else SynErr(109);
	}

	void ComprehensionOpExpr(ElaExpression init, out ElaExpression exp) {
		var list = default(List<ElaExpression>);
		var comp = default(ElaComprehension);
		var rng = default(ElaRange);
		exp = null;
		
		Expect(18);
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
		
		Expect(19);
	}

	void OpExpr6(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(26)) {
			OpExpr7(out exp);
			while (la.kind == 13) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(25)) {
					OpExpr6(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 13) {
			Get();
			op = t.val; 
			if (StartOf(26)) {
				OpExpr7(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(110);
	}

	void OpExpr7(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(27)) {
			OpExpr8(out exp);
			while (la.kind == 14) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(27)) {
					OpExpr8(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 14) {
			Get();
			op = t.val; 
			if (StartOf(27)) {
				OpExpr8(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(111);
	}

	void OpExpr8(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(28)) {
			Application(out exp);
			while (la.kind == 15 || la.kind == 53 || la.kind == 65) {
				var cexp = default(ElaExpression); 
				if (la.kind == 15) {
					Get();
				} else if (la.kind == 53) {
					Get();
				} else {
					Get();
				}
				op = t.val; 
				if (StartOf(28)) {
					Application(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 15 || la.kind == 53 || la.kind == 65) {
			if (la.kind == 15) {
				Get();
			} else if (la.kind == 53) {
				Get();
			} else {
				Get();
			}
			op = t.val; 
			if (StartOf(28)) {
				Application(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(112);
	}

	void Application(out ElaExpression exp) {
		exp = null; 
		AccessExpr(out exp);
		var ot = t;
		var mi = default(ElaFunctionCall); 
		var cexp = default(ElaExpression);
		
		while (StartOf(28)) {
			AccessExpr(out cexp);
			if (mi == null)
			{
				mi = new ElaFunctionCall(ot) { Target = exp };
				exp = mi; 
			}
			
				mi.Parameters.Add(cexp); 
			
		}
	}

	void AccessExpr(out ElaExpression exp) {
		Literal(out exp);
		while (la.kind == 25) {
			Get();
			if (la.kind == 18) {
				Get();
				var indExp = new ElaIndexer(t) { TargetObject = exp };
				exp = indExp;
				
				var cexp = default(ElaExpression); 
				Expr(out cexp);
				indExp.Index = cexp;	
				Expect(19);
			} else if (la.kind == 48) {
				Get();
				if (la.kind == 1) {
					Get();
				} else if (la.kind == 2) {
					Get();
				} else if (StartOf(15)) {
					Operators();
				} else SynErr(113);
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
				Expect(49);
			} else if (la.kind == 1 || la.kind == 2) {
				if (la.kind == 1) {
					Get();
				} else {
					Get();
				}
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
			} else SynErr(114);
		}
	}

	void EmbExpr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 1: case 2: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13: case 14: case 15: case 16: case 18: case 39: case 40: case 47: case 48: case 52: case 53: case 58: case 65: {
			BinaryExpr(out exp);
			break;
		}
		case 34: {
			IfExpr(out exp);
			break;
		}
		case 22: {
			LambdaExpr(out exp);
			break;
		}
		case 37: {
			RaiseExpr(out exp);
			break;
		}
		case 41: {
			FailExpr(out exp);
			break;
		}
		case 27: {
			MatchExpr(out exp);
			break;
		}
		case 38: {
			TryExpr(out exp);
			break;
		}
		default: SynErr(115); break;
		}
	}

	void ComprehensionEntry(ElaExpression body, out ElaGenerator it) {
		it = new ElaGenerator(t);
		it.Body = body;
		var cexp = default(ElaExpression);
		var pat = default(ElaPattern);
		
		GeneratorPattern(out pat);
		Expect(60);
		Expr(out cexp);
		it.Pattern = pat;
		it.Target = cexp;
		
		if (la.kind == 20 || la.kind == 50) {
			if (la.kind == 50) {
				var cit = default(ElaGenerator); 
				Get();
				ComprehensionEntry(body, out cit);
				it.Body = cit; 
			} else {
				Guard(out cexp);
				it.Guard = cexp; 
			}
		}
	}

	void Ela() {
		var b = new ElaBlock(t);
		Expression = b;
		
		DeclarationBlock(b);
		while (StartOf(29)) {
			DeclarationBlock(b);
		}
	}

	void DeclarationBlock(ElaBlock b) {
		var exp = default(ElaExpression); 
		if (la.kind == 30) {
			RootLetBinding(out exp);
		} else if (la.kind == 32) {
			IncludeStat(out exp);
		} else if (StartOf(3)) {
			EmbExpr(out exp);
			if (FALSE) 
			Expect(46);
		} else SynErr(116);
		b.Expressions.Add(exp); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Ela();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,T,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,x,T, x,x,T,x, x,x,T,x, x,T,T,T, T,T,x,x, x,x,x,T, T,x,x,x, T,T,x,T, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,x,T, x,x,T,x, x,x,T,x, x,T,T,T, T,T,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,x,T, x,x,x,x, x,x,T,x, x,T,T,T, T,T,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,x,x, T,T,T,T, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,x,T, x,x,T,x, T,x,T,x, x,T,T,T, T,T,T,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,T, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,T, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,T,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,x,T, x,x,T,x, T,x,x,x, x,x,x,T, T,x,T,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, T,T,T,T, x,x,x,x, x,x,x,x, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, x,x,x,x, T,T,T,T, T,T,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, x,x,x,x, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, x,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,x, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,x, x,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,x, x,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,x, x,x,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,x, x,x,x,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,x,T, x,x,T,x, T,x,T,x, x,T,T,T, T,T,x,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,T,x,x, x}

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
			case 3: s = "specVariantTok expected"; break;
			case 4: s = "intTok expected"; break;
			case 5: s = "realTok expected"; break;
			case 6: s = "stringTok expected"; break;
			case 7: s = "charTok expected"; break;
			case 8: s = "operatorTok1 expected"; break;
			case 9: s = "operatorTok2 expected"; break;
			case 10: s = "operatorTok3 expected"; break;
			case 11: s = "operatorTok4 expected"; break;
			case 12: s = "operatorTok5 expected"; break;
			case 13: s = "operatorTok6 expected"; break;
			case 14: s = "operatorTok7 expected"; break;
			case 15: s = "operatorTok8 expected"; break;
			case 16: s = "LBRA expected"; break;
			case 17: s = "RBRA expected"; break;
			case 18: s = "LILB expected"; break;
			case 19: s = "LIRB expected"; break;
			case 20: s = "PIPE expected"; break;
			case 21: s = "ARROW expected"; break;
			case 22: s = "LAMBDA expected"; break;
			case 23: s = "COMPH expected"; break;
			case 24: s = "COMPO expected"; break;
			case 25: s = "DOT expected"; break;
			case 26: s = "IN expected"; break;
			case 27: s = "MATCH expected"; break;
			case 28: s = "ASAMP expected"; break;
			case 29: s = "IS expected"; break;
			case 30: s = "LET expected"; break;
			case 31: s = "PRIVATE expected"; break;
			case 32: s = "OPEN expected"; break;
			case 33: s = "WITH expected"; break;
			case 34: s = "IFS expected"; break;
			case 35: s = "ELSE expected"; break;
			case 36: s = "THEN expected"; break;
			case 37: s = "RAISE expected"; break;
			case 38: s = "TRY expected"; break;
			case 39: s = "TRUE expected"; break;
			case 40: s = "FALSE expected"; break;
			case 41: s = "FAIL expected"; break;
			case 42: s = "WHERE expected"; break;
			case 43: s = "QUALIFIED expected"; break;
			case 44: s = "ET expected"; break;
			case 45: s = "ON expected"; break;
			case 46: s = "EBLOCK expected"; break;
			case 47: s = "\"_\" expected"; break;
			case 48: s = "\"(\" expected"; break;
			case 49: s = "\")\" expected"; break;
			case 50: s = "\",\" expected"; break;
			case 51: s = "\"=\" expected"; break;
			case 52: s = "\"::\" expected"; break;
			case 53: s = "\"!\" expected"; break;
			case 54: s = "\"..\" expected"; break;
			case 55: s = "\"&\" expected"; break;
			case 56: s = "\"inline\" expected"; break;
			case 57: s = "\"__internal\" expected"; break;
			case 58: s = "\"`\" expected"; break;
			case 59: s = "\"#\" expected"; break;
			case 60: s = "\"<-\" expected"; break;
			case 61: s = "\"<|\" expected"; break;
			case 62: s = "\"|>\" expected"; break;
			case 63: s = "\"||\" expected"; break;
			case 64: s = "\"&&\" expected"; break;
			case 65: s = "\":\" expected"; break;
			case 66: s = "\"$\" expected"; break;
			case 67: s = "??? expected"; break;
			case 68: s = "invalid Literal"; break;
			case 69: s = "invalid Primitive"; break;
			case 70: s = "invalid VariableReference"; break;
			case 71: s = "invalid Expr"; break;
			case 72: s = "this symbol not expected in MatchExpr"; break;
			case 73: s = "invalid Guard"; break;
			case 74: s = "this symbol not expected in WhereBinding"; break;
			case 75: s = "invalid OrPattern"; break;
			case 76: s = "invalid VariantPattern"; break;
			case 77: s = "invalid AsPattern"; break;
			case 78: s = "invalid SinglePattern"; break;
			case 79: s = "invalid LiteralPattern"; break;
			case 80: s = "invalid Operators"; break;
			case 81: s = "invalid BindingPattern"; break;
			case 82: s = "invalid BindingPattern"; break;
			case 83: s = "invalid GeneratorPattern"; break;
			case 84: s = "invalid IsOperatorPattern"; break;
			case 85: s = "invalid FieldPattern"; break;
			case 86: s = "invalid RecordField"; break;
			case 87: s = "invalid ParamList"; break;
			case 88: s = "this symbol not expected in LetBinding"; break;
			case 89: s = "invalid VariableAttributes"; break;
			case 90: s = "invalid BindingBody"; break;
			case 91: s = "this symbol not expected in RootLetBinding"; break;
			case 92: s = "invalid RootLetBinding"; break;
			case 93: s = "invalid BindingGuardList"; break;
			case 94: s = "invalid BindingBodyInit"; break;
			case 95: s = "invalid BindingBodyOperator"; break;
			case 96: s = "invalid OverloadExpr"; break;
			case 97: s = "invalid OverloadExpr"; break;
			case 98: s = "invalid InfixFunName"; break;
			case 99: s = "invalid FunName"; break;
			case 100: s = "this symbol not expected in LambdaExpr"; break;
			case 101: s = "this symbol not expected in IncludeStat"; break;
			case 102: s = "invalid IncludeStat"; break;
			case 103: s = "invalid Qualident"; break;
			case 104: s = "invalid OpExpr1"; break;
			case 105: s = "invalid OpExpr2"; break;
			case 106: s = "invalid OpExpr3"; break;
			case 107: s = "invalid OpExpr4"; break;
			case 108: s = "invalid OpExpr5"; break;
			case 109: s = "invalid InfixExpr"; break;
			case 110: s = "invalid OpExpr6"; break;
			case 111: s = "invalid OpExpr7"; break;
			case 112: s = "invalid OpExpr8"; break;
			case 113: s = "invalid AccessExpr"; break;
			case 114: s = "invalid AccessExpr"; break;
			case 115: s = "invalid EmbExpr"; break;
			case 116: s = "invalid DeclarationBlock"; break;

			default: s = "error " + n; break;
		}
		
		parser.errorCount++;
		
		if (parser.la.val == "\t" || parser.t.val == "\t")
			n = -1;
		
		ErrorList.Add(ErrorReporter.CreateMessage(n, s, line, col, parser.la));
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

