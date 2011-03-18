
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
	public const int _LBRA = 10;
	public const int _RBRA = 11;
	public const int _LILB = 12;
	public const int _LIRB = 13;
	public const int _PIPE = 14;
	public const int _ARROW = 15;
	public const int _LAMBDA = 16;
	public const int _EQ = 17;
	public const int _SEQ = 18;
	public const int _MINUS = 19;
	public const int _COMPH = 20;
	public const int _COMPO = 21;
	public const int _DOT = 22;
	public const int _IN = 23;
	public const int _BASE = 24;
	public const int _MATCH = 25;
	public const int _ASAMP = 26;
	public const int _IS = 27;
	public const int _LET = 28;
	public const int _PRIVATE = 29;
	public const int _OPEN = 30;
	public const int _WITH = 31;
	public const int _IFS = 32;
	public const int _ELSE = 33;
	public const int _THEN = 34;
	public const int _RAISE = 35;
	public const int _TRY = 36;
	public const int _TRUE = 37;
	public const int _FALSE = 38;
	public const int _FAIL = 39;
	public const int _WHERE = 40;
	public const int _ET = 41;
	public const int _EBLOCK = 42;
	public const int maxT = 83;

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
		Expect(42);
		if (!t.virt) scanner.PopIndent(); 
	}

	void Literal(out ElaExpression exp) {
		exp = null; 
		if (StartOf(1)) {
			Primitive(out exp);
		} else if (la.kind == 10) {
			RecordLiteral(out exp);
		} else if (la.kind == 12) {
			ListLiteral(out exp);
		} else if (la.kind == 44) {
			TupleLiteral(out exp);
		} else if (StartOf(2)) {
			SimpleExpr(out exp);
		} else SynErr(84);
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
		case 37: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(true) }; 
			break;
		}
		case 38: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(false) }; 
			break;
		}
		default: SynErr(85); break;
		}
	}

	void RecordLiteral(out ElaExpression exp) {
		exp = null; 
		var fld = default(ElaFieldDeclaration);
		
		Expect(10);
		var rec = new ElaRecordLiteral(t); exp = rec; 
		RecordField(out fld);
		rec.Fields.Add(fld); 
		while (la.kind == 46) {
			Get();
			RecordField(out fld);
			rec.Fields.Add(fld); 
		}
		Expect(11);
	}

	void ListLiteral(out ElaExpression exp) {
		Expect(12);
		var list = default(List<ElaExpression>);
		var comp = default(ElaComprehension);
		var rng = default(ElaRange);
		var ot = t;
		
		exp = null;
		
		if (StartOf(3)) {
			if (StartOf(4)) {
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
		
		Expect(13);
	}

	void TupleLiteral(out ElaExpression exp) {
		var ot = default(Token);
		exp = null; 
		
		Expect(44);
		ot = t; 
		if (StartOf(4)) {
			GroupExpr(out exp);
		}
		Expect(45);
		if (exp == null)
		exp = new ElaUnitLiteral(ot);
		
	}

	void SimpleExpr(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 3) {
			ArgumentReference(out exp);
		} else if (la.kind == 24) {
			BaseReference(out exp);
		} else if (la.kind == 1 || la.kind == 43) {
			VariableReference(out exp);
		} else if (la.kind == 82) {
			LazyExpr(out exp);
		} else SynErr(86);
	}

	void ArgumentReference(out ElaExpression exp) {
		Expect(3);
		exp = new ElaArgument(t) { ArgumentName = t.val.Substring(1) }; 
	}

	void BaseReference(out ElaExpression exp) {
		var baseRef = default(ElaBaseReference);
		exp = null;
		
		Expect(24);
		baseRef = new ElaBaseReference(t); 
		Expect(22);
		var ot = t; 
		Expect(1);
		exp = new ElaFieldReference(ot) { FieldName = t.val, TargetObject = baseRef }; 
	}

	void VariableReference(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = GetBuiltin(t, t.val); 
		} else if (la.kind == 43) {
			Get();
			exp = new ElaPlaceholder(t); 
		} else SynErr(87);
	}

	void LazyExpr(out ElaExpression exp) {
		Expect(82);
		var lazy = new ElaLazyLiteral(t); 
		Expr(out exp);
		var m = new ElaMatch(t);
		m.Entries.Add(new ElaMatchEntry { Pattern = new ElaUnitPattern(), Expression = exp });
		lazy.Body = m;
		exp = lazy;
		
		Expect(45);
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
		var ot = t;
		
		Expr(out exp);
		if (la.kind == 46) {
			var tuple = new ElaTupleLiteral(ot); 
			tuple.Parameters.Add(exp);
			exp = tuple; 
			
			Get();
			if (StartOf(4)) {
				Expr(out cexp);
				tuple.Parameters.Add(cexp); 
			}
			while (la.kind == 46) {
				Get();
				Expr(out cexp);
				tuple.Parameters.Add(cexp); 
			}
		}
	}

	void Expr(out ElaExpression exp) {
		exp = null; 
		if (StartOf(6)) {
			EmbExpr(out exp);
		} else if (la.kind == 28) {
			LetBinding(out exp);
		} else SynErr(88);
	}

	void MatchExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		Expect(25);
		var match = new ElaMatch(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(31);
		MatchEntry(match);
		while (StartOf(7)) {
			if (StartOf(8)) {
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
		
		if (la.kind == 14) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(17);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 40) {
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
		Expect(17);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 40) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
	}

	void RootPattern(out ElaPattern pat) {
		OrPattern(out pat);
		if (la.kind == 50) {
			ConsPattern(pat, out pat);
		}
	}

	void Guard(out ElaExpression exp) {
		exp = null; 
		Expect(14);
		if (StartOf(9)) {
			BinaryExpr(out exp);
			while (la.kind == 46) {
				var old = exp; 
				Get();
				var ot = t; 
				BinaryExpr(out exp);
				exp = new ElaBinary(t) { Operator = ElaOperator.BooleanAnd, Left = old, Right = exp };
				
			}
		} else if (la.kind == 33) {
			Get();
			exp = new ElaOtherwiseGuard(t); 
		} else SynErr(89);
	}

	void WhereBinding(out ElaExpression exp) {
		scanner.InjectBlock(); 
		Expect(40);
		BindingBody(ElaVariableFlags.None, out exp);
		EndBlock();
	}

	void BinaryExpr(out ElaExpression exp) {
		AssignExpr(out exp);
		while (la.kind == 18) {
			var cexp = default(ElaExpression); 
			Get();
			var ot = t; 
			AssignExpr(out cexp);
			exp = new ElaBinary(ot) { Left = exp, Right = cexp, Operator = ElaOperator.Sequence };
			
		}
	}

	void OrPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 2 || la.kind == 47) {
			VariantPattern(out pat);
		} else if (StartOf(10)) {
			AsPattern(out pat);
		} else SynErr(90);
	}

	void ConsPattern(ElaPattern prev, out ElaPattern exp) {
		var cexp = default(ElaPattern); 
		var ht = new ElaHeadTailPattern(t); 
		ht.Patterns.Add(prev);
		exp = ht;				
		
		Expect(50);
		AsPattern(out cexp);
		ht.Patterns.Add(cexp); 
		while (la.kind == 50) {
			Get();
			AsPattern(out cexp);
			ht.Patterns.Add(cexp); 
		}
	}

	void ParenPattern(out ElaPattern pat) {
		pat = null; 
		OrPattern(out pat);
		if (la.kind == 50) {
			ConsPattern(pat, out pat);
		}
		if (la.kind == 46) {
			TuplePattern(pat, out pat);
		}
	}

	void TuplePattern(ElaPattern prev, out ElaPattern pat) {
		var seq = new ElaTuplePattern(t); 
		seq.Patterns.Add(prev);
		var cpat = default(ElaPattern);
		pat = seq;
		
		Expect(46);
		if (StartOf(10)) {
			AsPattern(out cpat);
			if (la.kind == 50) {
				ConsPattern(cpat, out cpat);
			}
			seq.Patterns.Add(cpat); 
			while (la.kind == 46) {
				Get();
				AsPattern(out cpat);
				if (la.kind == 50) {
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
		} else if (la.kind == 47) {
			Get();
			vp = new ElaVariantPattern(t); 
			if (StartOf(10)) {
				AsPattern(out cpat);
				vp.Pattern = cpat; 
			}
			pat = vp; 
		} else SynErr(91);
	}

	void AsPattern(out ElaPattern pat) {
		pat = null; 
		SinglePattern(out pat);
		if (la.kind == 26) {
			Get();
			var asPat = new ElaAsPattern(t) { Pattern = pat }; 
			pat = asPat;				
			
			Expect(1);
			asPat.Name = t.val; 
		}
	}

	void FuncPattern(out ElaPattern pat) {
		AsPattern(out pat);
		if (la.kind == 50) {
			ConsPattern(pat, out pat);
		}
	}

	void SinglePattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 43: {
			DefaultPattern(out pat);
			break;
		}
		case 44: {
			UnitPattern(out pat);
			break;
		}
		case 5: case 6: case 7: case 8: case 37: case 38: case 51: {
			LiteralPattern(out pat);
			break;
		}
		case 12: {
			ListPattern(out pat);
			break;
		}
		case 10: {
			RecordPattern(out pat);
			break;
		}
		case 1: {
			IdentPattern(out pat);
			break;
		}
		case 48: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(92); break;
		}
	}

	void DefaultPattern(out ElaPattern pat) {
		Expect(43);
		pat = new ElaDefaultPattern(t); 
	}

	void UnitPattern(out ElaPattern pat) {
		var ot = t;
		pat = null;
		
		Expect(44);
		if (StartOf(8)) {
			ParenPattern(out pat);
		}
		Expect(45);
		if (pat == null)
		pat = new ElaUnitPattern(ot); 
		
	}

	void LiteralPattern(out ElaPattern pat) {
		var lit = default(ElaLiteralValue);
		pat = null;
		
		switch (la.kind) {
		case 7: {
			Get();
			lit = ParseString(t.val); 
			break;
		}
		case 8: {
			Get();
			lit = ParseChar(t.val); 
			break;
		}
		case 5: {
			Get();
			lit = ParseInt(t.val); 
			break;
		}
		case 6: {
			Get();
			lit = ParseReal(t.val); 
			break;
		}
		case 51: {
			Get();
			if (la.kind == 5) {
				Get();
				lit = ParseInt(t.val).MakeNegative(); 
			} else if (la.kind == 6) {
				Get();
				lit = ParseReal(t.val).MakeNegative(); 
			} else SynErr(93);
			break;
		}
		case 37: {
			Get();
			lit = new ElaLiteralValue(true); 
			break;
		}
		case 38: {
			Get();
			lit = new ElaLiteralValue(false); 
			break;
		}
		default: SynErr(94); break;
		}
		pat = new ElaLiteralPattern(t) { Value = lit };				
		
	}

	void ListPattern(out ElaPattern pat) {
		var cexp = default(ElaPattern); 
		var ht = default(ElaHeadTailPattern); 
		pat = null;
		
		Expect(12);
		if (StartOf(10)) {
			ht = new ElaHeadTailPattern(t);			
			
			AsPattern(out cexp);
			ht.Patterns.Add(cexp);  
			while (la.kind == 46) {
				Get();
				AsPattern(out cexp);
				ht.Patterns.Add(cexp); 
			}
			ht.Patterns.Add(new ElaNilPattern(t));
			pat = ht;
			
		}
		if (pat == null)
		pat = new ElaNilPattern(t);
		
		Expect(13);
	}

	void RecordPattern(out ElaPattern pat) {
		pat = null; 
		var cpat = default(ElaFieldPattern);
		
		Expect(10);
		var rec = new ElaRecordPattern(t); 
		pat = rec; 
		
		if (la.kind == 1 || la.kind == 7) {
			FieldPattern(out cpat);
			rec.Fields.Add(cpat); 
			while (la.kind == 46) {
				Get();
				FieldPattern(out cpat);
				rec.Fields.Add(cpat); 
			}
		}
		Expect(11);
	}

	void IdentPattern(out ElaPattern pat) {
		pat = null; 
		Expect(1);
		var name = t.val; 
		if (la.kind == 49) {
			Get();
			var cst = new ElaCastPattern(t) { VariableName = name }; pat = cst; 
			Expect(1);
			cst.TypeAffinity = GetType(t.val); 
		}
		if (pat == null)
		pat = new ElaVariablePattern(t) { Name = t.val }; 
		
	}

	void TypeCheckPattern(out ElaPattern pat) {
		var eis = new ElaIsPattern(t);
		var tmp = default(ElaTraits);
		pat = eis; 			
		
		Expect(48);
		if (la.kind == 1) {
			Get();
			eis.TypeAffinity = GetType(t.val); 
		} else if (la.kind == 44) {
			
			Get();
			Expect(1);
			tmp = Builtins.Trait(t.val); 
			
			if (tmp == ElaTraits.None) {
				AddError(ElaParserError.UnknownTrait, t.val);
			}
			
			eis.Traits |= tmp;
			
			while (la.kind == 46) {
				Get();
				Expect(1);
				tmp = Builtins.Trait(t.val); 
				
				if (tmp == ElaTraits.None) {
					AddError(ElaParserError.UnknownTrait, t.val);
				}
				
				eis.Traits |= tmp;
				
			}
			Expect(45);
		} else SynErr(95);
	}

	void BindingPattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 43: {
			DefaultPattern(out pat);
			break;
		}
		case 44: {
			UnitPattern(out pat);
			break;
		}
		case 5: case 6: case 7: case 8: case 37: case 38: case 51: {
			LiteralPattern(out pat);
			break;
		}
		case 12: {
			ListPattern(out pat);
			break;
		}
		case 10: {
			RecordPattern(out pat);
			break;
		}
		case 48: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(96); break;
		}
		if (la.kind == 26) {
			Get();
			var asPat = new ElaAsPattern(t) { Pattern = pat }; 
			pat = asPat;				
			
			Expect(1);
			asPat.Name = t.val; 
		}
	}

	void GeneratorPattern(out ElaPattern pat) {
		pat = null; string name = null; 
		if (la.kind == 1) {
			Get();
			name = t.val; 
			pat = new ElaVariablePattern(t) { Name = name }; 
		} else if (StartOf(11)) {
			LiteralPattern(out pat);
		} else if (la.kind == 10) {
			RecordPattern(out pat);
		} else if (la.kind == 12) {
			ListPattern(out pat);
		} else if (la.kind == 44) {
			UnitPattern(out pat);
		} else SynErr(97);
	}

	void IsOperatorPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 48) {
			TypeCheckPattern(out pat);
		} else if (StartOf(11)) {
			LiteralPattern(out pat);
		} else if (la.kind == 10) {
			RecordPattern(out pat);
		} else if (la.kind == 12) {
			ListPattern(out pat);
		} else if (la.kind == 44) {
			UnitPattern(out pat);
		} else SynErr(98);
	}

	void FieldPattern(out ElaFieldPattern fld) {
		fld = null;
		var cpat = default(ElaPattern);
		
		if (la.kind == 7) {
			Get();
			fld = new ElaFieldPattern(t) { Name = ReadString(t.val) }; 
			Expect(17);
			AsPattern(out cpat);
			fld.Value = cpat; 
		} else if (la.kind == 1) {
			Get();
			fld = new ElaFieldPattern(t) { Name = t.val }; 
			if (la.kind == 17) {
				Get();
				AsPattern(out cpat);
			}
			if (cpat == null)
			cpat = new ElaVariablePattern(t) { Name = fld.Name };
			
			fld.Value = cpat; 
			
		} else SynErr(99);
	}

	void RecordField(out ElaFieldDeclaration fld) {
		fld = null; 
		var cexp = default(ElaExpression);
		var mutable = false;
		
		if (la.kind == 52) {
			Get();
			mutable = true; 
		}
		if (la.kind == 1) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = t.val, Mutable = mutable }; 
			Expect(17);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else if (la.kind == 7) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = ReadString(t.val), Mutable = mutable }; 
			Expect(17);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else SynErr(100);
	}

	void RangeExpr(ElaExpression first, ElaExpression sec, out ElaRange rng) {
		rng = new ElaRange(t) { First = first, Second = sec };
		var cexp = default(ElaExpression);
		
		Expect(53);
		if (StartOf(4)) {
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
		if (la.kind == 21 || la.kind == 46 || la.kind == 53) {
			if (la.kind == 21) {
				ComprehensionExpr(exp, out comp);
			} else if (la.kind == 53) {
				RangeExpr(exp, null, out rng);
			} else {
				var oexp = exp; 
				Get();
				Expr(out exp);
				if (la.kind == 53) {
					RangeExpr(oexp, exp, out rng);
				} else if (la.kind == 13 || la.kind == 46) {
					list = new List<ElaExpression>();
					list.Add(oexp);
					list.Add(exp);
					
					while (la.kind == 46) {
						Get();
						Expr(out exp);
						list.Add(exp); 
					}
				} else SynErr(101);
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
		
		Expect(21);
		ComprehensionEntry(sel, out it);
		exp = new ElaComprehension(ot) { Generator = it }; 
	}

	void LetBinding(out ElaExpression exp) {
		exp = null; 
		var inExp = default(ElaExpression);
		var flags = default(ElaVariableFlags);
		
		Expect(28);
		if (la.kind == 29) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		Expect(23);
		Expr(out inExp);
		((ElaBinding)exp).In = inExp; 
	}

	void VariableAttributes(out ElaVariableFlags flags) {
		Expect(29);
		flags = ElaVariableFlags.Private; 
	}

	void BindingBody(ElaVariableFlags flags, out ElaExpression exp) {
		var varExp = new ElaBinding(t) { VariableFlags = flags }; 
		exp = varExp;
		var cexp = default(ElaExpression); 
		var pat = default(ElaPattern);
		
		if (la.kind == 1) {
			Get();
			varExp.VariableName = t.val; 
			if (StartOf(12)) {
				if (StartOf(10)) {
					FunExpr(varExp);
				} else if (la.kind == 14) {
					BindingBodyGuards(varExp);
				} else {
					BindingBodyInit(varExp);
				}
			}
			SetObjectMetadata(varExp, cexp); 
		} else if (StartOf(13)) {
			BindingPattern(out pat);
			varExp.Pattern = pat; 
			if (la.kind == 17) {
				BindingBodyInit(varExp);
			}
		} else if (la.kind == 9) {
			BindingBodyOperator(varExp);
		} else SynErr(102);
		if (la.kind == 41) {
			Get();
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
		Expect(28);
		if (la.kind == 29) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		if (la.kind == 23) {
			Get();
			Expr(out inExp);
			((ElaBinding)exp).In = inExp; 
			EndBlock();
		} else if (la.kind == 42) {
			EndBlock();
		} else SynErr(103);
	}

	void BindingGuard(out ElaExpression exp) {
		exp = null; 
		BinaryExpr(out exp);
		while (la.kind == 46) {
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
		
		Expect(14);
		if (StartOf(9)) {
			var newCond = new ElaCondition(t);
			cond.False = newCond;
			cond = newCond;
			
			BindingGuard(out gexp);
			cond.Condition = gexp; 
			Expect(17);
			Expr(out cexp);
			cond.True = cexp; 
			if (la.kind == 14) {
				BindingGuardList(ref cond);
			}
		} else if (la.kind == 33) {
			Get();
			Expect(17);
			Expr(out cexp);
			cond.False = cexp; 
		} else SynErr(104);
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
	}

	void BindingBodyGuards(ElaBinding varExp) {
		var gexp = default(ElaExpression);
		var cexp3 = default(ElaExpression);
		var cond = new ElaCondition(t);
		varExp.InitExpression = cond;
		
		Expect(14);
		BindingGuard(out gexp);
		cond.Condition = gexp; 
		Expect(17);
		Expr(out cexp3);
		cond.True = cexp3; 
		BindingGuardList(ref cond);
	}

	void BindingBodyInit(ElaBinding varExp) {
		var cexp = default(ElaExpression); 
		var cexp2 = default(ElaExpression);
		
		Expect(17);
		Expr(out cexp);
		varExp.InitExpression = cexp; 
		if (la.kind == 40) {
			WhereBinding(out cexp2);
			varExp.Where = (ElaBinding)cexp2; 
		}
	}

	void BindingBodyOperator(ElaBinding varExp) {
		Expect(9);
		varExp.VariableName = t.val; 
		FunExpr(varExp);
		var fun = (ElaFunctionLiteral)varExp.InitExpression;
		fun.FunctionType = ElaFunctionType.Operator;
		
	}

	void FunName(ElaFunctionLiteral fun) {
		if (la.kind == 1) {
			Get();
		} else if (la.kind == 9) {
			Get();
		} else SynErr(105);
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
		if (la.kind == 14) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(17);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 40) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (la.kind == 1 || la.kind == 9 || la.kind == 14) {
			if (la.kind == 1 || la.kind == 9) {
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
		Expect(17);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 40) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (la.kind == 1 || la.kind == 9 || la.kind == 14) {
			if (la.kind == 1 || la.kind == 9) {
				scanner.InjectBlock(); 
				FunName(fun);
				FunBodyExpr(fun);
			} else {
				ChildFunBodyExpr(fun);
			}
		}
	}

	void LambdaExpr(out ElaExpression exp) {
		Expect(16);
		var ot = t;
		var pat = default(ElaPattern);
		var seq = default(ElaPatternGroup);
		
		var mi = new ElaFunctionLiteral(t);
		exp = mi;
		var match = new ElaMatch(t);
		mi.Body = match;
		
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
		if (la.kind == 14) {
			var cexp = default(ElaExpression); 
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(15);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
	}

	void IncludeStat(out ElaExpression exp) {
		exp = null; 
		scanner.InjectBlock(); 
		Expect(30);
		var inc = new ElaModuleInclude(t); 
		Qualident(inc.Path);
		var name = inc.Path[inc.Path.Count - 1];				
		inc.Path.RemoveAt(inc.Path.Count - 1);				
		inc.Alias = inc.Name = name;
		exp = inc;
		
		if (la.kind == 55) {
			Get();
			if (la.kind == 1) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 7) {
				Get();
				inc.DllName = ReadString(t.val); 
			} else SynErr(106);
		}
		if (la.kind == 26) {
			Get();
			Expect(1);
			inc.Alias = t.val; 
		}
		if (la.kind == 31) {
			Get();
			IncludeImports(inc);
		}
		Expect(42);
	}

	void Qualident(List<String> path ) {
		var val = String.Empty; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 7) {
			Get();
			val = ReadString(t.val); 
		} else SynErr(107);
		path.Add(val); 
		if (la.kind == 22) {
			Get();
			Qualident(path);
		}
	}

	void IncludeImports(ElaModuleInclude inc) {
		Expect(1);
		var imp = new ElaImportedName(t) { LocalName = t.val, ExternalName = t.val }; 
		inc.Imports.Add(imp); 
		
		if (la.kind == 17) {
			Get();
			Expect(1);
			imp.ExternalName = t.val; 
		}
		if (la.kind == 46) {
			Get();
			IncludeImports(inc);
		}
	}

	void IfExpr(out ElaExpression exp) {
		Expect(32);
		var cond = new ElaCondition(t); 
		var cexp = default(ElaExpression);	
		exp = cond;
		
		Expr(out cexp);
		cond.Condition = cexp; 
		Expect(34);
		Expr(out cexp);
		cond.True = cexp; 
		ExpectWeak(33, 14);
		Expr(out cexp);
		cond.False = cexp; 
	}

	void RaiseExpr(out ElaExpression exp) {
		Expect(35);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		var code = String.Empty;
		
		Expect(1);
		code = t.val; 
		if (la.kind == 44) {
			Get();
			Expr(out cexp);
			Expect(45);
		}
		r.ErrorCode = code;
		r.Expression = cexp; 
		
	}

	void FailExpr(out ElaExpression exp) {
		Expect(39);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		
		Expr(out cexp);
		r.Expression = cexp; 
		r.ErrorCode = "Failure"; 
		
	}

	void TryExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		Expect(36);
		var ot = t;
		var match = new ElaTry(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(31);
		scanner.InjectBlock(); 
		MatchEntry(match);
		EndBlock();
		while (StartOf(8)) {
			scanner.InjectBlock(); 
			MatchEntry(match);
			EndBlock();
		}
		EndBlock();
	}

	void AssignExpr(out ElaExpression exp) {
		PartialBinaryExpr(out exp);
		while (la.kind == 56 || la.kind == 57) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 56) {
				Get();
				op = ElaOperator.Assign; 
			} else {
				Get();
				op = ElaOperator.Swap; 
			}
			PartialBinaryExpr(out cexp);
			exp = new ElaBinary(t) { Operator = op, Left = exp, Right = cexp }; 
		}
	}

	void PartialBinaryExpr(out ElaExpression exp) {
		exp = null; 
		if (StartOf(15)) {
			exp = null; 
			var ot = t;
			var op = ElaOperator.None;
			var opName = String.Empty;
			var funName = String.Empty;
			
			if (StartOf(16)) {
				FuncOperator(out op);
			} else if (la.kind == 9) {
				Get();
				opName = t.val; 
			} else {
				Get();
				funName = t.val.Trim('`'); 
			}
			if (StartOf(17)) {
				var nexp = default(ElaExpression); 
				BackwardPipeExpr(out nexp);
				if (op == ElaOperator.Negate || op == ElaOperator.BitwiseNot)
				exp = new ElaBinary(ot) { Left = nexp, Operator = op };
				else
					exp = 
						funName.Length != 0 ? GetPrefixFun(funName, nexp, true) :
						opName.Length != 0 ? GetCustomOperatorFun(opName, nexp) :
						GetOperatorFun(nexp, op, true);
				
			}
			if (exp == null)
			{
				if (opName.Length == 0 && funName.Length == 0)
					exp = new ElaBuiltinFunction(ot) { 
						Kind = ElaBuiltinFunctionKind.Operator,
						Operator = op
					};
				else 
					exp = new ElaCustomOperator(ot) { Operator = opName };
			}
			
		} else if (StartOf(17)) {
			BackwardPipeExpr(out exp);
		} else SynErr(108);
	}

	void FuncOperator(out ElaOperator op) {
		op = ElaOperator.None; 
		switch (la.kind) {
		case 71: {
			Get();
			op = ElaOperator.Add; 
			break;
		}
		case 19: {
			Get();
			op = ElaOperator.Subtract; 
			break;
		}
		case 72: {
			Get();
			op = ElaOperator.Multiply; 
			break;
		}
		case 73: {
			Get();
			op = ElaOperator.Divide; 
			break;
		}
		case 74: {
			Get();
			op = ElaOperator.Modulus; 
			break;
		}
		case 75: {
			Get();
			op = ElaOperator.Power; 
			break;
		}
		case 62: {
			Get();
			op = ElaOperator.Equals; 
			break;
		}
		case 63: {
			Get();
			op = ElaOperator.NotEquals; 
			break;
		}
		case 64: {
			Get();
			op = ElaOperator.Greater; 
			break;
		}
		case 65: {
			Get();
			op = ElaOperator.Lesser; 
			break;
		}
		case 67: {
			Get();
			op = ElaOperator.LesserEqual; 
			break;
		}
		case 66: {
			Get();
			op = ElaOperator.GreaterEqual; 
			break;
		}
		case 81: {
			Get();
			op = ElaOperator.CompForward; 
			break;
		}
		case 80: {
			Get();
			op = ElaOperator.CompBackward; 
			break;
		}
		case 50: {
			Get();
			op = ElaOperator.ConsList; 
			break;
		}
		case 61: {
			Get();
			op = ElaOperator.BooleanAnd; 
			break;
		}
		case 60: {
			Get();
			op = ElaOperator.BooleanOr; 
			break;
		}
		case 79: {
			Get();
			op = ElaOperator.BitwiseAnd; 
			break;
		}
		case 77: {
			Get();
			op = ElaOperator.BitwiseOr; 
			break;
		}
		case 78: {
			Get();
			op = ElaOperator.BitwiseXor; 
			break;
		}
		case 69: {
			Get();
			op = ElaOperator.ShiftLeft; 
			break;
		}
		case 68: {
			Get();
			op = ElaOperator.ShiftRight; 
			break;
		}
		case 70: {
			Get();
			op = ElaOperator.Concat; 
			break;
		}
		case 18: {
			Get();
			op = ElaOperator.Sequence; 
			break;
		}
		case 51: {
			Get();
			op = ElaOperator.Negate; 
			break;
		}
		case 76: {
			Get();
			op = ElaOperator.BitwiseNot; 
			break;
		}
		default: SynErr(109); break;
		}
	}

	void BackwardPipeExpr(out ElaExpression exp) {
		ForwardPipeExpr(out exp);
		while (la.kind == 58) {
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
		while (la.kind == 59) {
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
		while (la.kind == 60) {
			var cexp = default(ElaExpression); 
			Get();
			AndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanOr, Right = cexp }; 
			
		}
	}

	void AndExpr(out ElaExpression exp) {
		EqExpr(out exp);
		while (la.kind == 61) {
			var cexp = default(ElaExpression); 
			Get();
			EqExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanAnd, Right = cexp }; 
			
		}
	}

	void EqExpr(out ElaExpression exp) {
		ShiftExpr(out exp);
		while (StartOf(18)) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			switch (la.kind) {
			case 62: {
				Get();
				op = ElaOperator.Equals; 
				break;
			}
			case 63: {
				Get();
				op = ElaOperator.NotEquals; 
				break;
			}
			case 64: {
				Get();
				op = ElaOperator.Greater; 
				break;
			}
			case 65: {
				Get();
				op = ElaOperator.Lesser; 
				break;
			}
			case 66: {
				Get();
				op = ElaOperator.GreaterEqual; 
				break;
			}
			case 67: {
				Get();
				op = ElaOperator.LesserEqual; 
				break;
			}
			}
			if (StartOf(17)) {
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
		while (la.kind == 68 || la.kind == 69) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 68) {
				Get();
				op = ElaOperator.ShiftRight; 
			} else {
				Get();
				op = ElaOperator.ShiftLeft; 
			}
			if (StartOf(17)) {
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
		while (la.kind == 70) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			Get();
			op = ElaOperator.Concat; 
			if (StartOf(17)) {
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
		while (la.kind == 50) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			Get();
			op = ElaOperator.ConsList; 
			if (StartOf(17)) {
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
		while (la.kind == 19 || la.kind == 71) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 71) {
				Get();
				op = ElaOperator.Add; 
			} else {
				Get();
				op = ElaOperator.Subtract; 
			}
			if (StartOf(17)) {
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
		while (StartOf(19)) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 72) {
				Get();
				op = ElaOperator.Multiply; 
			} else if (la.kind == 73) {
				Get();
				op = ElaOperator.Divide; 
			} else if (la.kind == 74) {
				Get();
				op = ElaOperator.Modulus; 
			} else {
				Get();
				op = ElaOperator.Power; 
			}
			if (StartOf(17)) {
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
		while (la.kind == 20 || la.kind == 27 || la.kind == 49) {
			if (la.kind == 49) {
				Get();
				Expect(1);
				exp = new ElaCast(t) { CastAffinity = GetType(t.val), Expression = exp };
			} else if (la.kind == 27) {
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
		while (StartOf(20)) {
			var cexp = default(ElaExpression); 
			var op = ElaOperator.None;
			var name = String.Empty;
			var ot = t;
			
			if (la.kind == 9) {
				Get();
				op = ElaOperator.Custom; name = t.val; 
			} else if (la.kind == 4) {
				Get();
				name = t.val.Trim('`'); 
			} else if (la.kind == 51) {
				Get();
				op = ElaOperator.Negate; 
			} else {
				Get();
				op = ElaOperator.BitwiseNot; 
			}
			if (StartOf(17)) {
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
			else if (op != ElaOperator.Custom)
				exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			else
				exp = new ElaBinary(t) { CustomOperator = name, Left = exp, Operator = op, Right = cexp }; 
			
		}
	}

	void ComprehensionOpExpr(ElaExpression init, out ElaExpression exp) {
		var list = default(List<ElaExpression>);
		var comp = default(ElaComprehension);
		var rng = default(ElaRange);
		exp = null;
		
		Expect(12);
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
		
		Expect(13);
	}

	void BitOrExpr(out ElaExpression exp) {
		BitXorExpr(out exp);
		while (la.kind == 77) {
			var cexp = default(ElaExpression); 
			Get();
			if (StartOf(17)) {
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
		while (la.kind == 78) {
			var cexp = default(ElaExpression); 
			Get();
			if (StartOf(17)) {
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
		while (la.kind == 79) {
			var cexp = default(ElaExpression); 
			Get();
			if (StartOf(17)) {
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
		while (la.kind == 80) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			Get();
			op = ElaOperator.CompBackward; 
			if (StartOf(17)) {
				ForwardCompExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, true);
			else
				exp = new ElaBinary(t) { Right = exp, Operator = op, Left = cexp }; 
			
		}
	}

	void ForwardCompExpr(out ElaExpression exp) {
		Application(out exp);
		while (la.kind == 81) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			Get();
			op = ElaOperator.CompForward; 
			if (StartOf(17)) {
				ForwardCompExpr(out cexp);
			}
			if (cexp == null)
			exp = GetOperatorFun(exp, op, false);
			else
				exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
			
		}
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
		} else SynErr(110);
	}

	void AccessExpr(out ElaExpression exp) {
		Literal(out exp);
		while (la.kind == 22) {
			Get();
			if (la.kind == 12) {
				Get();
				var indExp = new ElaIndexer(t) { TargetObject = exp };
				exp = indExp;
				
				var cexp = default(ElaExpression); 
				Expr(out cexp);
				indExp.Index = cexp;	
				Expect(13);
			} else if (la.kind == 1) {
				Get();
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
			} else SynErr(111);
		}
	}

	void EmbExpr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 1: case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 12: case 18: case 19: case 24: case 37: case 38: case 43: case 44: case 50: case 51: case 60: case 61: case 62: case 63: case 64: case 65: case 66: case 67: case 68: case 69: case 70: case 71: case 72: case 73: case 74: case 75: case 76: case 77: case 78: case 79: case 80: case 81: case 82: {
			BinaryExpr(out exp);
			break;
		}
		case 32: {
			IfExpr(out exp);
			break;
		}
		case 16: {
			LambdaExpr(out exp);
			break;
		}
		case 35: {
			RaiseExpr(out exp);
			break;
		}
		case 39: {
			FailExpr(out exp);
			break;
		}
		case 25: {
			MatchExpr(out exp);
			break;
		}
		case 36: {
			TryExpr(out exp);
			break;
		}
		default: SynErr(112); break;
		}
	}

	void ComprehensionEntry(ElaExpression body, out ElaGenerator it) {
		it = new ElaGenerator(t);
		var cexp = default(ElaExpression);
		var pat = default(ElaPattern);
		
		GeneratorPattern(out pat);
		Expect(56);
		Expr(out cexp);
		it.Pattern = pat;
		it.Target = cexp;
		
		if (la.kind == 14) {
			Guard(out cexp);
			it.Guard = cexp; 
		}
		it.Body = body; 
		if (la.kind == 46) {
			var cit = default(ElaGenerator); 
			Get();
			ComprehensionEntry(body, out cit);
			it.Body = cit; 
		}
	}

	void Ela() {
		var b = new ElaBlock(t);
		Expression = b;
		
		DeclarationBlock(b);
		while (StartOf(21)) {
			DeclarationBlock(b);
		}
	}

	void DeclarationBlock(ElaBlock b) {
		var exp = default(ElaExpression); 
		if (la.kind == 28) {
			RootLetBinding(out exp);
		} else if (la.kind == 30) {
			IncludeStat(out exp);
		} else if (StartOf(6)) {
			EmbExpr(out exp);
			if (FALSE) 
			Expect(42);
		} else SynErr(113);
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,T,T, x,x,x,x, T,T,x,x, T,x,x,x, T,x,x,T, T,T,T,T, x,x,x,T, T,x,x,x, x,x,T,T, x,x,T,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,T,T, x,x,x,x, T,T,x,x, T,x,x,x, T,x,x,T, T,T,T,T, x,x,x,T, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x},
		{x,T,x,T, x,T,T,T, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, T,x,x,T, T,T,T,T, x,x,x,T, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x},
		{x,T,T,x, x,T,T,T, T,x,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,T, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,T,T,T, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,T, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x},
		{x,T,x,x, x,T,T,T, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, x,T,T,T, T,x,T,x, T,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,T,T, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,T,T, x,x,x,x, T,T,x,x, T,x,x,x, T,x,x,T, T,T,T,T, x,x,x,T, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x},
		{x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x},
		{x,T,T,T, x,T,T,T, T,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,T,T, x,x,x,x, T,T,x,x, T,x,T,x, T,x,x,T, T,T,T,T, x,x,x,T, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x}

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
			case 10: s = "LBRA expected"; break;
			case 11: s = "RBRA expected"; break;
			case 12: s = "LILB expected"; break;
			case 13: s = "LIRB expected"; break;
			case 14: s = "PIPE expected"; break;
			case 15: s = "ARROW expected"; break;
			case 16: s = "LAMBDA expected"; break;
			case 17: s = "EQ expected"; break;
			case 18: s = "SEQ expected"; break;
			case 19: s = "MINUS expected"; break;
			case 20: s = "COMPH expected"; break;
			case 21: s = "COMPO expected"; break;
			case 22: s = "DOT expected"; break;
			case 23: s = "IN expected"; break;
			case 24: s = "BASE expected"; break;
			case 25: s = "MATCH expected"; break;
			case 26: s = "ASAMP expected"; break;
			case 27: s = "IS expected"; break;
			case 28: s = "LET expected"; break;
			case 29: s = "PRIVATE expected"; break;
			case 30: s = "OPEN expected"; break;
			case 31: s = "WITH expected"; break;
			case 32: s = "IFS expected"; break;
			case 33: s = "ELSE expected"; break;
			case 34: s = "THEN expected"; break;
			case 35: s = "RAISE expected"; break;
			case 36: s = "TRY expected"; break;
			case 37: s = "TRUE expected"; break;
			case 38: s = "FALSE expected"; break;
			case 39: s = "FAIL expected"; break;
			case 40: s = "WHERE expected"; break;
			case 41: s = "ET expected"; break;
			case 42: s = "EBLOCK expected"; break;
			case 43: s = "\"_\" expected"; break;
			case 44: s = "\"(\" expected"; break;
			case 45: s = "\")\" expected"; break;
			case 46: s = "\",\" expected"; break;
			case 47: s = "\"`\" expected"; break;
			case 48: s = "\"?\" expected"; break;
			case 49: s = "\":\" expected"; break;
			case 50: s = "\"::\" expected"; break;
			case 51: s = "\"--\" expected"; break;
			case 52: s = "\"!\" expected"; break;
			case 53: s = "\"..\" expected"; break;
			case 54: s = "\"&\" expected"; break;
			case 55: s = "\"#\" expected"; break;
			case 56: s = "\"<-\" expected"; break;
			case 57: s = "\"<->\" expected"; break;
			case 58: s = "\"<|\" expected"; break;
			case 59: s = "\"|>\" expected"; break;
			case 60: s = "\"||\" expected"; break;
			case 61: s = "\"&&\" expected"; break;
			case 62: s = "\"==\" expected"; break;
			case 63: s = "\"<>\" expected"; break;
			case 64: s = "\">\" expected"; break;
			case 65: s = "\"<\" expected"; break;
			case 66: s = "\">=\" expected"; break;
			case 67: s = "\"<=\" expected"; break;
			case 68: s = "\">>>\" expected"; break;
			case 69: s = "\"<<<\" expected"; break;
			case 70: s = "\"++\" expected"; break;
			case 71: s = "\"+\" expected"; break;
			case 72: s = "\"*\" expected"; break;
			case 73: s = "\"/\" expected"; break;
			case 74: s = "\"%\" expected"; break;
			case 75: s = "\"**\" expected"; break;
			case 76: s = "\"~~~\" expected"; break;
			case 77: s = "\"|||\" expected"; break;
			case 78: s = "\"^^^\" expected"; break;
			case 79: s = "\"&&&\" expected"; break;
			case 80: s = "\"<<\" expected"; break;
			case 81: s = "\">>\" expected"; break;
			case 82: s = "\"(&\" expected"; break;
			case 83: s = "??? expected"; break;
			case 84: s = "invalid Literal"; break;
			case 85: s = "invalid Primitive"; break;
			case 86: s = "invalid SimpleExpr"; break;
			case 87: s = "invalid VariableReference"; break;
			case 88: s = "invalid Expr"; break;
			case 89: s = "invalid Guard"; break;
			case 90: s = "invalid OrPattern"; break;
			case 91: s = "invalid VariantPattern"; break;
			case 92: s = "invalid SinglePattern"; break;
			case 93: s = "invalid LiteralPattern"; break;
			case 94: s = "invalid LiteralPattern"; break;
			case 95: s = "invalid TypeCheckPattern"; break;
			case 96: s = "invalid BindingPattern"; break;
			case 97: s = "invalid GeneratorPattern"; break;
			case 98: s = "invalid IsOperatorPattern"; break;
			case 99: s = "invalid FieldPattern"; break;
			case 100: s = "invalid RecordField"; break;
			case 101: s = "invalid ParamList"; break;
			case 102: s = "invalid BindingBody"; break;
			case 103: s = "invalid RootLetBinding"; break;
			case 104: s = "invalid BindingGuardList"; break;
			case 105: s = "invalid FunName"; break;
			case 106: s = "invalid IncludeStat"; break;
			case 107: s = "invalid Qualident"; break;
			case 108: s = "invalid PartialBinaryExpr"; break;
			case 109: s = "invalid FuncOperator"; break;
			case 110: s = "invalid Application"; break;
			case 111: s = "invalid AccessExpr"; break;
			case 112: s = "invalid EmbExpr"; break;
			case 113: s = "invalid DeclarationBlock"; break;

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

