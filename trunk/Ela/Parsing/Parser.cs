
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
	public const int _operatorTok = 10;
	public const int _LBRA = 11;
	public const int _RBRA = 12;
	public const int _LILB = 13;
	public const int _LIRB = 14;
	public const int _PIPE = 15;
	public const int _ARROW = 16;
	public const int _LAMBDA = 17;
	public const int _EQ = 18;
	public const int _SEQ = 19;
	public const int _MINUS = 20;
	public const int _COMPH = 21;
	public const int _COMPO = 22;
	public const int _DOT = 23;
	public const int _IN = 24;
	public const int _BASE = 25;
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
	public const int _ET = 42;
	public const int _EBLOCK = 43;
	public const int maxT = 84;

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
		Expect(43);
		if (!t.virt) scanner.PopIndent(); 
	}

	void Literal(out ElaExpression exp) {
		exp = null; 
		if (StartOf(1)) {
			Primitive(out exp);
		} else if (la.kind == 11) {
			RecordLiteral(out exp);
		} else if (la.kind == 13) {
			ListLiteral(out exp);
		} else if (la.kind == 45) {
			TupleLiteral(out exp);
		} else if (StartOf(2)) {
			SimpleExpr(out exp);
		} else SynErr(85);
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
		default: SynErr(86); break;
		}
	}

	void RecordLiteral(out ElaExpression exp) {
		exp = null; 
		var fld = default(ElaFieldDeclaration);
		
		Expect(11);
		var rec = new ElaRecordLiteral(t); exp = rec; 
		RecordField(out fld);
		rec.Fields.Add(fld); 
		while (la.kind == 47) {
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
		
		Expect(14);
	}

	void TupleLiteral(out ElaExpression exp) {
		var ot = default(Token);
		exp = null; 
		
		Expect(45);
		ot = t; 
		if (StartOf(4)) {
			GroupExpr(out exp);
		}
		Expect(46);
		if (exp == null)
		exp = new ElaUnitLiteral(ot);
		
	}

	void SimpleExpr(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 4) {
			ArgumentReference(out exp);
		} else if (la.kind == 25) {
			BaseReference(out exp);
		} else if (la.kind == 1 || la.kind == 44) {
			VariableReference(out exp);
		} else if (la.kind == 83) {
			LazyExpr(out exp);
		} else SynErr(87);
	}

	void ArgumentReference(out ElaExpression exp) {
		Expect(4);
		exp = new ElaArgument(t) { ArgumentName = t.val.Substring(1) }; 
	}

	void BaseReference(out ElaExpression exp) {
		var baseRef = default(ElaBaseReference);
		exp = null;
		
		Expect(25);
		baseRef = new ElaBaseReference(t); 
		Expect(23);
		var ot = t; 
		Expect(1);
		exp = new ElaFieldReference(ot) { FieldName = t.val, TargetObject = baseRef }; 
	}

	void VariableReference(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = new ElaVariableReference(t) { VariableName = t.val }; 
		} else if (la.kind == 44) {
			Get();
			exp = new ElaPlaceholder(t); 
		} else SynErr(88);
	}

	void LazyExpr(out ElaExpression exp) {
		Expect(83);
		var lazy = new ElaLazyLiteral(t); 
		Expr(out exp);
		var m = new ElaMatch(t);
		m.Entries.Add(new ElaMatchEntry { Pattern = new ElaUnitPattern(), Expression = exp });
		lazy.Body = m;
		exp = lazy;
		
		Expect(46);
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
		if (la.kind == 47) {
			var tuple = new ElaTupleLiteral(ot); 
			tuple.Parameters.Add(exp);
			exp = tuple; 
			
			Get();
			if (StartOf(4)) {
				Expr(out cexp);
				tuple.Parameters.Add(cexp); 
			}
			while (la.kind == 47) {
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
		} else if (la.kind == 29) {
			LetBinding(out exp);
		} else SynErr(89);
	}

	void MatchExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		Expect(26);
		var match = new ElaMatch(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(32);
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
		
		if (la.kind == 15) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(18);
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
		Expect(18);
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
		Expect(15);
		if (StartOf(9)) {
			BinaryExpr(out exp);
			while (la.kind == 47) {
				var old = exp; 
				Get();
				var ot = t; 
				BinaryExpr(out exp);
				exp = new ElaBinary(t) { Operator = ElaOperator.BooleanAnd, Left = old, Right = exp };
				
			}
		} else if (la.kind == 34) {
			Get();
			exp = new ElaOtherwiseGuard(t); 
		} else SynErr(90);
	}

	void WhereBinding(out ElaExpression exp) {
		scanner.InjectBlock(); 
		Expect(41);
		BindingBody(ElaVariableFlags.None, out exp);
		EndBlock();
	}

	void BinaryExpr(out ElaExpression exp) {
		var op = ElaOperator.Sequence; 
		exp = null;
		var ot = t; 
		
		if (StartOf(10)) {
			AssignExpr(out exp);
			while (la.kind == 19) {
				var cexp = default(ElaExpression); 
				Get();
				if (StartOf(10)) {
					AssignExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (la.kind == 19) {
			Get();
			if (StartOf(11)) {
				MulExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { 
					VariableName = "$"
				};
			
		} else SynErr(91);
	}

	void OrPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 2 || la.kind == 48) {
			VariantPattern(out pat);
		} else if (StartOf(12)) {
			AsPattern(out pat);
		} else SynErr(92);
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
		if (la.kind == 47) {
			TuplePattern(pat, out pat);
		}
	}

	void TuplePattern(ElaPattern prev, out ElaPattern pat) {
		var seq = new ElaTuplePattern(t); 
		seq.Patterns.Add(prev);
		var cpat = default(ElaPattern);
		pat = seq;
		
		Expect(47);
		if (StartOf(12)) {
			AsPattern(out cpat);
			if (la.kind == 51) {
				ConsPattern(cpat, out cpat);
			}
			seq.Patterns.Add(cpat); 
			while (la.kind == 47) {
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
		
		if (la.kind == 2) {
			Get();
			vp = new ElaVariantPattern(t) { Tag = t.val.Substring(1) }; 
			if (StartOf(12)) {
				AsPattern(out cpat);
				vp.Pattern = cpat; 
			}
			pat = vp; 
		} else if (la.kind == 48) {
			Get();
			vp = new ElaVariantPattern(t); 
			if (StartOf(12)) {
				AsPattern(out cpat);
				vp.Pattern = cpat; 
			}
			pat = vp; 
		} else SynErr(93);
	}

	void AsPattern(out ElaPattern pat) {
		pat = null; 
		SinglePattern(out pat);
		if (la.kind == 27) {
			Get();
			var asPat = new ElaAsPattern(t) { Pattern = pat }; 
			pat = asPat;				
			
			Expect(1);
			asPat.Name = t.val; 
		}
	}

	void FuncPattern(out ElaPattern pat) {
		AsPattern(out pat);
		if (la.kind == 51) {
			ConsPattern(pat, out pat);
		}
	}

	void SinglePattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 44: {
			DefaultPattern(out pat);
			break;
		}
		case 45: {
			UnitPattern(out pat);
			break;
		}
		case 6: case 7: case 8: case 9: case 38: case 39: {
			LiteralPattern(out pat);
			break;
		}
		case 13: {
			ListPattern(out pat);
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
		case 49: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(94); break;
		}
	}

	void DefaultPattern(out ElaPattern pat) {
		Expect(44);
		pat = new ElaDefaultPattern(t); 
	}

	void UnitPattern(out ElaPattern pat) {
		var ot = t;
		pat = null;
		
		Expect(45);
		if (StartOf(8)) {
			ParenPattern(out pat);
		}
		Expect(46);
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
		default: SynErr(95); break;
		}
		pat = new ElaLiteralPattern(t) { Value = lit };				
		
	}

	void ListPattern(out ElaPattern pat) {
		var cexp = default(ElaPattern); 
		var ht = default(ElaHeadTailPattern); 
		pat = null;
		
		Expect(13);
		if (StartOf(12)) {
			ht = new ElaHeadTailPattern(t);			
			
			AsPattern(out cexp);
			ht.Patterns.Add(cexp);  
			while (la.kind == 47) {
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

	void RecordPattern(out ElaPattern pat) {
		pat = null; 
		var cpat = default(ElaFieldPattern);
		
		Expect(11);
		var rec = new ElaRecordPattern(t); 
		pat = rec; 
		
		if (la.kind == 1 || la.kind == 8) {
			FieldPattern(out cpat);
			rec.Fields.Add(cpat); 
			while (la.kind == 47) {
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
		if (la.kind == 50) {
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
		
		Expect(49);
		if (la.kind == 1) {
			Get();
			eis.TypeAffinity = GetType(t.val); 
		} else if (la.kind == 45) {
			
			Get();
			Expect(1);
			tmp = Builtins.Trait(t.val); 
			
			if (tmp == ElaTraits.None) {
				AddError(ElaParserError.UnknownTrait, t.val);
			}
			
			eis.Traits |= tmp;
			
			while (la.kind == 47) {
				Get();
				Expect(1);
				tmp = Builtins.Trait(t.val); 
				
				if (tmp == ElaTraits.None) {
					AddError(ElaParserError.UnknownTrait, t.val);
				}
				
				eis.Traits |= tmp;
				
			}
			Expect(46);
		} else SynErr(96);
	}

	void BindingPattern(out ElaPattern pat) {
		pat = null; 
		switch (la.kind) {
		case 44: {
			DefaultPattern(out pat);
			break;
		}
		case 45: {
			UnitPattern(out pat);
			break;
		}
		case 6: case 7: case 8: case 9: case 38: case 39: {
			LiteralPattern(out pat);
			break;
		}
		case 13: {
			ListPattern(out pat);
			break;
		}
		case 11: {
			RecordPattern(out pat);
			break;
		}
		case 49: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(97); break;
		}
		if (la.kind == 27) {
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
		} else if (la.kind == 11) {
			RecordPattern(out pat);
		} else if (la.kind == 13) {
			ListPattern(out pat);
		} else if (la.kind == 45) {
			UnitPattern(out pat);
		} else SynErr(98);
	}

	void IsOperatorPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 49) {
			TypeCheckPattern(out pat);
		} else if (StartOf(1)) {
			LiteralPattern(out pat);
		} else if (la.kind == 11) {
			RecordPattern(out pat);
		} else if (la.kind == 13) {
			ListPattern(out pat);
		} else if (la.kind == 45) {
			UnitPattern(out pat);
		} else SynErr(99);
	}

	void FieldPattern(out ElaFieldPattern fld) {
		fld = null;
		var cpat = default(ElaPattern);
		
		if (la.kind == 8) {
			Get();
			fld = new ElaFieldPattern(t) { Name = ReadString(t.val) }; 
			Expect(18);
			AsPattern(out cpat);
			fld.Value = cpat; 
		} else if (la.kind == 1) {
			Get();
			fld = new ElaFieldPattern(t) { Name = t.val }; 
			if (la.kind == 18) {
				Get();
				AsPattern(out cpat);
			}
			if (cpat == null)
			cpat = new ElaVariablePattern(t) { Name = fld.Name };
			
			fld.Value = cpat; 
			
		} else SynErr(100);
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
			if (la.kind == 18) {
				Get();
				Expr(out cexp);
				fld.FieldValue = cexp; 
			}
			if (fld.FieldValue == null)
			fld.FieldValue = new ElaVariableReference(t) { VariableName = t.val };
			
		} else if (la.kind == 8) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = ReadString(t.val), Mutable = mutable }; 
			Expect(18);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else SynErr(101);
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
		if (la.kind == 22 || la.kind == 47 || la.kind == 53) {
			if (la.kind == 22) {
				ComprehensionExpr(exp, out comp);
			} else if (la.kind == 53) {
				RangeExpr(exp, null, out rng);
			} else {
				var oexp = exp; 
				Get();
				Expr(out exp);
				if (la.kind == 53) {
					RangeExpr(oexp, exp, out rng);
				} else if (la.kind == 14 || la.kind == 47) {
					list = new List<ElaExpression>();
					list.Add(oexp);
					list.Add(exp);
					
					while (la.kind == 47) {
						Get();
						Expr(out exp);
						list.Add(exp); 
					}
				} else SynErr(102);
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
		
		Expect(22);
		ComprehensionEntry(sel, out it);
		exp = new ElaComprehension(ot) { Generator = it }; 
	}

	void LetBinding(out ElaExpression exp) {
		exp = null; 
		var inExp = default(ElaExpression);
		var flags = default(ElaVariableFlags);
		
		Expect(29);
		if (la.kind == 30) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		Expect(24);
		Expr(out inExp);
		((ElaBinding)exp).In = inExp; 
	}

	void VariableAttributes(out ElaVariableFlags flags) {
		Expect(30);
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
			if (StartOf(13)) {
				if (StartOf(12)) {
					FunExpr(varExp);
				} else if (la.kind == 15) {
					BindingBodyGuards(varExp);
				} else {
					BindingBodyInit(varExp);
				}
			}
			SetObjectMetadata(varExp, cexp); 
		} else if (StartOf(14)) {
			BindingPattern(out pat);
			varExp.Pattern = pat; 
			if (la.kind == 18) {
				BindingBodyInit(varExp);
			}
		} else if (StartOf(15)) {
			BindingBodyOperator(varExp);
		} else SynErr(103);
		if (la.kind == 42) {
			ExpectWeak(42, 16);
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
		Expect(29);
		if (la.kind == 30) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		if (la.kind == 24) {
			ExpectWeak(24, 17);
			Expr(out inExp);
			((ElaBinding)exp).In = inExp; 
			EndBlock();
		} else if (la.kind == 43) {
			EndBlock();
		} else SynErr(104);
	}

	void BindingGuard(out ElaExpression exp) {
		exp = null; 
		BinaryExpr(out exp);
		while (la.kind == 47) {
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
		
		Expect(15);
		if (StartOf(9)) {
			var newCond = new ElaCondition(t);
			cond.False = newCond;
			cond = newCond;
			
			BindingGuard(out gexp);
			cond.Condition = gexp; 
			Expect(18);
			Expr(out cexp);
			cond.True = cexp; 
			if (la.kind == 15) {
				BindingGuardList(ref cond);
			}
		} else if (la.kind == 34) {
			Get();
			Expect(18);
			Expr(out cexp);
			cond.False = cexp; 
		} else SynErr(105);
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
		
		Expect(15);
		BindingGuard(out gexp);
		cond.Condition = gexp; 
		Expect(18);
		Expr(out cexp3);
		cond.True = cexp3; 
		BindingGuardList(ref cond);
	}

	void BindingBodyInit(ElaBinding varExp) {
		var cexp = default(ElaExpression); 
		var cexp2 = default(ElaExpression);
		
		Expect(18);
		Expr(out cexp);
		varExp.InitExpression = cexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp2);
			varExp.Where = (ElaBinding)cexp2; 
		}
	}

	void BindingBodyOperator(ElaBinding varExp) {
		switch (la.kind) {
		case 10: {
			Get();
			break;
		}
		case 55: {
			Get();
			break;
		}
		case 56: {
			Get();
			break;
		}
		case 57: {
			Get();
			break;
		}
		case 58: {
			Get();
			break;
		}
		case 59: {
			Get();
			break;
		}
		case 60: {
			Get();
			break;
		}
		case 61: {
			Get();
			break;
		}
		case 20: {
			Get();
			break;
		}
		case 62: {
			Get();
			break;
		}
		case 63: {
			Get();
			break;
		}
		case 64: {
			Get();
			break;
		}
		case 65: {
			Get();
			break;
		}
		case 66: {
			Get();
			break;
		}
		case 67: {
			Get();
			break;
		}
		case 68: {
			Get();
			break;
		}
		case 69: {
			Get();
			break;
		}
		case 70: {
			Get();
			break;
		}
		case 71: {
			Get();
			break;
		}
		case 72: {
			Get();
			break;
		}
		case 73: {
			Get();
			break;
		}
		case 51: {
			Get();
			break;
		}
		default: SynErr(106); break;
		}
		varExp.VariableName = t.val; 
		FunExpr(varExp);
		var fun = (ElaFunctionLiteral)varExp.InitExpression;
		fun.FunctionType = ElaFunctionType.Operator;
		
	}

	void FunName(ElaFunctionLiteral fun) {
		if (la.kind == 1) {
			Get();
		} else if (la.kind == 10) {
			Get();
		} else SynErr(107);
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
		while (StartOf(12)) {
			FuncPattern(out pat);
			if (seq == null)
			{
				seq = new ElaPatternGroup(ot);
				seq.Patterns.Add(entry.Pattern);
				entry.Pattern = seq;							
			}
			
				seq.Patterns.Add(pat); 
			
		}
		if (la.kind == 15) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(18);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (la.kind == 1 || la.kind == 10 || la.kind == 15) {
			if (la.kind == 1 || la.kind == 10) {
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
		Expect(18);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 41) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
		if (la.kind == 1 || la.kind == 10 || la.kind == 15) {
			if (la.kind == 1 || la.kind == 10) {
				scanner.InjectBlock(); 
				FunName(fun);
				FunBodyExpr(fun);
			} else {
				ChildFunBodyExpr(fun);
			}
		}
	}

	void LambdaExpr(out ElaExpression exp) {
		Expect(17);
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
		while (StartOf(12)) {
			FuncPattern(out pat);
			if (seq == null)
			{
				seq = new ElaPatternGroup(ot);
				seq.Patterns.Add(entry.Pattern);
				entry.Pattern = seq;							
			}
			
				seq.Patterns.Add(pat); 
			
		}
		if (la.kind == 15) {
			var cexp = default(ElaExpression); 
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(16);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
	}

	void IncludeStat(out ElaExpression exp) {
		exp = null; 
		scanner.InjectBlock(); 
		Expect(31);
		var inc = new ElaModuleInclude(t); 
		Qualident(inc.Path);
		var name = inc.Path[inc.Path.Count - 1];				
		inc.Path.RemoveAt(inc.Path.Count - 1);				
		inc.Alias = inc.Name = name;
		exp = inc;
		
		if (la.kind == 74) {
			Get();
			if (la.kind == 1) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 8) {
				Get();
				inc.DllName = ReadString(t.val); 
			} else SynErr(108);
		}
		if (la.kind == 27) {
			Get();
			Expect(1);
			inc.Alias = t.val; 
		}
		Expect(43);
	}

	void Qualident(List<String> path ) {
		var val = String.Empty; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 8) {
			Get();
			val = ReadString(t.val); 
		} else SynErr(109);
		path.Add(val); 
		if (la.kind == 23) {
			Get();
			Qualident(path);
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
		ExpectWeak(34, 17);
		Expr(out cexp);
		cond.False = cexp; 
	}

	void RaiseExpr(out ElaExpression exp) {
		Expect(36);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		var code = String.Empty;
		
		Expect(1);
		code = t.val; 
		if (la.kind == 45) {
			Get();
			Expr(out cexp);
			Expect(46);
		}
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
		while (StartOf(8)) {
			scanner.InjectBlock(); 
			MatchEntry(match);
			EndBlock();
		}
		EndBlock();
	}

	void AssignExpr(out ElaExpression exp) {
		BackwardPipeExpr(out exp);
		while (la.kind == 75 || la.kind == 76) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 75) {
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
		while (la.kind == 77) {
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
		while (la.kind == 78) {
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
		while (la.kind == 79) {
			var cexp = default(ElaExpression); 
			Get();
			AndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanOr, Right = cexp }; 
			
		}
	}

	void AndExpr(out ElaExpression exp) {
		EqExpr(out exp);
		while (la.kind == 80) {
			var cexp = default(ElaExpression); 
			Get();
			EqExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanAnd, Right = cexp }; 
			
		}
	}

	void EqExpr(out ElaExpression exp) {
		var op = default(ElaOperator); 
		exp = null;
		var ot = t; 
		
		if (StartOf(18)) {
			ShiftExpr(out exp);
			while (StartOf(19)) {
				var cexp = default(ElaExpression); 
				switch (la.kind) {
				case 55: {
					Get();
					op = ElaOperator.Equals; 
					break;
				}
				case 56: {
					Get();
					op = ElaOperator.NotEquals; 
					break;
				}
				case 59: {
					Get();
					op = ElaOperator.Greater; 
					break;
				}
				case 60: {
					Get();
					op = ElaOperator.Lesser; 
					break;
				}
				case 57: {
					Get();
					op = ElaOperator.GreaterEqual; 
					break;
				}
				case 58: {
					Get();
					op = ElaOperator.LesserEqual; 
					break;
				}
				}
				if (StartOf(18)) {
					ShiftExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (StartOf(19)) {
			switch (la.kind) {
			case 55: {
				Get();
				op = ElaOperator.Equals; 
				break;
			}
			case 56: {
				Get();
				op = ElaOperator.NotEquals; 
				break;
			}
			case 59: {
				Get();
				op = ElaOperator.Greater; 
				break;
			}
			case 60: {
				Get();
				op = ElaOperator.Lesser; 
				break;
			}
			case 57: {
				Get();
				op = ElaOperator.GreaterEqual; 
				break;
			}
			case 58: {
				Get();
				op = ElaOperator.LesserEqual; 
				break;
			}
			}
			if (StartOf(18)) {
				ShiftExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { 
					VariableName = t.val
				};
			
		} else SynErr(110);
	}

	void ShiftExpr(out ElaExpression exp) {
		var op = default(ElaOperator); 
		exp = null;
		var ot = t; 
		
		if (StartOf(20)) {
			ConcatExpr(out exp);
			while (la.kind == 70 || la.kind == 71) {
				var cexp = default(ElaExpression); 
				if (la.kind == 70) {
					Get();
					op = ElaOperator.ShiftRight; 
				} else {
					Get();
					op = ElaOperator.ShiftLeft; 
				}
				if (StartOf(20)) {
					ConcatExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (la.kind == 70 || la.kind == 71) {
			if (la.kind == 70) {
				Get();
				op = ElaOperator.ShiftRight; 
			} else {
				Get();
				op = ElaOperator.ShiftLeft; 
			}
			if (StartOf(20)) {
				ConcatExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { 
					VariableName = t.val
				};
			
		} else SynErr(111);
	}

	void ConcatExpr(out ElaExpression exp) {
		var op = default(ElaOperator); 
		exp = null;
		var ot = t; 
		
		if (StartOf(21)) {
			ConsExpr(out exp);
			while (la.kind == 73) {
				var cexp = default(ElaExpression); 
				Get();
				op = ElaOperator.Concat; 
				if (StartOf(21)) {
					ConsExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (la.kind == 73) {
			Get();
			op = ElaOperator.Concat; 
			if (StartOf(21)) {
				ConsExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(112);
	}

	void ConsExpr(out ElaExpression exp) {
		var op = default(ElaOperator); 
		exp = null;
		var ot = t; 
		
		if (StartOf(22)) {
			AddExpr(out exp);
			while (la.kind == 51) {
				var cexp = default(ElaExpression); 
				Get();
				op = ElaOperator.Cons; 
				if (StartOf(21)) {
					ConsExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp };
				
			}
		} else if (la.kind == 51) {
			Get();
			op = ElaOperator.Cons; 
			if (StartOf(22)) {
				AddExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(113);
	}

	void AddExpr(out ElaExpression exp) {
		var op = default(ElaOperator); 
		exp = null;
		var ot = t; 
		
		if (StartOf(11)) {
			MulExpr(out exp);
			while (la.kind == 20 || la.kind == 61) {
				var cexp = default(ElaExpression); 
				if (la.kind == 61) {
					Get();
					op = ElaOperator.Add; 
				} else {
					Get();
					op = ElaOperator.Subtract; 
				}
				if (StartOf(11)) {
					MulExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (la.kind == 20 || la.kind == 61) {
			if (la.kind == 61) {
				Get();
				op = ElaOperator.Add; 
			} else {
				Get();
				op = ElaOperator.Subtract; 
			}
			if (StartOf(11)) {
				MulExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(114);
	}

	void MulExpr(out ElaExpression exp) {
		var op = default(ElaOperator); 
		exp = null;
		var ot = t; 
		
		if (StartOf(23)) {
			CastExpr(out exp);
			while (StartOf(24)) {
				var cexp = default(ElaExpression); 
				if (la.kind == 62) {
					Get();
					op = ElaOperator.Multiply; 
				} else if (la.kind == 63) {
					Get();
					op = ElaOperator.Divide; 
				} else if (la.kind == 64) {
					Get();
					op = ElaOperator.Remainder; 
				} else {
					Get();
					op = ElaOperator.Power; 
				}
				if (StartOf(23)) {
					CastExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (StartOf(24)) {
			if (la.kind == 62) {
				Get();
				op = ElaOperator.Multiply; 
			} else if (la.kind == 63) {
				Get();
				op = ElaOperator.Divide; 
			} else if (la.kind == 64) {
				Get();
				op = ElaOperator.Remainder; 
			} else {
				Get();
				op = ElaOperator.Power; 
			}
			if (StartOf(23)) {
				CastExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(115);
	}

	void CastExpr(out ElaExpression exp) {
		InfixExpr(out exp);
		while (la.kind == 21 || la.kind == 28 || la.kind == 50) {
			if (la.kind == 50) {
				Get();
				Expect(1);
				exp = new ElaCast(t) { CastAffinity = GetType(t.val), Expression = exp };
			} else if (la.kind == 28) {
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
		
		if (StartOf(25)) {
			BitOrExpr(out exp);
			while (la.kind == 5 || la.kind == 10) {
				var cexp = default(ElaExpression); 
				ot = t;
				
				if (la.kind == 10) {
					Get();
					name = t.val; 
				} else {
					Get();
					name = t.val.Trim('`'); 
				}
				if (StartOf(25)) {
					BitOrExpr(out cexp);
				}
				var fc = new ElaFunctionCall(ot) { 
					Target = new ElaVariableReference(t) { VariableName = name }
				};
				fc.Parameters.Add(exp);			
				
				if (cexp != null)
					fc.Parameters.Add(cexp);
								
				exp = fc;
				
			}
		} else if (la.kind == 5 || la.kind == 10) {
			if (la.kind == 10) {
				Get();
				name = t.val; 
			} else {
				Get();
				name = t.val.Trim('`'); 
			}
			if (StartOf(25)) {
				BitOrExpr(out exp);
				exp = GetPrefixFun(name, exp, true);	
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = name };
			
		} else SynErr(116);
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

	void BitOrExpr(out ElaExpression exp) {
		var op = ElaOperator.BitwiseOr; 
		exp = null;
		var ot = t; 
		
		if (StartOf(26)) {
			BitXorExpr(out exp);
			while (la.kind == 68) {
				var cexp = default(ElaExpression); 
				Get();
				if (StartOf(26)) {
					BitXorExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { 
						Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (la.kind == 68) {
			Get();
			if (StartOf(26)) {
				BitXorExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(117);
	}

	void BitXorExpr(out ElaExpression exp) {
		var op = ElaOperator.BitwiseXor; 
		exp = null;
		var ot = t; 
		
		if (StartOf(27)) {
			BitAndExpr(out exp);
			while (la.kind == 69) {
				var cexp = default(ElaExpression); 
				Get();
				if (StartOf(27)) {
					BitAndExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { 
						Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (la.kind == 69) {
			Get();
			if (StartOf(27)) {
				BitAndExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(118);
	}

	void BitAndExpr(out ElaExpression exp) {
		var op = ElaOperator.BitwiseAnd; 
		exp = null;
		var ot = t; 
		
		if (StartOf(28)) {
			BackwardCompExpr(out exp);
			while (la.kind == 67) {
				var cexp = default(ElaExpression); 
				Get();
				if (StartOf(28)) {
					BackwardCompExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { 
						Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (la.kind == 67) {
			Get();
			if (StartOf(28)) {
				BackwardCompExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(119);
	}

	void BackwardCompExpr(out ElaExpression exp) {
		var op = default(ElaOperator); 
		exp = null;
		var ot = t; 
		
		if (StartOf(29)) {
			ForwardCompExpr(out exp);
			while (la.kind == 81) {
				var cexp = default(ElaExpression); 
				Get();
				op = ElaOperator.CompBackward; 
				if (StartOf(28)) {
					BackwardCompExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, true);
				else
					exp = new ElaBinary(t) { Right = exp, Operator = op, Left = cexp }; 
				
			}
		} else if (la.kind == 81) {
			Get();
			op = ElaOperator.CompBackward; 
			if (StartOf(29)) {
				ForwardCompExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(120);
	}

	void ForwardCompExpr(out ElaExpression exp) {
		var op = default(ElaOperator); 
		exp = null;
		var ot = t; 
		
		if (StartOf(30)) {
			UnaryExpr(out exp);
			while (la.kind == 82) {
				var cexp = default(ElaExpression); 
				Get();
				op = ElaOperator.CompForward; 
				if (StartOf(30)) {
					UnaryExpr(out cexp);
				}
				if (cexp == null)
				exp = GetOperatorFun(exp, op, false);
				else
					exp = new ElaBinary(t) { Left = exp, Operator = op, Right = cexp }; 
				
			}
		} else if (la.kind == 82) {
			Get();
			op = ElaOperator.CompForward; 
			if (StartOf(30)) {
				UnaryExpr(out exp);
				exp = GetOperatorFun(exp, op, true); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(121);
	}

	void UnaryExpr(out ElaExpression exp) {
		var op = default(ElaOperator); 
		exp = null;
		var ot = t; 
		
		if (StartOf(31)) {
			Application(out exp);
			while (la.kind == 66 || la.kind == 72) {
				if (la.kind == 66) {
					Get();
					op = ElaOperator.Negate; 
				} else {
					Get();
					op = ElaOperator.BitwiseNot; 
				}
				exp = new ElaBinary(t) { Left = exp, Operator = op }; 
			}
		} else if (la.kind == 66 || la.kind == 72) {
			if (la.kind == 66) {
				Get();
				op = ElaOperator.Negate; 
			} else {
				Get();
				op = ElaOperator.BitwiseNot; 
			}
			if (StartOf(31)) {
				Application(out exp);
				exp = new ElaBinary(t) { Left = exp, Operator = op }; 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(122);
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
		} else SynErr(123);
	}

	void AccessExpr(out ElaExpression exp) {
		Literal(out exp);
		while (la.kind == 23) {
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
			} else SynErr(124);
		}
	}

	void EmbExpr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 1: case 2: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 13: case 19: case 20: case 25: case 38: case 39: case 44: case 45: case 51: case 55: case 56: case 57: case 58: case 59: case 60: case 61: case 62: case 63: case 64: case 65: case 66: case 67: case 68: case 69: case 70: case 71: case 72: case 73: case 81: case 82: case 83: {
			BinaryExpr(out exp);
			break;
		}
		case 33: {
			IfExpr(out exp);
			break;
		}
		case 17: {
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
		default: SynErr(125); break;
		}
	}

	void ComprehensionEntry(ElaExpression body, out ElaGenerator it) {
		it = new ElaGenerator(t);
		it.Body = body;
		var cexp = default(ElaExpression);
		var pat = default(ElaPattern);
		
		GeneratorPattern(out pat);
		Expect(75);
		Expr(out cexp);
		it.Pattern = pat;
		it.Target = cexp;
		
		if (la.kind == 15 || la.kind == 47) {
			if (la.kind == 47) {
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
		while (StartOf(32)) {
			DeclarationBlock(b);
		}
	}

	void DeclarationBlock(ElaBlock b) {
		var exp = default(ElaExpression); 
		if (la.kind == 29) {
			RootLetBinding(out exp);
		} else if (la.kind == 31) {
			IncludeStat(out exp);
		} else if (StartOf(6)) {
			EmbExpr(out exp);
			if (FALSE) 
			Expect(43);
		} else SynErr(126);
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,T,x,x, x,T,x,x, T,T,T,T, T,x,x,x, T,T,x,x, x,x,x,T, x,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,T,x,x, x,T,x,x, T,T,T,T, T,x,x,x, T,T,x,x, x,x,x,T, x,x,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,x,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,T,x,x, T,T,T,T, T,x,x,x, T,T,x,x, x,x,x,T, x,x,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, x,x,T,T, T,T,x,T, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,T, x,x,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,T, x,x,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,x,x, x,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,x,x, x,x,T,T, T,T,x,T, x,T,x,T, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x},
		{T,T,x,x, x,x,T,T, T,T,T,T, x,T,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,T,x,T, x,x,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x},
		{T,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,T,x,x, x,T,x,x, T,T,T,T, T,x,x,x, T,T,x,x, x,x,x,T, x,x,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,x,x, T,T,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,T,x,x, T,x,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, T,x,x,x, x,x,x,x, x,T,T,T, x,x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,T,T, x,x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x},
		{x,T,T,x, T,T,T,T, T,T,T,T, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,T,x,T, x,T,x,x, T,T,T,T, T,x,x,x, T,T,x,x, x,x,x,T, x,x,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x, x,T,T,T, x,x}

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
			case 10: s = "operatorTok expected"; break;
			case 11: s = "LBRA expected"; break;
			case 12: s = "RBRA expected"; break;
			case 13: s = "LILB expected"; break;
			case 14: s = "LIRB expected"; break;
			case 15: s = "PIPE expected"; break;
			case 16: s = "ARROW expected"; break;
			case 17: s = "LAMBDA expected"; break;
			case 18: s = "EQ expected"; break;
			case 19: s = "SEQ expected"; break;
			case 20: s = "MINUS expected"; break;
			case 21: s = "COMPH expected"; break;
			case 22: s = "COMPO expected"; break;
			case 23: s = "DOT expected"; break;
			case 24: s = "IN expected"; break;
			case 25: s = "BASE expected"; break;
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
			case 42: s = "ET expected"; break;
			case 43: s = "EBLOCK expected"; break;
			case 44: s = "\"_\" expected"; break;
			case 45: s = "\"(\" expected"; break;
			case 46: s = "\")\" expected"; break;
			case 47: s = "\",\" expected"; break;
			case 48: s = "\"`\" expected"; break;
			case 49: s = "\"?\" expected"; break;
			case 50: s = "\":\" expected"; break;
			case 51: s = "\"::\" expected"; break;
			case 52: s = "\"!\" expected"; break;
			case 53: s = "\"..\" expected"; break;
			case 54: s = "\"&\" expected"; break;
			case 55: s = "\"==\" expected"; break;
			case 56: s = "\"<>\" expected"; break;
			case 57: s = "\">=\" expected"; break;
			case 58: s = "\"<=\" expected"; break;
			case 59: s = "\">\" expected"; break;
			case 60: s = "\"<\" expected"; break;
			case 61: s = "\"+\" expected"; break;
			case 62: s = "\"*\" expected"; break;
			case 63: s = "\"/\" expected"; break;
			case 64: s = "\"%\" expected"; break;
			case 65: s = "\"**\" expected"; break;
			case 66: s = "\"--\" expected"; break;
			case 67: s = "\"&&&\" expected"; break;
			case 68: s = "\"|||\" expected"; break;
			case 69: s = "\"^^^\" expected"; break;
			case 70: s = "\">>>\" expected"; break;
			case 71: s = "\"<<<\" expected"; break;
			case 72: s = "\"~~~\" expected"; break;
			case 73: s = "\"++\" expected"; break;
			case 74: s = "\"#\" expected"; break;
			case 75: s = "\"<-\" expected"; break;
			case 76: s = "\"<->\" expected"; break;
			case 77: s = "\"<|\" expected"; break;
			case 78: s = "\"|>\" expected"; break;
			case 79: s = "\"||\" expected"; break;
			case 80: s = "\"&&\" expected"; break;
			case 81: s = "\"<<\" expected"; break;
			case 82: s = "\">>\" expected"; break;
			case 83: s = "\"(&\" expected"; break;
			case 84: s = "??? expected"; break;
			case 85: s = "invalid Literal"; break;
			case 86: s = "invalid Primitive"; break;
			case 87: s = "invalid SimpleExpr"; break;
			case 88: s = "invalid VariableReference"; break;
			case 89: s = "invalid Expr"; break;
			case 90: s = "invalid Guard"; break;
			case 91: s = "invalid BinaryExpr"; break;
			case 92: s = "invalid OrPattern"; break;
			case 93: s = "invalid VariantPattern"; break;
			case 94: s = "invalid SinglePattern"; break;
			case 95: s = "invalid LiteralPattern"; break;
			case 96: s = "invalid TypeCheckPattern"; break;
			case 97: s = "invalid BindingPattern"; break;
			case 98: s = "invalid GeneratorPattern"; break;
			case 99: s = "invalid IsOperatorPattern"; break;
			case 100: s = "invalid FieldPattern"; break;
			case 101: s = "invalid RecordField"; break;
			case 102: s = "invalid ParamList"; break;
			case 103: s = "invalid BindingBody"; break;
			case 104: s = "invalid RootLetBinding"; break;
			case 105: s = "invalid BindingGuardList"; break;
			case 106: s = "invalid BindingBodyOperator"; break;
			case 107: s = "invalid FunName"; break;
			case 108: s = "invalid IncludeStat"; break;
			case 109: s = "invalid Qualident"; break;
			case 110: s = "invalid EqExpr"; break;
			case 111: s = "invalid ShiftExpr"; break;
			case 112: s = "invalid ConcatExpr"; break;
			case 113: s = "invalid ConsExpr"; break;
			case 114: s = "invalid AddExpr"; break;
			case 115: s = "invalid MulExpr"; break;
			case 116: s = "invalid InfixExpr"; break;
			case 117: s = "invalid BitOrExpr"; break;
			case 118: s = "invalid BitXorExpr"; break;
			case 119: s = "invalid BitAndExpr"; break;
			case 120: s = "invalid BackwardCompExpr"; break;
			case 121: s = "invalid ForwardCompExpr"; break;
			case 122: s = "invalid UnaryExpr"; break;
			case 123: s = "invalid Application"; break;
			case 124: s = "invalid AccessExpr"; break;
			case 125: s = "invalid EmbExpr"; break;
			case 126: s = "invalid DeclarationBlock"; break;

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

