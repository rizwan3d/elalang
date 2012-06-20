
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
	public const int _intTok = 3;
	public const int _realTok = 4;
	public const int _stringTok = 5;
	public const int _charTok = 6;
	public const int _operatorTok1 = 7;
	public const int _operatorTok2 = 8;
	public const int _operatorTok3 = 9;
	public const int _operatorTok4 = 10;
	public const int _operatorTok5 = 11;
	public const int _operatorTok6 = 12;
	public const int _operatorTok7 = 13;
	public const int _operatorTok8 = 14;
	public const int _operatorTok9 = 15;
	public const int _LBRA = 16;
	public const int _RBRA = 17;
	public const int _LILB = 18;
	public const int _LIRB = 19;
	public const int _PIPE = 20;
	public const int _ARROW = 21;
	public const int _LAMBDA = 22;
	public const int _COMPO = 23;
	public const int _DOT = 24;
	public const int _IN = 25;
	public const int _MATCH = 26;
	public const int _ASAMP = 27;
	public const int _IS = 28;
	public const int _LET = 29;
	public const int _PRIVATE = 30;
	public const int _OPEN = 31;
	public const int _WITH = 32;
	public const int _IFS = 33;
	public const int _ELSE = 34;
	public const int _THEN = 35;
	public const int _RAISE = 36;
	public const int _TRY = 37;
	public const int _TRUE = 38;
	public const int _FALSE = 39;
	public const int _FAIL = 40;
	public const int _WHERE = 41;
	public const int _QUALIFIED = 42;
	public const int _ET = 43;
	public const int _EBLOCK = 44;
	public const int maxT = 66;

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
		errors.AddErr(t.line, t.col, error, args);
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
		Expect(44);
		if (!t.virt) scanner.PopIndent(); 
	}

	void Literal(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 3: case 4: case 5: case 6: case 38: case 39: {
			Primitive(out exp);
			break;
		}
		case 16: {
			RecordLiteral(out exp);
			break;
		}
		case 18: case 53: {
			ListLiteral(out exp);
			break;
		}
		case 46: {
			TupleLiteral(out exp);
			break;
		}
		case 1: case 45: {
			SimpleExpr(out exp);
			break;
		}
		case 2: {
			VariantLiteral(out exp);
			break;
		}
		default: SynErr(67); break;
		}
	}

	void Primitive(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 3: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseInt(t.val) };	
			break;
		}
		case 4: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseReal(t.val) }; 
			break;
		}
		case 5: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseString(t.val) }; 
			break;
		}
		case 6: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseChar(t.val) }; 
			break;
		}
		case 38: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(true) }; 
			break;
		}
		case 39: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(false) }; 
			break;
		}
		default: SynErr(68); break;
		}
	}

	void RecordLiteral(out ElaExpression exp) {
		exp = null; 
		var fld = default(ElaFieldDeclaration);
		
		Expect(16);
		var rec = new ElaRecordLiteral(t); exp = rec; 
		RecordField(out fld);
		rec.Fields.Add(fld); 
		while (la.kind == 48) {
			Get();
			RecordField(out fld);
			rec.Fields.Add(fld); 
		}
		Expect(17);
	}

	void ListLiteral(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 18) {
			Get();
			var list = default(List<ElaExpression>);
			var comp = default(ElaComprehension);
			var rng = default(ElaRange);
			var ot = t;
			
			exp = null;
			
			if (StartOf(1)) {
				ParamList(out list, out comp, out rng);
				if (list != null)
				{
				    var listExp = new ElaListLiteral(ot) { Values = list };
				 exp = listExp;
				}	
				else if (comp != null)
				    exp = comp;
				else if (rng != null)
				    exp = rng;
				
			}
			if (exp == null)
			exp = new ElaListLiteral(ot);
			
			Expect(19);
		} else if (la.kind == 53) {
			Get();
			var comp = default(ElaComprehension);
			var ot = t;        				
			exp = null;
			
			var cexp = default(ElaExpression); 
			Expr(out cexp);
			ComprehensionExpr(cexp, out comp);
			comp.Lazy = true;
			exp = comp;
			
			Expect(19);
		} else SynErr(69);
	}

	void TupleLiteral(out ElaExpression exp) {
		var ot = default(Token);
		exp = null; 
		
		Expect(46);
		ot = t; 
		if (StartOf(1)) {
			GroupExpr(out exp);
		}
		Expect(47);
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
		} else if (la.kind == 45) {
			Get();
			exp = new ElaPlaceholder(t); 
		} else SynErr(70);
	}

	void GroupExpr(out ElaExpression exp) {
		exp = null; 
		var cexp = default(ElaExpression);
		var ot = t;
		
		Expr(out exp);
		if (la.kind == 48) {
			var tuple = new ElaTupleLiteral(ot); 
			tuple.Parameters.Add(exp);
			exp = tuple; 
			
			Get();
			if (StartOf(1)) {
				Expr(out cexp);
				tuple.Parameters.Add(cexp); 
			}
			while (la.kind == 48) {
				Get();
				Expr(out cexp);
				tuple.Parameters.Add(cexp); 
			}
		}
	}

	void Expr(out ElaExpression exp) {
		exp = null; 
		if (StartOf(2)) {
			EmbExpr(out exp);
		} else if (la.kind == 29) {
			LetBinding(out exp);
		} else SynErr(71);
	}

	void MatchExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 26)) {SynErr(72); Get();}
		Expect(26);
		var match = new ElaMatch(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(32);
		MatchEntry(match);
		while (StartOf(3)) {
			if (StartOf(4)) {
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
		Expect(49);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 41) {
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
		Expect(49);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
	}

	void RootPattern(out ElaPattern pat) {
		OrPattern(out pat);
		if (la.kind == 51) {
			ConsPattern(pat, out pat);
		}
	}

	void Guard(out ElaExpression exp) {
		exp = null; 
		Expect(20);
		if (StartOf(5)) {
			BinaryExpr(out exp);
			while (la.kind == 20) {
				var old = exp; 
				Get();
				var ot = t; 
				BinaryExpr(out exp);
				exp = new ElaBinary(t) { Operator = ElaOperator.BooleanOr, Left = old, Right = exp };
				
			}
		} else if (la.kind == 34) {
			Get();
			exp = new ElaOtherwiseGuard(t); 
		} else SynErr(73);
	}

	void WhereBinding(out ElaExpression exp) {
		var flags = default(ElaVariableFlags); 
		scanner.InjectBlock(); 
		
		while (!(la.kind == 0 || la.kind == 41)) {SynErr(74); Get();}
		Expect(41);
		if (la.kind == 30 || la.kind == 54) {
			VariableAttributes(ref flags);
		}
		BindingBody(flags, out exp);
		EndBlock();
	}

	void BinaryExpr(out ElaExpression exp) {
		exp = null;
		var ot = t; 
		
		OrExpr(out exp);
		while (la.kind == 60) {
			var cexp = default(ElaExpression); 
			Get();
			OrExpr(out cexp);
			exp = new ElaBinary(t) { Operator = ElaOperator.Sequence, Left = exp, Right = cexp };
			
		}
	}

	void OrPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 2) {
			VariantPattern(out pat);
		} else if (StartOf(6)) {
			AsPattern(out pat);
		} else SynErr(75);
	}

	void ConsPattern(ElaPattern prev, out ElaPattern exp) {
		var cexp = default(ElaPattern); 
		var ht = new ElaHeadTailPattern(t); 
		ht.Patterns.Add(prev);
		exp = ht;				
		
		Expect(51);
		AsPattern(out cexp);
		ht.Patterns.Add(cexp); 
		while (la.kind == 51) {
			Get();
			AsPattern(out cexp);
			ht.Patterns.Add(cexp); 
		}
	}

	void ParenPattern(out ElaPattern pat) {
		pat = null; 
		OrPattern(out pat);
		if (la.kind == 51) {
			ConsPattern(pat, out pat);
		}
		if (la.kind == 48) {
			TuplePattern(pat, out pat);
		}
	}

	void TuplePattern(ElaPattern prev, out ElaPattern pat) {
		var seq = new ElaTuplePattern(t); 
		seq.Patterns.Add(prev);
		var cpat = default(ElaPattern);
		pat = seq;
		
		Expect(48);
		if (StartOf(6)) {
			AsPattern(out cpat);
			if (la.kind == 51) {
				ConsPattern(cpat, out cpat);
			}
			seq.Patterns.Add(cpat); 
			while (la.kind == 48) {
				Get();
				AsPattern(out cpat);
				if (la.kind == 51) {
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
		
		Expect(2);
		vp = new ElaVariantPattern(t);
		vp.Tag = t.val; 
		
		if (StartOf(6)) {
			AsPattern(out cpat);
			vp.Pattern = cpat; 
		}
		pat = vp; 
	}

	void AsPattern(out ElaPattern pat) {
		pat = null; 
		SinglePattern(out pat);
		if (la.kind == 27) {
			Get();
			var asPat = new ElaAsPattern { Pattern = pat }; 
			pat = asPat;				
			
			if (la.kind == 1) {
				Get();
				asPat.Name = t.val; 
				asPat.SetLinePragma(t.line, t.col);
				
			} else if (la.kind == 46) {
				Get();
				Operators();
				asPat.Name = t.val; 
				asPat.SetLinePragma(t.line, t.col); 
				
				Expect(47);
			} else SynErr(76);
		}
	}

	void SimpleVariantPattern(out ElaPattern pat) {
		pat = null; 
		Expect(2);
		pat = new ElaVariantPattern(t) { Tag = t.val };
		
	}

	void FuncPattern(out ElaPattern pat) {
		pat = null; 
		if (StartOf(6)) {
			AsPattern(out pat);
		} else if (la.kind == 2) {
			SimpleVariantPattern(out pat);
		} else SynErr(77);
	}

	void FuncPattern2(out ElaPattern pat) {
		pat = null; 
		if (StartOf(6)) {
			AsPattern(out pat);
		} else if (la.kind == 2) {
			SimpleVariantPattern(out pat);
		} else SynErr(78);
		if (la.kind == 51) {
			ConsPattern(pat, out pat);
		}
	}

	void SinglePattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 45: {
			DefaultPattern(out pat);
			break;
		}
		case 46: {
			UnitPattern(out pat);
			break;
		}
		case 3: case 4: case 5: case 6: case 38: case 39: {
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
		case 50: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(79); break;
		}
	}

	void DefaultPattern(out ElaPattern pat) {
		Expect(45);
		pat = new ElaDefaultPattern(t); 
	}

	void UnitPattern(out ElaPattern pat) {
		var ot = t;
		pat = null;
		
		Expect(46);
		if (StartOf(7)) {
			if (StartOf(4)) {
				ParenPattern(out pat);
			} else {
				SymbolicIdentPattern(out pat);
			}
		}
		Expect(47);
		if (pat == null)
		pat = new ElaUnitPattern(ot); 
		
	}

	void LiteralPattern(out ElaPattern pat) {
		var lit = default(ElaLiteralValue);
		pat = null;
		
		switch (la.kind) {
		case 5: {
			Get();
			lit = ParseString(t.val); 
			break;
		}
		case 6: {
			Get();
			lit = ParseChar(t.val); 
			break;
		}
		case 3: {
			Get();
			lit = ParseInt(t.val); 
			break;
		}
		case 4: {
			Get();
			lit = ParseReal(t.val); 
			break;
		}
		case 38: {
			Get();
			lit = new ElaLiteralValue(true); 
			break;
		}
		case 39: {
			Get();
			lit = new ElaLiteralValue(false); 
			break;
		}
		default: SynErr(80); break;
		}
		pat = new ElaLiteralPattern(t) { Value = lit };				
		
	}

	void ListPattern(out ElaPattern pat) {
		var cexp = default(ElaPattern); 
		var ht = default(ElaHeadTailPattern); 
		pat = null;
		
		Expect(18);
		if (StartOf(6)) {
			ht = new ElaHeadTailPattern(t);			
			
			AsPattern(out cexp);
			ht.Patterns.Add(cexp);  
			while (la.kind == 48) {
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
		
		if (la.kind == 1 || la.kind == 5) {
			FieldPattern(out cpat);
			rec.Fields.Add(cpat); 
			while (la.kind == 48) {
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

	void TypeCheckPattern(out ElaPattern pat) {
		Expect(50);
		var eis = new ElaIsPattern(t);
		pat = eis; 			
		var fst = default(String); 
		var snd = default(String);
		
		if (la.kind == 1) {
			Get();
			fst = t.val; 
			if (la.kind == 24) {
				Get();
				Expect(1);
				snd = t.val; 
			}
			if (fst != null && snd != null)
			{
			    eis.TypePrefix = fst;
			    eis.TypeName = snd; 
			}
			else
			    eis.TypeName = fst;
			
		} else if (la.kind == 46) {
			Get();
			while (la.kind == 1 || la.kind == 2) {
				if (la.kind == 1) {
					Get();
				} else {
					Get();
				}
				fst = t.val; snd = null; 
				if (la.kind == 24) {
					Get();
					if (la.kind == 1) {
						Get();
					} else if (la.kind == 2) {
						Get();
					} else SynErr(81);
					snd = t.val; 
				}
				var ti = new TraitInfo(snd != null ? fst : null, snd != null ? snd : fst);
				eis.Traits.Add(ti);
				
			}
			Expect(47);
		} else SynErr(82);
	}

	void Operators() {
		switch (la.kind) {
		case 7: {
			Get();
			break;
		}
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
		case 51: {
			Get();
			break;
		}
		default: SynErr(83); break;
		}
	}

	void SymbolicIdentPattern(out ElaPattern pat) {
		Operators();
		pat = new ElaVariablePattern(t) { Name = t.val }; 
	}

	void BindingPattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 45: {
			DefaultPattern(out pat);
			break;
		}
		case 46: {
			UnitPattern(out pat);
			break;
		}
		case 3: case 4: case 5: case 6: case 38: case 39: {
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
		case 50: {
			TypeCheckPattern(out pat);
			break;
		}
		case 2: {
			SimpleVariantPattern(out pat);
			break;
		}
		default: SynErr(84); break;
		}
		if (la.kind == 27) {
			Get();
			var asPat = new ElaAsPattern(t) { Pattern = pat }; 
			pat = asPat;				
			
			if (la.kind == 1) {
				Get();
				asPat.Name = t.val; 
			} else if (la.kind == 46) {
				Get();
				Operators();
				asPat.Name = t.val; 
				Expect(47);
			} else SynErr(85);
		}
	}

	void GeneratorPattern(out ElaPattern pat) {
		pat = null; string name = null; 
		switch (la.kind) {
		case 1: {
			IdentPattern(out pat);
			name = pat.GetName(); 
			break;
		}
		case 3: case 4: case 5: case 6: case 38: case 39: {
			LiteralPattern(out pat);
			break;
		}
		case 16: {
			RecordPattern(out pat);
			break;
		}
		case 18: {
			ListPattern(out pat);
			break;
		}
		case 46: {
			UnitPattern(out pat);
			break;
		}
		case 2: {
			SimpleVariantPattern(out pat);
			break;
		}
		default: SynErr(86); break;
		}
	}

	void IsOperatorPattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 50: {
			TypeCheckPattern(out pat);
			break;
		}
		case 3: case 4: case 5: case 6: case 38: case 39: {
			LiteralPattern(out pat);
			break;
		}
		case 16: {
			RecordPattern(out pat);
			break;
		}
		case 18: {
			ListPattern(out pat);
			break;
		}
		case 46: {
			UnitPattern(out pat);
			break;
		}
		case 2: {
			VariantPattern(out pat);
			break;
		}
		default: SynErr(87); break;
		}
	}

	void FieldPattern(out ElaFieldPattern fld) {
		fld = null;
		var cpat = default(ElaPattern);
		
		if (la.kind == 5) {
			Get();
			fld = new ElaFieldPattern(t) { Name = ReadString(t.val) }; 
			Expect(49);
			AsPattern(out cpat);
			fld.Value = cpat; 
		} else if (la.kind == 1) {
			Get();
			fld = new ElaFieldPattern(t) { Name = t.val }; 
			if (la.kind == 49) {
				Get();
				AsPattern(out cpat);
			}
			if (cpat == null)
			cpat = new ElaVariablePattern(t) { Name = fld.Name };
			
			fld.Value = cpat; 
			
		} else SynErr(88);
	}

	void RecordField(out ElaFieldDeclaration fld) {
		fld = null; 
		var cexp = default(ElaExpression);
		
		if (la.kind == 1) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = t.val }; 
			if (la.kind == 49) {
				Get();
				Expr(out cexp);
				fld.FieldValue = cexp; 
			}
			if (fld.FieldValue == null)
			fld.FieldValue = new ElaVariableReference(t) { VariableName = t.val };
			
		} else if (la.kind == 5) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = ReadString(t.val) }; 
			Expect(49);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else SynErr(89);
	}

	void RangeExpr(ElaExpression first, ElaExpression sec, out ElaRange rng) {
		rng = new ElaRange(t) { First = first, Second = sec };
		var cexp = default(ElaExpression);
		
		Expect(52);
		if (StartOf(1)) {
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
		if (la.kind == 23 || la.kind == 48 || la.kind == 52) {
			if (la.kind == 23) {
				ComprehensionExpr(exp, out comp);
			} else if (la.kind == 52) {
				RangeExpr(exp, null, out rng);
			} else {
				var oexp = exp; 
				Get();
				Expr(out exp);
				if (la.kind == 52) {
					RangeExpr(oexp, exp, out rng);
				} else if (la.kind == 19 || la.kind == 48) {
					list = new List<ElaExpression>();
					list.Add(oexp);
					list.Add(exp);
					
					while (la.kind == 48) {
						Get();
						Expr(out exp);
						list.Add(exp); 
					}
				} else SynErr(90);
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
		
		Expect(23);
		ComprehensionEntry(sel, out it);
		exp = new ElaComprehension(ot) { Generator = it }; 
	}

	void LetBinding(out ElaExpression exp) {
		exp = null; 
		var inExp = default(ElaExpression);
		var flags = default(ElaVariableFlags);
		
		while (!(la.kind == 0 || la.kind == 29)) {SynErr(91); Get();}
		Expect(29);
		if (la.kind == 30 || la.kind == 54) {
			VariableAttributes(ref flags);
		}
		BindingBody(flags, out exp);
		ExpectWeak(25, 8);
		Expr(out inExp);
		((ElaBinding)exp).In = inExp; 
	}

	void VariableAttributes(ref ElaVariableFlags flags) {
		if (la.kind == 30) {
			Get();
			flags |= ElaVariableFlags.Private; 
		} else if (la.kind == 54) {
			Get();
			flags |= ElaVariableFlags.Inline; 
		} else SynErr(92);
		if (la.kind == 30 || la.kind == 54) {
			VariableAttributes(ref flags);
		}
	}

	void BindingBody(ElaVariableFlags flags, out ElaExpression exp) {
		var varExp = new ElaBinding(t) { VariableFlags = flags }; 
		exp = varExp;
		var pat = default(ElaPattern);
		
		if (la.kind == 1) {
			Get();
			varExp.VariableName = t.val; 
			varExp.SetLinePragma(t.line, t.col);
			
			if (StartOf(9)) {
				if (StartOf(4)) {
					FunExpr(varExp);
				} else if (la.kind == 20) {
					BindingBodyGuards(varExp);
				} else if (la.kind == 49) {
					BindingBodyInit(varExp);
				} else {
					InfixFunExpr(new ElaVariablePattern(t) { Name = t.val },varExp);
				}
			}
			SetObjectMetadata(varExp); 
		} else if (StartOf(10)) {
			BindingPattern(out pat);
			if (pat.Type != ElaNodeType.VariablePattern)
			varExp.Pattern = pat; 
			else
			{
				varExp.VariableName = pat.GetName();
				varExp.SetLinePragma(pat.Line, pat.Column);
			}
			
			if (StartOf(9)) {
				if (la.kind == 49) {
					BindingBodyInit(varExp);
				} else if (StartOf(11)) {
					varExp.Pattern = null; 
					InfixFunExpr(pat,varExp);
				} else if (StartOf(4)) {
					if (pat.Type != ElaNodeType.VariablePattern)
					AddError(ElaParserError.InvalidFunctionDeclaration, pat);
					
					FunExpr(varExp);
				} else {
					BindingBodyGuards(varExp);
				}
			}
		} else if (StartOf(12)) {
			BindingBodyOperator(varExp);
		} else SynErr(93);
		if (la.kind == 43) {
			ExpectWeak(43, 13);
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
		while (!(la.kind == 0 || la.kind == 29)) {SynErr(94); Get();}
		Expect(29);
		if (la.kind == 30 || la.kind == 54) {
			VariableAttributes(ref flags);
		}
		BindingBody(flags, out exp);
		if (la.kind == 25) {
			ExpectWeak(25, 8);
			Expr(out inExp);
			((ElaBinding)exp).In = inExp; 
			EndBlock();
		} else if (la.kind == 44) {
			EndBlock();
		} else SynErr(95);
	}

	void BindingGuard(out ElaExpression exp) {
		exp = null; 
		BinaryExpr(out exp);
		while (la.kind == 20) {
			var old = exp; 
			Get();
			var ot = t; 
			BinaryExpr(out exp);
			exp = new ElaBinary(t) { Operator = ElaOperator.BooleanOr, Left = old, Right = exp };
			
		}
	}

	void BindingGuardList(ref ElaCondition cond) {
		var gexp = default(ElaExpression);
		var cexp = default(ElaExpression);
		
		Expect(20);
		if (StartOf(5)) {
			var newCond = new ElaCondition(t);
			cond.False = newCond;
			cond = newCond;
			
			BindingGuard(out gexp);
			cond.Condition = gexp; 
			Expect(49);
			Expr(out cexp);
			cond.True = cexp; 
			if (la.kind == 20) {
				BindingGuardList(ref cond);
			}
		} else if (la.kind == 34) {
			Get();
			Expect(49);
			Expr(out cexp);
			cond.False = cexp; 
		} else SynErr(96);
	}

	void FunExpr(ElaBinding varExp) {
		var ot = t;
		var mi = new ElaFunctionLiteral(t);
		mi.Name = varExp.VariableName;
		varExp.InitExpression = mi;
		mi.Body = new ElaMatch(t);
		scanner.InjectBlock(t.col);				
		
		FunBodyExpr(mi);
		ProcessFunctionParameters(mi, ot, varExp); 
	}

	void BindingBodyGuards(ElaBinding varExp) {
		var gexp = default(ElaExpression);
		var cexp3 = default(ElaExpression);
		var cond = new ElaCondition(t);
		varExp.InitExpression = cond;
		
		Expect(20);
		BindingGuard(out gexp);
		cond.Condition = gexp; 
		Expect(49);
		Expr(out cexp3);
		cond.True = cexp3; 
		if (la.kind == 20) {
			BindingGuardList(ref cond);
		}
	}

	void BindingBodyInit(ElaBinding varExp) {
		var cexp = default(ElaExpression); 
		var cexp2 = default(ElaExpression);
		
		Expect(49);
		if (StartOf(1)) {
			Expr(out cexp);
		} else if (la.kind == 55) {
			Get();
			Expect(1);
			cexp = new ElaBuiltin(t) { Kind = Builtins.Kind(t.val) }; 
		} else SynErr(97);
		varExp.InitExpression = cexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp2);
			var b = (ElaBinding)cexp2;
			b.In = cexp; 
			varExp.InitExpression = cexp2;
			
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
		ProcessFunctionParameters(mi, ot, varExp); 
		
	}

	void BindingBodyOperator(ElaBinding varExp) {
		Operators();
		varExp.VariableName = t.val; 
		if (StartOf(4)) {
			FunExpr(varExp);
			var fun = (ElaFunctionLiteral)varExp.InitExpression;
			fun.FunctionType = ElaFunctionType.Operator;
			
		} else if (la.kind == 49) {
			BindingBodyInit(varExp);
			if (varExp.InitExpression.Type == ElaNodeType.FunctionLiteral)
			((ElaFunctionLiteral)varExp.InitExpression).FunctionType = ElaFunctionType.Operator;
			
		} else if (la.kind == 20) {
			BindingBodyGuards(varExp);
		} else SynErr(98);
	}

	void InfixFunBodyExprFirst(ElaPattern pat, ElaFunctionLiteral fun) {
		var ot = t;
		var match = fun.Body;
		var cexp = default(ElaExpression);			
		var entry = new ElaMatchEntry(t);
		match.Entries.Add(entry);
		entry.Pattern = pat;
		
		InfixFunName(fun);
		if (StartOf(4)) {
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
		Expect(49);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(3)) {
			if (StartOf(4)) {
				scanner.InjectBlock(); 
				InfixFunBodyExpr(fun);
			} else {
				InfixChildFunBodyExpr(fun);
			}
		}
	}

	void InfixFunName(ElaFunctionLiteral fun) {
		var name = String.Empty; 
		if (la.kind == 56) {
			Get();
			Expect(1);
			name = t.val; 
			Expect(56);
		} else if (StartOf(12)) {
			Operators();
			name = t.val; 
		} else SynErr(99);
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
		if (StartOf(4)) {
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
		Expect(49);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(3)) {
			if (StartOf(4)) {
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
		Expect(49);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(3)) {
			if (StartOf(4)) {
				scanner.InjectBlock(); 
				InfixFunBodyExpr(fun);
			} else {
				InfixChildFunBodyExpr(fun);
			}
		}
	}

	void FunName(ElaFunctionLiteral fun) {
		string val = ""; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (StartOf(12)) {
			Operators();
			val = t.val; 
		} else if (la.kind == 46) {
			Get();
			Operators();
			val = t.val; 
			Expect(47);
		} else SynErr(100);
		if (val != fun.Name)
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
		while (StartOf(4)) {
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
		Expect(49);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(14)) {
			if (StartOf(15)) {
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
		Expect(49);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(14)) {
			if (StartOf(15)) {
				scanner.InjectBlock(); 
				FunName(fun);
				FunBodyExpr(fun);
			} else {
				ChildFunBodyExpr(fun);
			}
		}
	}

	void LambdaExpr(out ElaExpression exp) {
		while (!(la.kind == 0 || la.kind == 22)) {SynErr(101); Get();}
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
		while (StartOf(4)) {
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
		while (!(la.kind == 0 || la.kind == 31)) {SynErr(102); Get();}
		Expect(31);
		var inc = default(ElaModuleInclude); 
		var par = default(ElaModuleInclude); 
		
		IncludeStatElement(out inc);
		exp = inc; par = inc; 
		while (la.kind == 1 || la.kind == 5 || la.kind == 42) {
			IncludeStatElement(out inc);
			par.And = inc; par = inc; 
		}
		EndBlock();
	}

	void IncludeStatElement(out ElaModuleInclude exp) {
		exp = null; 
		var inc = new ElaModuleInclude(t); 
		if (la.kind == 42) {
			Get();
			inc.RequireQuailified = true; 
		}
		Qualident(inc.Path);
		var name = inc.Path[inc.Path.Count - 1];				
		inc.Path.RemoveAt(inc.Path.Count - 1);				
		inc.Alias = inc.Name = name;
		exp = inc;
		
		if (la.kind == 57) {
			Get();
			if (la.kind == 1) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 5) {
				Get();
				inc.DllName = ReadString(t.val); 
			} else SynErr(103);
		}
		if (la.kind == 27) {
			Get();
			Expect(1);
			inc.Alias = t.val; 
		}
		if (la.kind == 46) {
			var imp = default(ElaImportedVariable); 
			Get();
			ImportName(out imp);
			inc.ImportList.Add(imp); 
			while (la.kind == 48) {
				Get();
				ImportName(out imp);
				inc.ImportList.Add(imp); 
			}
			Expect(47);
		}
	}

	void Qualident(List<String> path ) {
		var val = String.Empty; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 5) {
			Get();
			val = ReadString(t.val); 
		} else SynErr(104);
		path.Add(val); 
		if (la.kind == 24) {
			Get();
			Qualident(path);
		}
	}

	void ImportName(out ElaImportedVariable imp) {
		imp = new ElaImportedVariable(t); 
		if (la.kind == 30) {
			Get();
			imp.Private = true; 
		}
		Expect(1);
		imp.Name = imp.LocalName = t.val; 
		if (la.kind == 49) {
			Get();
			Expect(1);
			imp.Name = t.val; 
		}
	}

	void IfExpr(out ElaExpression exp) {
		Expect(33);
		var cond = new ElaCondition(t); 
		var cexp = default(ElaExpression);	
		exp = cond;
		
		Expr(out cexp);
		cond.Condition = cexp; 
		Expect(35);
		Expr(out cexp);
		cond.True = cexp; 
		ExpectWeak(34, 8);
		Expr(out cexp);
		cond.False = cexp; 
	}

	void RaiseExpr(out ElaExpression exp) {
		Expect(36);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		var code = String.Empty;
		
		Expect(2);
		code = t.val; 
		if (StartOf(1)) {
			Expr(out cexp);
		} else if (StartOf(16)) {
		} else SynErr(105);
		r.ErrorCode = code;
		r.Expression = cexp; 
		
	}

	void FailExpr(out ElaExpression exp) {
		Expect(40);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		
		Expr(out cexp);
		r.Expression = cexp; 
		r.ErrorCode = "Failure"; 
		
	}

	void TryExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		Expect(37);
		var ot = t;
		var match = new ElaTry(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(32);
		scanner.InjectBlock(); 
		MatchEntry(match);
		EndBlock();
		while (StartOf(4)) {
			scanner.InjectBlock(); 
			MatchEntry(match);
			EndBlock();
		}
		EndBlock();
	}

	void OrExpr(out ElaExpression exp) {
		AndExpr(out exp);
		while (la.kind == 58) {
			var cexp = default(ElaExpression); 
			Get();
			AndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanOr, Right = cexp }; 
			
		}
	}

	void AndExpr(out ElaExpression exp) {
		OpExpr1(out exp);
		while (la.kind == 59) {
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
		
		if (StartOf(17)) {
			OpExpr1b(out exp);
			while (la.kind == 7) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(17)) {
					OpExpr1b(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 7) {
			Get();
			op = t.val; 
			if (StartOf(17)) {
				OpExpr1b(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = op };
			
		} else SynErr(106);
	}

	void OpExpr1b(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(18)) {
			OpExpr2(out exp);
			while (la.kind == 15) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(17)) {
					OpExpr1b(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 15) {
			Get();
			op = t.val; 
			if (StartOf(18)) {
				OpExpr2(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(107);
	}

	void OpExpr2(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(19)) {
			OpExpr3(out exp);
			while (la.kind == 8) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(19)) {
					OpExpr3(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 8) {
			Get();
			op = t.val; 
			if (StartOf(19)) {
				OpExpr3(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(108);
	}

	void OpExpr3(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(20)) {
			OpExpr4(out exp);
			while (la.kind == 9 || la.kind == 51) {
				var cexp = default(ElaExpression); 
				if (la.kind == 9) {
					Get();
				} else {
					Get();
				}
				op = t.val; 
				if (StartOf(19)) {
					OpExpr3(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 9 || la.kind == 51) {
			if (la.kind == 9) {
				Get();
			} else {
				Get();
			}
			op = t.val; 
			if (StartOf(20)) {
				OpExpr4(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(109);
	}

	void OpExpr4(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(21)) {
			OpExpr5(out exp);
			while (la.kind == 10) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(21)) {
					OpExpr5(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 10) {
			Get();
			op = t.val; 
			if (StartOf(21)) {
				OpExpr5(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(110);
	}

	void OpExpr5(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(22)) {
			CastExpr(out exp);
			while (la.kind == 11) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(22)) {
					CastExpr(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 11) {
			Get();
			op = t.val; 
			if (StartOf(22)) {
				CastExpr(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(111);
	}

	void CastExpr(out ElaExpression exp) {
		InfixExpr(out exp);
		while (la.kind == 28) {
			var pat = default(ElaPattern); 
			Get();
			IsOperatorPattern(out pat);
			exp = new ElaIs(t) { Expression = exp, Pattern = pat }; 
		}
	}

	void InfixExpr(out ElaExpression exp) {
		exp = null;
		var ot = t; 
		var funexp = default(ElaExpression);
		
		if (StartOf(23)) {
			OpExpr6(out exp);
			while (la.kind == 56) {
				var cexp = default(ElaExpression); 
				ot = t;
				
				Get();
				Literal(out funexp);
				Expect(56);
				if (StartOf(23)) {
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
		} else if (la.kind == 56) {
			Get();
			Literal(out funexp);
			Expect(56);
			if (StartOf(23)) {
				OpExpr6(out exp);
				exp = GetPrefixFun(funexp, exp, true);	
			}
			if (exp == null)
			exp = funexp;
			
		} else SynErr(112);
	}

	void OpExpr6(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(24)) {
			OpExpr7(out exp);
			while (la.kind == 12) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(23)) {
					OpExpr6(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 12) {
			Get();
			op = t.val; 
			if (StartOf(24)) {
				OpExpr7(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(113);
	}

	void OpExpr7(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(25)) {
			OpExpr8(out exp);
			while (la.kind == 13) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(25)) {
					OpExpr8(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 13) {
			Get();
			op = t.val; 
			if (StartOf(25)) {
				OpExpr8(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(114);
	}

	void OpExpr8(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(26)) {
			Application(out exp);
			while (la.kind == 14) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(26)) {
					Application(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 14) {
			Get();
			op = t.val; 
			if (StartOf(26)) {
				Application(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(115);
	}

	void Application(out ElaExpression exp) {
		exp = null; 
		AccessExpr(out exp);
		var ot = t;
		var mi = default(ElaFunctionCall); 
		var cexp = default(ElaExpression);
		
		while (StartOf(26)) {
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
		while (la.kind == 24) {
			Get();
			if (la.kind == 46) {
				Get();
				if (la.kind == 1) {
					Get();
				} else if (StartOf(12)) {
					Operators();
				} else SynErr(116);
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
				Expect(47);
			} else if (la.kind == 1) {
				Get();
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
			} else SynErr(117);
		}
	}

	void EmbExpr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 1: case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13: case 14: case 15: case 16: case 18: case 38: case 39: case 45: case 46: case 51: case 53: case 56: {
			BinaryExpr(out exp);
			break;
		}
		case 33: {
			IfExpr(out exp);
			break;
		}
		case 22: {
			LambdaExpr(out exp);
			break;
		}
		case 36: {
			RaiseExpr(out exp);
			break;
		}
		case 40: {
			FailExpr(out exp);
			break;
		}
		case 26: {
			MatchExpr(out exp);
			break;
		}
		case 37: {
			TryExpr(out exp);
			break;
		}
		case 61: {
			LazyExpr(out exp);
			break;
		}
		default: SynErr(118); break;
		}
	}

	void LazyExpr(out ElaExpression exp) {
		Expect(61);
		var lazy = new ElaLazyLiteral(t); 
		Expr(out exp);
		lazy.Expression = exp;
		exp = lazy;
		
	}

	void WhereExpr(ElaExpression cexp, out ElaExpression exp) {
		WhereBinding(out exp);
		((ElaBinding)exp).In = cexp;
		
	}

	void ComprehensionEntry(ElaExpression body, out ElaGenerator it) {
		it = new ElaGenerator(t);
		it.Body = body;
		var cexp = default(ElaExpression);
		var pat = default(ElaPattern);
		
		GeneratorPattern(out pat);
		Expect(62);
		Expr(out cexp);
		it.Pattern = pat;
		it.Target = cexp;
		
		if (la.kind == 20 || la.kind == 48) {
			if (la.kind == 48) {
				var cit = default(ElaGenerator); 
				Get();
				ComprehensionEntry(body, out cit);
				it.Body = cit; 
			} else {
				Guard(out cexp);
				it.Guard = cexp; 
				if (la.kind == 48) {
					var cit = default(ElaGenerator); 
					Get();
					ComprehensionEntry(body, out cit);
					it.Body = cit; 
				}
			}
		}
	}

	void Ela() {
		var b = new ElaBlock(t);
		Expression = b;
		
		DeclarationBlock(b);
		while (StartOf(27)) {
			DeclarationBlock(b);
		}
	}

	void DeclarationBlock(ElaBlock b) {
		var exp = default(ElaExpression); 
		switch (la.kind) {
		case 29: {
			RootLetBinding(out exp);
			break;
		}
		case 31: {
			IncludeStat(out exp);
			break;
		}
		case 63: {
			TypeClass(out exp);
			break;
		}
		case 64: {
			ClassInstance(out exp);
			break;
		}
		case 65: {
			NewType(out exp);
			break;
		}
		case 1: case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13: case 14: case 15: case 16: case 18: case 22: case 26: case 33: case 36: case 37: case 38: case 39: case 40: case 45: case 46: case 51: case 53: case 56: case 61: {
			scanner.InjectBlock(); 
			EmbExpr(out exp);
			if (la.kind == 41) {
				WhereExpr(exp, out exp);
			}
			EndBlock();
			break;
		}
		default: SynErr(119); break;
		}
		b.Expressions.Add(exp); 
	}

	void TypeClass(out ElaExpression exp) {
		exp = null; 
		scanner.InjectBlock();
		
		Expect(63);
		var tc = new ElaTypeClass(t);
		exp = tc;
		
		if (la.kind == 1) {
			Get();
		} else if (la.kind == 2) {
			Get();
		} else SynErr(120);
		var nm = default(String);
		tc.Name = t.val;
		
		if (la.kind == 55) {
			Get();
			if (la.kind == 1) {
				Get();
			} else if (la.kind == 2) {
				Get();
			} else SynErr(121);
			tc.BuiltinName = t.val; 
			while (StartOf(15)) {
				if (la.kind == 1) {
					Get();
					nm=t.val; 
				} else if (StartOf(12)) {
					Operators();
					nm=t.val; 
				} else {
					Get();
					if (StartOf(12)) {
						Operators();
					} else if (la.kind == 1) {
						Get();
					} else SynErr(122);
					nm=t.val; 
					Expect(47);
				}
				tc.Members.Add(new ElaClassMember(t) { Name = nm });
				
			}
		} else if (la.kind == 1) {
			var targ = String.Empty; 
			Get();
			targ = t.val; 
			Expect(41);
			if (la.kind == 1) {
				Get();
				nm=t.val; 
			} else if (StartOf(12)) {
				Operators();
				nm=t.val; 
			} else if (la.kind == 46) {
				Get();
				if (StartOf(12)) {
					Operators();
				} else if (la.kind == 1) {
					Get();
				} else SynErr(123);
				nm=t.val; 
				Expect(47);
			} else SynErr(124);
			Expect(51);
			var count = 0;
			var mask = 0;
			
			if (la.kind == 1) {
				Get();
			} else if (la.kind == 45) {
				Get();
			} else SynErr(125);
			BuildMask(ref count, ref mask, t.val, targ); 
			while (la.kind == 21) {
				Get();
				if (la.kind == 1) {
					Get();
				} else if (la.kind == 45) {
					Get();
				} else SynErr(126);
				BuildMask(ref count, ref mask, t.val, targ); 
			}
			tc.Members.Add(new ElaClassMember(t) { Name = nm, Arguments = count, Mask = mask });
			
			while (la.kind == 43) {
				Get();
				if (la.kind == 1) {
					Get();
					nm=t.val; 
				} else if (StartOf(12)) {
					Operators();
					nm=t.val; 
				} else if (la.kind == 46) {
					Get();
					if (StartOf(12)) {
						Operators();
					} else if (la.kind == 1) {
						Get();
					} else SynErr(127);
					nm=t.val; 
					Expect(47);
				} else SynErr(128);
				Expect(51);
				count = 0;
				mask = 0;
				
				if (la.kind == 1) {
					Get();
				} else if (la.kind == 45) {
					Get();
				} else SynErr(129);
				BuildMask(ref count, ref mask, t.val, targ); 
				while (la.kind == 21) {
					Get();
					if (la.kind == 1) {
						Get();
					} else if (la.kind == 45) {
						Get();
					} else SynErr(130);
					BuildMask(ref count, ref mask, t.val, targ); 
				}
				tc.Members.Add(new ElaClassMember(t) { Name = nm, Arguments = count, Mask = mask });
				
			}
		} else SynErr(131);
		EndBlock();
	}

	void ClassInstance(out ElaExpression exp) {
		exp = null; 
		var fst0 = default(String);
		var snd0 = default(String);
		var fst = default(String);
		var snd = default(String);
		
		Expect(64);
		var ci = new ElaClassInstance(t);
		exp = ci;
		
		if (la.kind == 1) {
			Get();
		} else if (la.kind == 2) {
			Get();
		} else SynErr(132);
		fst0 = t.val; 
		if (la.kind == 24) {
			Get();
			if (la.kind == 1) {
				Get();
			} else if (la.kind == 2) {
				Get();
			} else SynErr(133);
			snd0 = t.val; 
		}
		if (fst0 != null && snd0 != null)
		{
		    ci.TypeClassPrefix = fst0;
		    ci.TypeClassName = snd0;
		}
		else
		    ci.TypeClassName = fst0;
		
		Expect(1);
		fst = t.val; 
		if (la.kind == 24) {
			Get();
			Expect(1);
			snd = t.val; 
		}
		if (fst != null && snd != null)
		{
		    ci.TypePrefix = fst;
		    ci.TypeName = snd;
		}
		else
		    ci.TypeName = fst;
		
		var cexp = default(ElaExpression);
		
		if (la.kind == 41) {
			WhereBinding(out cexp);
			ci.Where = (ElaBinding)cexp; 
		}
	}

	void NewType(out ElaExpression exp) {
		exp = null; 
		scanner.InjectBlock();
		
		Expect(65);
		Expect(1);
		var nt = new ElaNewtype(t) { Name = t.val };
		exp = nt;
		var pat = default(ElaPattern);
		
		if (StartOf(4)) {
			RootPattern(out pat);
			var m = new ElaMatch(t);
			nt.Body = m;
			var cexp = default(ElaExpression);
			
			Expect(49);
			Expr(out cexp);
			if (la.kind == 41) {
				WhereExpr(cexp, out cexp);
			}
			m.Entries.Add(new ElaMatchEntry { Pattern = pat, Expression = cexp }); 
		}
		EndBlock();
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Ela();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,T,x, x,T,x,T, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,T,x, x,T,x,x, x,T,x,x, T,T,T,T, T,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,x, x,T,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,T,x, x,x,x,x, x,T,x,x, T,T,T,T, T,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,x, x,T,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,T, T,T,T,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,T,x, x,T,x,T, x,T,x,x, T,T,T,T, T,T,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,x, x,T,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,T,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,T,T, T,T,T,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,T, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,T, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,T,x, x,T,x,T, x,x,x,x, x,x,T,T, x,T,x,x, x,T,T,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,x,T, T,T,T,T, T,T,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,x,T, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,T, x,T,x,x, x,x,x,x, T,x,T,T, x,x,x,x, x,T,x,T, T,x,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, T,T,T,T, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,T,T,T, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,x,T,T, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,x,x,T, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,x,x,x, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,x,x,x, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,x,x,x, x,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,x,x,x, x,x,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,T,x, x,T,x,T, x,T,x,x, T,T,T,T, T,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,x, x,T,x,T, T,T,x,x}

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
			case 3: s = "intTok expected"; break;
			case 4: s = "realTok expected"; break;
			case 5: s = "stringTok expected"; break;
			case 6: s = "charTok expected"; break;
			case 7: s = "operatorTok1 expected"; break;
			case 8: s = "operatorTok2 expected"; break;
			case 9: s = "operatorTok3 expected"; break;
			case 10: s = "operatorTok4 expected"; break;
			case 11: s = "operatorTok5 expected"; break;
			case 12: s = "operatorTok6 expected"; break;
			case 13: s = "operatorTok7 expected"; break;
			case 14: s = "operatorTok8 expected"; break;
			case 15: s = "operatorTok9 expected"; break;
			case 16: s = "LBRA expected"; break;
			case 17: s = "RBRA expected"; break;
			case 18: s = "LILB expected"; break;
			case 19: s = "LIRB expected"; break;
			case 20: s = "PIPE expected"; break;
			case 21: s = "ARROW expected"; break;
			case 22: s = "LAMBDA expected"; break;
			case 23: s = "COMPO expected"; break;
			case 24: s = "DOT expected"; break;
			case 25: s = "IN expected"; break;
			case 26: s = "MATCH expected"; break;
			case 27: s = "ASAMP expected"; break;
			case 28: s = "IS expected"; break;
			case 29: s = "LET expected"; break;
			case 30: s = "PRIVATE expected"; break;
			case 31: s = "OPEN expected"; break;
			case 32: s = "WITH expected"; break;
			case 33: s = "IFS expected"; break;
			case 34: s = "ELSE expected"; break;
			case 35: s = "THEN expected"; break;
			case 36: s = "RAISE expected"; break;
			case 37: s = "TRY expected"; break;
			case 38: s = "TRUE expected"; break;
			case 39: s = "FALSE expected"; break;
			case 40: s = "FAIL expected"; break;
			case 41: s = "WHERE expected"; break;
			case 42: s = "QUALIFIED expected"; break;
			case 43: s = "ET expected"; break;
			case 44: s = "EBLOCK expected"; break;
			case 45: s = "\"_\" expected"; break;
			case 46: s = "\"(\" expected"; break;
			case 47: s = "\")\" expected"; break;
			case 48: s = "\",\" expected"; break;
			case 49: s = "\"=\" expected"; break;
			case 50: s = "\"?\" expected"; break;
			case 51: s = "\"::\" expected"; break;
			case 52: s = "\"..\" expected"; break;
			case 53: s = "\"[&\" expected"; break;
			case 54: s = "\"inline\" expected"; break;
			case 55: s = "\"__internal\" expected"; break;
			case 56: s = "\"`\" expected"; break;
			case 57: s = "\"#\" expected"; break;
			case 58: s = "\"or\" expected"; break;
			case 59: s = "\"and\" expected"; break;
			case 60: s = "\"$\" expected"; break;
			case 61: s = "\"&\" expected"; break;
			case 62: s = "\"<-\" expected"; break;
			case 63: s = "\"class\" expected"; break;
			case 64: s = "\"instance\" expected"; break;
			case 65: s = "\"type\" expected"; break;
			case 66: s = "??? expected"; break;
			case 67: s = "invalid Literal"; break;
			case 68: s = "invalid Primitive"; break;
			case 69: s = "invalid ListLiteral"; break;
			case 70: s = "invalid VariableReference"; break;
			case 71: s = "invalid Expr"; break;
			case 72: s = "this symbol not expected in MatchExpr"; break;
			case 73: s = "invalid Guard"; break;
			case 74: s = "this symbol not expected in WhereBinding"; break;
			case 75: s = "invalid OrPattern"; break;
			case 76: s = "invalid AsPattern"; break;
			case 77: s = "invalid FuncPattern"; break;
			case 78: s = "invalid FuncPattern2"; break;
			case 79: s = "invalid SinglePattern"; break;
			case 80: s = "invalid LiteralPattern"; break;
			case 81: s = "invalid TypeCheckPattern"; break;
			case 82: s = "invalid TypeCheckPattern"; break;
			case 83: s = "invalid Operators"; break;
			case 84: s = "invalid BindingPattern"; break;
			case 85: s = "invalid BindingPattern"; break;
			case 86: s = "invalid GeneratorPattern"; break;
			case 87: s = "invalid IsOperatorPattern"; break;
			case 88: s = "invalid FieldPattern"; break;
			case 89: s = "invalid RecordField"; break;
			case 90: s = "invalid ParamList"; break;
			case 91: s = "this symbol not expected in LetBinding"; break;
			case 92: s = "invalid VariableAttributes"; break;
			case 93: s = "invalid BindingBody"; break;
			case 94: s = "this symbol not expected in RootLetBinding"; break;
			case 95: s = "invalid RootLetBinding"; break;
			case 96: s = "invalid BindingGuardList"; break;
			case 97: s = "invalid BindingBodyInit"; break;
			case 98: s = "invalid BindingBodyOperator"; break;
			case 99: s = "invalid InfixFunName"; break;
			case 100: s = "invalid FunName"; break;
			case 101: s = "this symbol not expected in LambdaExpr"; break;
			case 102: s = "this symbol not expected in IncludeStat"; break;
			case 103: s = "invalid IncludeStatElement"; break;
			case 104: s = "invalid Qualident"; break;
			case 105: s = "invalid RaiseExpr"; break;
			case 106: s = "invalid OpExpr1"; break;
			case 107: s = "invalid OpExpr1b"; break;
			case 108: s = "invalid OpExpr2"; break;
			case 109: s = "invalid OpExpr3"; break;
			case 110: s = "invalid OpExpr4"; break;
			case 111: s = "invalid OpExpr5"; break;
			case 112: s = "invalid InfixExpr"; break;
			case 113: s = "invalid OpExpr6"; break;
			case 114: s = "invalid OpExpr7"; break;
			case 115: s = "invalid OpExpr8"; break;
			case 116: s = "invalid AccessExpr"; break;
			case 117: s = "invalid AccessExpr"; break;
			case 118: s = "invalid EmbExpr"; break;
			case 119: s = "invalid DeclarationBlock"; break;
			case 120: s = "invalid TypeClass"; break;
			case 121: s = "invalid TypeClass"; break;
			case 122: s = "invalid TypeClass"; break;
			case 123: s = "invalid TypeClass"; break;
			case 124: s = "invalid TypeClass"; break;
			case 125: s = "invalid TypeClass"; break;
			case 126: s = "invalid TypeClass"; break;
			case 127: s = "invalid TypeClass"; break;
			case 128: s = "invalid TypeClass"; break;
			case 129: s = "invalid TypeClass"; break;
			case 130: s = "invalid TypeClass"; break;
			case 131: s = "invalid TypeClass"; break;
			case 132: s = "invalid ClassInstance"; break;
			case 133: s = "invalid ClassInstance"; break;

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
