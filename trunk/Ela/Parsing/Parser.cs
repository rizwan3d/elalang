
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
	public const int _traitTok = 3;
	public const int _argIdent = 4;
	public const int _funcTok = 5;
	public const int _intTok = 6;
	public const int _realTok = 7;
	public const int _stringTok = 8;
	public const int _charTok = 9;
	public const int _operatorTok1 = 10;
	public const int _operatorTok2 = 11;
	public const int _operatorTok3 = 12;
	public const int _operatorTok4 = 13;
	public const int _operatorTok5 = 14;
	public const int _operatorTok6 = 15;
	public const int _operatorTok7 = 16;
	public const int _operatorTok8 = 17;
	public const int _LBRA = 18;
	public const int _RBRA = 19;
	public const int _LILB = 20;
	public const int _LIRB = 21;
	public const int _PIPE = 22;
	public const int _ARROW = 23;
	public const int _LAMBDA = 24;
	public const int _COMPH = 25;
	public const int _COMPO = 26;
	public const int _DOT = 27;
	public const int _IN = 28;
	public const int _BASE = 29;
	public const int _MATCH = 30;
	public const int _ASAMP = 31;
	public const int _IS = 32;
	public const int _LET = 33;
	public const int _PRIVATE = 34;
	public const int _OPEN = 35;
	public const int _WITH = 36;
	public const int _IFS = 37;
	public const int _ELSE = 38;
	public const int _THEN = 39;
	public const int _RAISE = 40;
	public const int _TRY = 41;
	public const int _TRUE = 42;
	public const int _FALSE = 43;
	public const int _FAIL = 44;
	public const int _WHERE = 45;
	public const int _ET = 46;
	public const int _EBLOCK = 47;
	public const int maxT = 71;

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
		Expect(47);
		if (!t.virt) scanner.PopIndent(); 
	}

	void Literal(out ElaExpression exp) {
		exp = null; 
		if (StartOf(1)) {
			Primitive(out exp);
		} else if (la.kind == 18) {
			RecordLiteral(out exp);
		} else if (la.kind == 20) {
			ListLiteral(out exp);
		} else if (la.kind == 49) {
			TupleLiteral(out exp);
		} else if (StartOf(2)) {
			SimpleExpr(out exp);
		} else SynErr(72);
	}

	void Primitive(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 6: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseInt(t.val) };	
			break;
		}
		case 7: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseReal(t.val) }; 
			break;
		}
		case 8: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseString(t.val) }; 
			break;
		}
		case 9: {
			Get();
			exp = new ElaPrimitive(t) { Value = ParseChar(t.val) }; 
			break;
		}
		case 42: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(true) }; 
			break;
		}
		case 43: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(false) }; 
			break;
		}
		default: SynErr(73); break;
		}
	}

	void RecordLiteral(out ElaExpression exp) {
		exp = null; 
		var fld = default(ElaFieldDeclaration);
		
		Expect(18);
		var rec = new ElaRecordLiteral(t); exp = rec; 
		RecordField(out fld);
		rec.Fields.Add(fld); 
		while (la.kind == 51) {
			Get();
			RecordField(out fld);
			rec.Fields.Add(fld); 
		}
		Expect(19);
	}

	void ListLiteral(out ElaExpression exp) {
		Expect(20);
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
		
		Expect(21);
	}

	void TupleLiteral(out ElaExpression exp) {
		var ot = default(Token);
		exp = null; 
		
		Expect(49);
		ot = t; 
		if (StartOf(4)) {
			GroupExpr(out exp);
		}
		Expect(50);
		if (exp == null)
		exp = new ElaUnitLiteral(ot);
		
	}

	void SimpleExpr(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 4) {
			ArgumentReference(out exp);
		} else if (la.kind == 29) {
			BaseReference(out exp);
		} else if (la.kind == 1 || la.kind == 48) {
			VariableReference(out exp);
		} else if (la.kind == 70) {
			LazyExpr(out exp);
		} else SynErr(74);
	}

	void ArgumentReference(out ElaExpression exp) {
		Expect(4);
		exp = new ElaArgument(t) { ArgumentName = t.val.Substring(1) }; 
	}

	void BaseReference(out ElaExpression exp) {
		var baseRef = default(ElaBaseReference);
		exp = null;
		
		Expect(29);
		baseRef = new ElaBaseReference(t); 
		Expect(27);
		var ot = t; 
		Expect(1);
		exp = new ElaFieldReference(ot) { FieldName = t.val, TargetObject = baseRef }; 
	}

	void VariableReference(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = new ElaVariableReference(t) { VariableName = t.val }; 
		} else if (la.kind == 48) {
			Get();
			exp = new ElaPlaceholder(t); 
		} else SynErr(75);
	}

	void LazyExpr(out ElaExpression exp) {
		Expect(70);
		var lazy = new ElaLazyLiteral(t); 
		Expr(out exp);
		var m = new ElaMatch(t);
		m.Entries.Add(new ElaMatchEntry { Pattern = new ElaUnitPattern(), Expression = exp });
		lazy.Body = m;
		exp = lazy;
		
		Expect(50);
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
		if (la.kind == 51) {
			var tuple = new ElaTupleLiteral(ot); 
			tuple.Parameters.Add(exp);
			exp = tuple; 
			
			Get();
			if (StartOf(4)) {
				Expr(out cexp);
				tuple.Parameters.Add(cexp); 
			}
			while (la.kind == 51) {
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
		} else if (la.kind == 33) {
			LetBinding(out exp);
		} else SynErr(76);
	}

	void MatchExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 30)) {SynErr(77); Get();}
		Expect(30);
		var match = new ElaMatch(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(36);
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
		
		if (la.kind == 22) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(52);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 45) {
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
		Expect(52);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 45) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
	}

	void RootPattern(out ElaPattern pat) {
		OrPattern(out pat);
		if (la.kind == 56) {
			ConsPattern(pat, out pat);
		}
	}

	void Guard(out ElaExpression exp) {
		exp = null; 
		Expect(22);
		if (StartOf(9)) {
			BinaryExpr(out exp);
			while (la.kind == 51) {
				var old = exp; 
				Get();
				var ot = t; 
				BinaryExpr(out exp);
				exp = new ElaBinary(t) { Operator = ElaOperator.BooleanAnd, Left = old, Right = exp };
				
			}
		} else if (la.kind == 38) {
			Get();
			exp = new ElaOtherwiseGuard(t); 
		} else SynErr(78);
	}

	void WhereBinding(out ElaExpression exp) {
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 45)) {SynErr(79); Get();}
		Expect(45);
		BindingBody(ElaVariableFlags.None, out exp);
		EndBlock();
	}

	void BinaryExpr(out ElaExpression exp) {
		exp = null;
		var ot = t; 
		
		AssignExpr(out exp);
		while (la.kind == 69) {
			var cexp = default(ElaExpression); 
			Get();
			AssignExpr(out cexp);
			exp = new ElaBinary(t) { Operator = ElaOperator.Sequence, Left = exp, Right = cexp };
			
		}
	}

	void OrPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 2 || la.kind == 53) {
			VariantPattern(out pat);
		} else if (StartOf(10)) {
			AsPattern(out pat);
		} else SynErr(80);
	}

	void ConsPattern(ElaPattern prev, out ElaPattern exp) {
		var cexp = default(ElaPattern); 
		var ht = new ElaHeadTailPattern(t); 
		ht.Patterns.Add(prev);
		exp = ht;				
		
		Expect(56);
		AsPattern(out cexp);
		ht.Patterns.Add(cexp); 
		while (la.kind == 56) {
			Get();
			AsPattern(out cexp);
			ht.Patterns.Add(cexp); 
		}
	}

	void ParenPattern(out ElaPattern pat) {
		pat = null; 
		OrPattern(out pat);
		if (la.kind == 56) {
			ConsPattern(pat, out pat);
		}
		if (la.kind == 51) {
			TuplePattern(pat, out pat);
		}
	}

	void TuplePattern(ElaPattern prev, out ElaPattern pat) {
		var seq = new ElaTuplePattern(t); 
		seq.Patterns.Add(prev);
		var cpat = default(ElaPattern);
		pat = seq;
		
		Expect(51);
		if (StartOf(10)) {
			AsPattern(out cpat);
			if (la.kind == 56) {
				ConsPattern(cpat, out cpat);
			}
			seq.Patterns.Add(cpat); 
			while (la.kind == 51) {
				Get();
				AsPattern(out cpat);
				if (la.kind == 56) {
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
		} else if (la.kind == 53) {
			Get();
			vp = new ElaVariantPattern(t); 
			if (StartOf(10)) {
				AsPattern(out cpat);
				vp.Pattern = cpat; 
			}
			pat = vp; 
		} else SynErr(81);
	}

	void AsPattern(out ElaPattern pat) {
		pat = null; 
		SinglePattern(out pat);
		if (la.kind == 31) {
			Get();
			var asPat = new ElaAsPattern(t) { Pattern = pat }; 
			pat = asPat;				
			
			Expect(1);
			asPat.Name = t.val; 
		}
	}

	void FuncPattern(out ElaPattern pat) {
		AsPattern(out pat);
		if (la.kind == 56) {
			ConsPattern(pat, out pat);
		}
	}

	void SinglePattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 48: {
			DefaultPattern(out pat);
			break;
		}
		case 49: {
			UnitPattern(out pat);
			break;
		}
		case 6: case 7: case 8: case 9: case 42: case 43: {
			LiteralPattern(out pat);
			break;
		}
		case 20: {
			ListPattern(out pat);
			break;
		}
		case 18: {
			RecordPattern(out pat);
			break;
		}
		case 1: {
			IdentPattern(out pat);
			break;
		}
		case 54: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(82); break;
		}
	}

	void DefaultPattern(out ElaPattern pat) {
		Expect(48);
		pat = new ElaDefaultPattern(t); 
	}

	void UnitPattern(out ElaPattern pat) {
		var ot = t;
		pat = null;
		
		Expect(49);
		if (StartOf(8)) {
			ParenPattern(out pat);
		}
		Expect(50);
		if (pat == null)
		pat = new ElaUnitPattern(ot); 
		
	}

	void LiteralPattern(out ElaPattern pat) {
		var lit = default(ElaLiteralValue);
		pat = null;
		
		switch (la.kind) {
		case 8: {
			Get();
			lit = ParseString(t.val); 
			break;
		}
		case 9: {
			Get();
			lit = ParseChar(t.val); 
			break;
		}
		case 6: {
			Get();
			lit = ParseInt(t.val); 
			break;
		}
		case 7: {
			Get();
			lit = ParseReal(t.val); 
			break;
		}
		case 42: {
			Get();
			lit = new ElaLiteralValue(true); 
			break;
		}
		case 43: {
			Get();
			lit = new ElaLiteralValue(false); 
			break;
		}
		default: SynErr(83); break;
		}
		pat = new ElaLiteralPattern(t) { Value = lit };				
		
	}

	void ListPattern(out ElaPattern pat) {
		var cexp = default(ElaPattern); 
		var ht = default(ElaHeadTailPattern); 
		pat = null;
		
		Expect(20);
		if (StartOf(10)) {
			ht = new ElaHeadTailPattern(t);			
			
			AsPattern(out cexp);
			ht.Patterns.Add(cexp);  
			while (la.kind == 51) {
				Get();
				AsPattern(out cexp);
				ht.Patterns.Add(cexp); 
			}
			ht.Patterns.Add(new ElaNilPattern(t));
			pat = ht;
			
		}
		if (pat == null)
		pat = new ElaNilPattern(t);
		
		Expect(21);
	}

	void RecordPattern(out ElaPattern pat) {
		pat = null; 
		var cpat = default(ElaFieldPattern);
		
		Expect(18);
		var rec = new ElaRecordPattern(t); 
		pat = rec; 
		
		if (la.kind == 1 || la.kind == 8) {
			FieldPattern(out cpat);
			rec.Fields.Add(cpat); 
			while (la.kind == 51) {
				Get();
				FieldPattern(out cpat);
				rec.Fields.Add(cpat); 
			}
		}
		Expect(19);
	}

	void IdentPattern(out ElaPattern pat) {
		pat = null; 
		Expect(1);
		var name = t.val; 
		if (la.kind == 55) {
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
		
		Expect(54);
		if (la.kind == 1) {
			Get();
			eis.TypeAffinity = GetType(t.val); 
		} else if (la.kind == 49) {
			
			Get();
			Expect(1);
			tmp = Builtins.Trait(t.val); 
			
			if (tmp == ElaTraits.None) {
				AddError(ElaParserError.UnknownTrait, t.val);
			}
			
			eis.Traits |= tmp;
			
			while (la.kind == 51) {
				Get();
				Expect(1);
				tmp = Builtins.Trait(t.val); 
				
				if (tmp == ElaTraits.None) {
					AddError(ElaParserError.UnknownTrait, t.val);
				}
				
				eis.Traits |= tmp;
				
			}
			Expect(50);
		} else SynErr(84);
	}

	void BindingPattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 48: {
			DefaultPattern(out pat);
			break;
		}
		case 49: {
			UnitPattern(out pat);
			break;
		}
		case 6: case 7: case 8: case 9: case 42: case 43: {
			LiteralPattern(out pat);
			break;
		}
		case 20: {
			ListPattern(out pat);
			break;
		}
		case 18: {
			RecordPattern(out pat);
			break;
		}
		case 54: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(85); break;
		}
		if (la.kind == 31) {
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
		} else if (StartOf(1)) {
			LiteralPattern(out pat);
		} else if (la.kind == 18) {
			RecordPattern(out pat);
		} else if (la.kind == 20) {
			ListPattern(out pat);
		} else if (la.kind == 49) {
			UnitPattern(out pat);
		} else SynErr(86);
	}

	void IsOperatorPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 54) {
			TypeCheckPattern(out pat);
		} else if (StartOf(1)) {
			LiteralPattern(out pat);
		} else if (la.kind == 18) {
			RecordPattern(out pat);
		} else if (la.kind == 20) {
			ListPattern(out pat);
		} else if (la.kind == 49) {
			UnitPattern(out pat);
		} else SynErr(87);
	}

	void FieldPattern(out ElaFieldPattern fld) {
		fld = null;
		var cpat = default(ElaPattern);
		
		if (la.kind == 8) {
			Get();
			fld = new ElaFieldPattern(t) { Name = ReadString(t.val) }; 
			Expect(52);
			AsPattern(out cpat);
			fld.Value = cpat; 
		} else if (la.kind == 1) {
			Get();
			fld = new ElaFieldPattern(t) { Name = t.val }; 
			if (la.kind == 52) {
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
		var mutable = false;
		
		if (la.kind == 57) {
			Get();
			mutable = true; 
		}
		if (la.kind == 1) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = t.val, Mutable = mutable }; 
			if (la.kind == 52) {
				Get();
				Expr(out cexp);
				fld.FieldValue = cexp; 
			}
			if (fld.FieldValue == null)
			fld.FieldValue = new ElaVariableReference(t) { VariableName = t.val };
			
		} else if (la.kind == 8) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = ReadString(t.val), Mutable = mutable }; 
			Expect(52);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else SynErr(89);
	}

	void RangeExpr(ElaExpression first, ElaExpression sec, out ElaRange rng) {
		rng = new ElaRange(t) { First = first, Second = sec };
		var cexp = default(ElaExpression);
		
		Expect(58);
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
		if (la.kind == 26 || la.kind == 51 || la.kind == 58) {
			if (la.kind == 26) {
				ComprehensionExpr(exp, out comp);
			} else if (la.kind == 58) {
				RangeExpr(exp, null, out rng);
			} else {
				var oexp = exp; 
				Get();
				Expr(out exp);
				if (la.kind == 58) {
					RangeExpr(oexp, exp, out rng);
				} else if (la.kind == 21 || la.kind == 51) {
					list = new List<ElaExpression>();
					list.Add(oexp);
					list.Add(exp);
					
					while (la.kind == 51) {
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
		
		Expect(26);
		ComprehensionEntry(sel, out it);
		exp = new ElaComprehension(ot) { Generator = it }; 
	}

	void LetBinding(out ElaExpression exp) {
		exp = null; 
		var inExp = default(ElaExpression);
		var flags = default(ElaVariableFlags);
		
		while (!(la.kind == 0 || la.kind == 33)) {SynErr(91); Get();}
		Expect(33);
		if (la.kind == 34 || la.kind == 60) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		ExpectWeak(28, 11);
		Expr(out inExp);
		((ElaBinding)exp).In = inExp; 
	}

	void VariableAttributes(out ElaVariableFlags flags) {
		flags = ElaVariableFlags.None; 
		if (la.kind == 34) {
			Get();
			flags = ElaVariableFlags.Private; 
			if (la.kind == 60) {
				Get();
				flags = ElaVariableFlags.Inline; 
			}
		} else if (la.kind == 60) {
			Get();
			flags = ElaVariableFlags.Inline; 
			if (la.kind == 34) {
				Get();
				flags = ElaVariableFlags.Private; 
			}
		} else SynErr(92);
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
				} else if (la.kind == 22) {
					BindingBodyGuards(varExp);
				} else {
					BindingBodyInit(varExp);
				}
			}
			SetObjectMetadata(varExp, cexp); 
		} else if (StartOf(13)) {
			BindingPattern(out pat);
			varExp.Pattern = pat; 
			if (la.kind == 52) {
				BindingBodyInit(varExp);
			}
		} else if (StartOf(14)) {
			BindingBodyOperator(varExp);
		} else SynErr(93);
		if (la.kind == 46) {
			ExpectWeak(46, 15);
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
		while (!(la.kind == 0 || la.kind == 33)) {SynErr(94); Get();}
		Expect(33);
		if (la.kind == 34 || la.kind == 60) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		if (la.kind == 28) {
			ExpectWeak(28, 11);
			Expr(out inExp);
			((ElaBinding)exp).In = inExp; 
			EndBlock();
		} else if (la.kind == 47) {
			EndBlock();
		} else SynErr(95);
	}

	void BindingGuard(out ElaExpression exp) {
		exp = null; 
		BinaryExpr(out exp);
		while (la.kind == 51) {
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
		
		Expect(22);
		if (StartOf(9)) {
			var newCond = new ElaCondition(t);
			cond.False = newCond;
			cond = newCond;
			
			BindingGuard(out gexp);
			cond.Condition = gexp; 
			Expect(52);
			Expr(out cexp);
			cond.True = cexp; 
			if (la.kind == 22) {
				BindingGuardList(ref cond);
			}
		} else if (la.kind == 38) {
			Get();
			Expect(52);
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
		ProcessFunctionParameters(mi, ot); 
	}

	void BindingBodyGuards(ElaBinding varExp) {
		var gexp = default(ElaExpression);
		var cexp3 = default(ElaExpression);
		var cond = new ElaCondition(t);
		varExp.InitExpression = cond;
		
		Expect(22);
		BindingGuard(out gexp);
		cond.Condition = gexp; 
		Expect(52);
		Expr(out cexp3);
		cond.True = cexp3; 
		BindingGuardList(ref cond);
	}

	void BindingBodyInit(ElaBinding varExp) {
		var cexp = default(ElaExpression); 
		var cexp2 = default(ElaExpression);
		
		Expect(52);
		if (StartOf(4)) {
			Expr(out cexp);
		} else if (la.kind == 61) {
			Get();
			Expect(1);
			cexp = new ElaBuiltin(t) { Kind = Builtins.Kind(t.val) }; 
		} else SynErr(97);
		varExp.InitExpression = cexp; 
		if (la.kind == 45) {
			WhereBinding(out cexp2);
			varExp.Where = (ElaBinding)cexp2; 
		}
	}

	void BindingBodyOperator(ElaBinding varExp) {
		Operators();
		varExp.VariableName = t.val; 
		if (StartOf(10)) {
			FunExpr(varExp);
			var fun = (ElaFunctionLiteral)varExp.InitExpression;
			fun.FunctionType = ElaFunctionType.Operator;
			
		} else if (la.kind == 52) {
			BindingBodyInit(varExp);
			if (varExp.InitExpression.Type == ElaNodeType.FunctionLiteral)
			((ElaFunctionLiteral)varExp.InitExpression).FunctionType = ElaFunctionType.Operator;
			
		} else SynErr(98);
	}

	void Operators() {
		switch (la.kind) {
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
		case 16: {
			Get();
			break;
		}
		case 17: {
			Get();
			break;
		}
		case 56: {
			Get();
			break;
		}
		default: SynErr(99); break;
		}
	}

	void FunName(ElaFunctionLiteral fun) {
		if (la.kind == 1) {
			Get();
		} else if (StartOf(14)) {
			Operators();
		} else SynErr(100);
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
		if (la.kind == 22) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(52);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 45) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(16)) {
			if (StartOf(17)) {
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
		Expect(52);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 45) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (StartOf(16)) {
			if (StartOf(17)) {
				scanner.InjectBlock(); 
				FunName(fun);
				FunBodyExpr(fun);
			} else {
				ChildFunBodyExpr(fun);
			}
		}
	}

	void LambdaExpr(out ElaExpression exp) {
		while (!(la.kind == 0 || la.kind == 24)) {SynErr(101); Get();}
		Expect(24);
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
		if (la.kind == 22) {
			var cexp = default(ElaExpression); 
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(23);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
	}

	void IncludeStat(out ElaExpression exp) {
		exp = null; 
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 35)) {SynErr(102); Get();}
		Expect(35);
		var inc = new ElaModuleInclude(t); 
		Qualident(inc.Path);
		var name = inc.Path[inc.Path.Count - 1];				
		inc.Path.RemoveAt(inc.Path.Count - 1);				
		inc.Alias = inc.Name = name;
		exp = inc;
		
		if (la.kind == 62) {
			Get();
			if (la.kind == 1) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 8) {
				Get();
				inc.DllName = ReadString(t.val); 
			} else SynErr(103);
		}
		if (la.kind == 31) {
			Get();
			Expect(1);
			inc.Alias = t.val; 
		}
		Expect(47);
	}

	void Qualident(List<String> path ) {
		var val = String.Empty; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 8) {
			Get();
			val = ReadString(t.val); 
		} else SynErr(104);
		path.Add(val); 
		if (la.kind == 27) {
			Get();
			Qualident(path);
		}
	}

	void IfExpr(out ElaExpression exp) {
		Expect(37);
		var cond = new ElaCondition(t); 
		var cexp = default(ElaExpression);	
		exp = cond;
		
		Expr(out cexp);
		cond.Condition = cexp; 
		Expect(39);
		Expr(out cexp);
		cond.True = cexp; 
		ExpectWeak(38, 11);
		Expr(out cexp);
		cond.False = cexp; 
	}

	void RaiseExpr(out ElaExpression exp) {
		Expect(40);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		var code = String.Empty;
		
		Expect(1);
		code = t.val; 
		if (la.kind == 49) {
			Get();
			Expr(out cexp);
			Expect(50);
		}
		r.ErrorCode = code;
		r.Expression = cexp; 
		
	}

	void FailExpr(out ElaExpression exp) {
		Expect(44);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		
		Expr(out cexp);
		r.Expression = cexp; 
		r.ErrorCode = "Failure"; 
		
	}

	void TryExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		Expect(41);
		var ot = t;
		var match = new ElaTry(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(36);
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
		BackwardPipeExpr(out exp);
		while (la.kind == 63 || la.kind == 64) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 63) {
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
		while (la.kind == 65) {
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
		while (la.kind == 66) {
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
		while (la.kind == 67) {
			var cexp = default(ElaExpression); 
			Get();
			AndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanOr, Right = cexp }; 
			
		}
	}

	void AndExpr(out ElaExpression exp) {
		OpExpr1(out exp);
		while (la.kind == 68) {
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
		
		if (StartOf(18)) {
			OpExpr2(out exp);
			while (la.kind == 10) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(18)) {
					OpExpr2(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(op, exp, null);
				else
					exp = GetBinaryFunction(op, exp, cexp);
				
			}
		} else if (la.kind == 10) {
			Get();
			op = t.val; 
			if (StartOf(18)) {
				OpExpr2(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = op };
			
		} else SynErr(105);
	}

	void OpExpr2(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(19)) {
			OpExpr3(out exp);
			while (la.kind == 11) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(19)) {
					OpExpr3(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(op, exp, null);
				else
					exp = GetBinaryFunction(op, exp, cexp);
				
			}
		} else if (la.kind == 11) {
			Get();
			op = t.val; 
			if (StartOf(19)) {
				OpExpr3(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(106);
	}

	void OpExpr3(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(20)) {
			OpExpr4(out exp);
			while (la.kind == 12 || la.kind == 56) {
				var cexp = default(ElaExpression); 
				if (la.kind == 12) {
					Get();
				} else {
					Get();
				}
				op = t.val; 
				if (StartOf(19)) {
					OpExpr3(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(op, exp, null);
				else
					exp = GetBinaryFunction(op, exp, cexp);
				
			}
		} else if (la.kind == 12 || la.kind == 56) {
			if (la.kind == 12) {
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
			
		} else SynErr(107);
	}

	void OpExpr4(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(21)) {
			OpExpr5(out exp);
			while (la.kind == 13) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(21)) {
					OpExpr5(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(op, exp, null);
				else
					exp = GetBinaryFunction(op, exp, cexp);
				
			}
		} else if (la.kind == 13) {
			Get();
			op = t.val; 
			if (StartOf(21)) {
				OpExpr5(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(108);
	}

	void OpExpr5(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(22)) {
			CastExpr(out exp);
			while (la.kind == 14) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(22)) {
					CastExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(op, exp, null);
				else
					exp = GetBinaryFunction(op, exp, cexp);
				
			}
		} else if (la.kind == 14) {
			Get();
			op = t.val; 
			if (StartOf(22)) {
				CastExpr(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(109);
	}

	void CastExpr(out ElaExpression exp) {
		InfixExpr(out exp);
		while (la.kind == 25 || la.kind == 32 || la.kind == 55) {
			if (la.kind == 55) {
				Get();
				Expect(1);
				exp = new ElaCast(t) { CastAffinity = GetType(t.val), Expression = exp };
			} else if (la.kind == 32) {
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
		var name = String.Empty;
		
		if (StartOf(23)) {
			OpExpr6(out exp);
			while (la.kind == 5) {
				var cexp = default(ElaExpression); 
				ot = t;
				
				Get();
				name = t.val.Trim('`'); 
				if (StartOf(23)) {
					OpExpr6(out cexp);
				}
				var fc = new ElaFunctionCall(ot) { 
					Target = new ElaVariableReference(t) { VariableName = name }
				};
				fc.Parameters.Add(exp);			
				
				if (cexp != null)
					fc.Parameters.Add(cexp);
								
				exp = fc;
				
			}
		} else if (la.kind == 5) {
			Get();
			name = t.val.Trim('`'); 
			if (StartOf(23)) {
				OpExpr6(out exp);
				exp = GetPrefixFun(name, exp, true);	
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = name };
			
		} else SynErr(110);
	}

	void ComprehensionOpExpr(ElaExpression init, out ElaExpression exp) {
		var list = default(List<ElaExpression>);
		var comp = default(ElaComprehension);
		var rng = default(ElaRange);
		exp = null;
		
		Expect(20);
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
		
		Expect(21);
	}

	void OpExpr6(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(24)) {
			OpExpr7(out exp);
			while (la.kind == 15) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(23)) {
					OpExpr6(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(op, exp, null);
				else
					exp = GetBinaryFunction(op, exp, cexp);
				
			}
		} else if (la.kind == 15) {
			Get();
			op = t.val; 
			if (StartOf(24)) {
				OpExpr7(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(111);
	}

	void OpExpr7(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(25)) {
			OpExpr8(out exp);
			while (la.kind == 16) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(25)) {
					OpExpr8(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(op, exp, null);
				else
					exp = GetBinaryFunction(op, exp, cexp);
				
			}
		} else if (la.kind == 16) {
			Get();
			op = t.val; 
			if (StartOf(25)) {
				OpExpr8(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(112);
	}

	void OpExpr8(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(26)) {
			Application(out exp);
			while (la.kind == 17) {
				Get();
				op = t.val; 
				exp = GetBinaryFunction(op, exp, null); 
			}
		} else if (la.kind == 17) {
			Get();
			op = t.val; 
			if (StartOf(26)) {
				Application(out exp);
				exp = GetBinaryFunction(op, exp, null); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(113);
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
		} else SynErr(114);
	}

	void AccessExpr(out ElaExpression exp) {
		Literal(out exp);
		while (la.kind == 27) {
			Get();
			if (la.kind == 20) {
				Get();
				var indExp = new ElaIndexer(t) { TargetObject = exp };
				exp = indExp;
				
				var cexp = default(ElaExpression); 
				Expr(out cexp);
				indExp.Index = cexp;	
				Expect(21);
			} else if (la.kind == 1) {
				Get();
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
			} else SynErr(115);
		}
	}

	void EmbExpr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 1: case 2: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13: case 14: case 15: case 16: case 17: case 18: case 20: case 29: case 42: case 43: case 48: case 49: case 56: case 70: {
			BinaryExpr(out exp);
			break;
		}
		case 37: {
			IfExpr(out exp);
			break;
		}
		case 24: {
			LambdaExpr(out exp);
			break;
		}
		case 40: {
			RaiseExpr(out exp);
			break;
		}
		case 44: {
			FailExpr(out exp);
			break;
		}
		case 30: {
			MatchExpr(out exp);
			break;
		}
		case 41: {
			TryExpr(out exp);
			break;
		}
		default: SynErr(116); break;
		}
	}

	void ComprehensionEntry(ElaExpression body, out ElaGenerator it) {
		it = new ElaGenerator(t);
		it.Body = body;
		var cexp = default(ElaExpression);
		var pat = default(ElaPattern);
		
		GeneratorPattern(out pat);
		Expect(63);
		Expr(out cexp);
		it.Pattern = pat;
		it.Target = cexp;
		
		if (la.kind == 22 || la.kind == 51) {
			if (la.kind == 51) {
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
		while (StartOf(27)) {
			DeclarationBlock(b);
		}
	}

	void DeclarationBlock(ElaBlock b) {
		var exp = default(ElaExpression); 
		if (la.kind == 33) {
			RootLetBinding(out exp);
		} else if (la.kind == 35) {
			IncludeStat(out exp);
		} else if (StartOf(6)) {
			EmbExpr(out exp);
			if (FALSE) 
			Expect(47);
		} else SynErr(117);
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,T,x, x,T,x,T, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,x,x, x,T,T,x, x,T,x,x, x,T,x,x, T,T,T,T, T,x,x,x, T,T,x,x, x,x,x,x, T,x,x,T, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,x,x, x,T,T,x, x,T,x,x, x,T,x,x, T,T,T,T, T,x,x,x, T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,x,x, T,x,T,T, T,T,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,x,x, x,T,T,x, x,x,x,x, x,T,x,x, T,T,T,T, T,x,x,x, T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,x,x, x,T,T,x, x,T,x,T, x,T,x,x, T,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,T,x,x, x,x,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,x,x, x,x,T,x, x,T,x,T, x,x,x,x, x,x,T,T, x,T,x,x, T,T,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, T,T,x,T, T,T,T,T, T,T,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,T,T,T, T,T,x,x, T,T,T,T, T,T,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,T,T,T, T,T,x,x, x,T,T,T, T,T,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,T,T,T, T,T,x,x, x,x,T,T, T,T,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,T,T,T, T,T,x,x, x,x,x,T, T,T,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,x,x,T, T,T,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,x,x,x, T,T,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,x,x,x, x,T,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x},
		{x,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, T,x,x,x, x,T,T,x, x,T,x,T, x,T,x,x, T,T,T,T, T,x,x,x, T,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x}

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
			case 3: s = "traitTok expected"; break;
			case 4: s = "argIdent expected"; break;
			case 5: s = "funcTok expected"; break;
			case 6: s = "intTok expected"; break;
			case 7: s = "realTok expected"; break;
			case 8: s = "stringTok expected"; break;
			case 9: s = "charTok expected"; break;
			case 10: s = "operatorTok1 expected"; break;
			case 11: s = "operatorTok2 expected"; break;
			case 12: s = "operatorTok3 expected"; break;
			case 13: s = "operatorTok4 expected"; break;
			case 14: s = "operatorTok5 expected"; break;
			case 15: s = "operatorTok6 expected"; break;
			case 16: s = "operatorTok7 expected"; break;
			case 17: s = "operatorTok8 expected"; break;
			case 18: s = "LBRA expected"; break;
			case 19: s = "RBRA expected"; break;
			case 20: s = "LILB expected"; break;
			case 21: s = "LIRB expected"; break;
			case 22: s = "PIPE expected"; break;
			case 23: s = "ARROW expected"; break;
			case 24: s = "LAMBDA expected"; break;
			case 25: s = "COMPH expected"; break;
			case 26: s = "COMPO expected"; break;
			case 27: s = "DOT expected"; break;
			case 28: s = "IN expected"; break;
			case 29: s = "BASE expected"; break;
			case 30: s = "MATCH expected"; break;
			case 31: s = "ASAMP expected"; break;
			case 32: s = "IS expected"; break;
			case 33: s = "LET expected"; break;
			case 34: s = "PRIVATE expected"; break;
			case 35: s = "OPEN expected"; break;
			case 36: s = "WITH expected"; break;
			case 37: s = "IFS expected"; break;
			case 38: s = "ELSE expected"; break;
			case 39: s = "THEN expected"; break;
			case 40: s = "RAISE expected"; break;
			case 41: s = "TRY expected"; break;
			case 42: s = "TRUE expected"; break;
			case 43: s = "FALSE expected"; break;
			case 44: s = "FAIL expected"; break;
			case 45: s = "WHERE expected"; break;
			case 46: s = "ET expected"; break;
			case 47: s = "EBLOCK expected"; break;
			case 48: s = "\"_\" expected"; break;
			case 49: s = "\"(\" expected"; break;
			case 50: s = "\")\" expected"; break;
			case 51: s = "\",\" expected"; break;
			case 52: s = "\"=\" expected"; break;
			case 53: s = "\"`\" expected"; break;
			case 54: s = "\"?\" expected"; break;
			case 55: s = "\":\" expected"; break;
			case 56: s = "\"::\" expected"; break;
			case 57: s = "\"!\" expected"; break;
			case 58: s = "\"..\" expected"; break;
			case 59: s = "\"&\" expected"; break;
			case 60: s = "\"inline\" expected"; break;
			case 61: s = "\"__internal\" expected"; break;
			case 62: s = "\"#\" expected"; break;
			case 63: s = "\"<-\" expected"; break;
			case 64: s = "\"<->\" expected"; break;
			case 65: s = "\"<|\" expected"; break;
			case 66: s = "\"|>\" expected"; break;
			case 67: s = "\"||\" expected"; break;
			case 68: s = "\"&&\" expected"; break;
			case 69: s = "\"$\" expected"; break;
			case 70: s = "\"(&\" expected"; break;
			case 71: s = "??? expected"; break;
			case 72: s = "invalid Literal"; break;
			case 73: s = "invalid Primitive"; break;
			case 74: s = "invalid SimpleExpr"; break;
			case 75: s = "invalid VariableReference"; break;
			case 76: s = "invalid Expr"; break;
			case 77: s = "this symbol not expected in MatchExpr"; break;
			case 78: s = "invalid Guard"; break;
			case 79: s = "this symbol not expected in WhereBinding"; break;
			case 80: s = "invalid OrPattern"; break;
			case 81: s = "invalid VariantPattern"; break;
			case 82: s = "invalid SinglePattern"; break;
			case 83: s = "invalid LiteralPattern"; break;
			case 84: s = "invalid TypeCheckPattern"; break;
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
			case 99: s = "invalid Operators"; break;
			case 100: s = "invalid FunName"; break;
			case 101: s = "this symbol not expected in LambdaExpr"; break;
			case 102: s = "this symbol not expected in IncludeStat"; break;
			case 103: s = "invalid IncludeStat"; break;
			case 104: s = "invalid Qualident"; break;
			case 105: s = "invalid OpExpr1"; break;
			case 106: s = "invalid OpExpr2"; break;
			case 107: s = "invalid OpExpr3"; break;
			case 108: s = "invalid OpExpr4"; break;
			case 109: s = "invalid OpExpr5"; break;
			case 110: s = "invalid InfixExpr"; break;
			case 111: s = "invalid OpExpr6"; break;
			case 112: s = "invalid OpExpr7"; break;
			case 113: s = "invalid OpExpr8"; break;
			case 114: s = "invalid Application"; break;
			case 115: s = "invalid AccessExpr"; break;
			case 116: s = "invalid EmbExpr"; break;
			case 117: s = "invalid DeclarationBlock"; break;

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

