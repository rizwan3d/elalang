
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
	public const int _operatorTok1 = 9;
	public const int _operatorTok2 = 10;
	public const int _operatorTok3 = 11;
	public const int _operatorTok4 = 12;
	public const int _operatorTok5 = 13;
	public const int _operatorTok6 = 14;
	public const int _operatorTok7 = 15;
	public const int _operatorTok8 = 16;
	public const int _LBRA = 17;
	public const int _RBRA = 18;
	public const int _LILB = 19;
	public const int _LIRB = 20;
	public const int _PIPE = 21;
	public const int _ARROW = 22;
	public const int _LAMBDA = 23;
	public const int _COMPH = 24;
	public const int _COMPO = 25;
	public const int _DOT = 26;
	public const int _IN = 27;
	public const int _BASE = 28;
	public const int _MATCH = 29;
	public const int _ASAMP = 30;
	public const int _IS = 31;
	public const int _LET = 32;
	public const int _PRIVATE = 33;
	public const int _OPEN = 34;
	public const int _WITH = 35;
	public const int _IFS = 36;
	public const int _ELSE = 37;
	public const int _THEN = 38;
	public const int _RAISE = 39;
	public const int _TRY = 40;
	public const int _TRUE = 41;
	public const int _FALSE = 42;
	public const int _FAIL = 43;
	public const int _WHERE = 44;
	public const int _ET = 45;
	public const int _EBLOCK = 46;
	public const int maxT = 70;

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
		if (StartOf(1)) {
			Primitive(out exp);
		} else if (la.kind == 17) {
			RecordLiteral(out exp);
		} else if (la.kind == 19) {
			ListLiteral(out exp);
		} else if (la.kind == 48) {
			TupleLiteral(out exp);
		} else if (StartOf(2)) {
			SimpleExpr(out exp);
		} else SynErr(71);
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
		case 41: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(true) }; 
			break;
		}
		case 42: {
			Get();
			exp = new ElaPrimitive(t) { Value = new ElaLiteralValue(false) }; 
			break;
		}
		default: SynErr(72); break;
		}
	}

	void RecordLiteral(out ElaExpression exp) {
		exp = null; 
		var fld = default(ElaFieldDeclaration);
		
		Expect(17);
		var rec = new ElaRecordLiteral(t); exp = rec; 
		RecordField(out fld);
		rec.Fields.Add(fld); 
		while (la.kind == 50) {
			Get();
			RecordField(out fld);
			rec.Fields.Add(fld); 
		}
		Expect(18);
	}

	void ListLiteral(out ElaExpression exp) {
		Expect(19);
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
		
		Expect(20);
	}

	void TupleLiteral(out ElaExpression exp) {
		var ot = default(Token);
		exp = null; 
		
		Expect(48);
		ot = t; 
		if (StartOf(4)) {
			GroupExpr(out exp);
		}
		Expect(49);
		if (exp == null)
		exp = new ElaUnitLiteral(ot);
		
	}

	void SimpleExpr(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 3) {
			ArgumentReference(out exp);
		} else if (la.kind == 28) {
			BaseReference(out exp);
		} else if (la.kind == 1 || la.kind == 47) {
			VariableReference(out exp);
		} else if (la.kind == 69) {
			LazyExpr(out exp);
		} else SynErr(73);
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
		Expect(26);
		var ot = t; 
		Expect(1);
		exp = new ElaFieldReference(ot) { FieldName = t.val, TargetObject = baseRef }; 
	}

	void VariableReference(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = new ElaVariableReference(t) { VariableName = t.val }; 
		} else if (la.kind == 47) {
			Get();
			exp = new ElaPlaceholder(t); 
		} else SynErr(74);
	}

	void LazyExpr(out ElaExpression exp) {
		Expect(69);
		var lazy = new ElaLazyLiteral(t); 
		Expr(out exp);
		var m = new ElaMatch(t);
		m.Entries.Add(new ElaMatchEntry { Pattern = new ElaUnitPattern(), Expression = exp });
		lazy.Body = m;
		exp = lazy;
		
		Expect(49);
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
		if (la.kind == 50) {
			var tuple = new ElaTupleLiteral(ot); 
			tuple.Parameters.Add(exp);
			exp = tuple; 
			
			Get();
			if (StartOf(4)) {
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

	void Expr(out ElaExpression exp) {
		exp = null; 
		if (StartOf(6)) {
			EmbExpr(out exp);
		} else if (la.kind == 32) {
			LetBinding(out exp);
		} else SynErr(75);
	}

	void MatchExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 29)) {SynErr(76); Get();}
		Expect(29);
		var match = new ElaMatch(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(35);
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
		
		if (la.kind == 21) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(51);
		Expr(out cexp);
		entry.Expression = cexp; 
		if (la.kind == 44) {
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
		if (la.kind == 44) {
			WhereBinding(out cexp);
			entry.Where = cexp; 
		}
		if (RequireEndBlock()) 
		EndBlock();
	}

	void RootPattern(out ElaPattern pat) {
		OrPattern(out pat);
		if (la.kind == 55) {
			ConsPattern(pat, out pat);
		}
	}

	void Guard(out ElaExpression exp) {
		exp = null; 
		Expect(21);
		if (StartOf(9)) {
			BinaryExpr(out exp);
			while (la.kind == 50) {
				var old = exp; 
				Get();
				var ot = t; 
				BinaryExpr(out exp);
				exp = new ElaBinary(t) { Operator = ElaOperator.BooleanAnd, Left = old, Right = exp };
				
			}
		} else if (la.kind == 37) {
			Get();
			exp = new ElaOtherwiseGuard(t); 
		} else SynErr(77);
	}

	void WhereBinding(out ElaExpression exp) {
		var flags = default(ElaVariableFlags); 
		scanner.InjectBlock(); 
		
		while (!(la.kind == 0 || la.kind == 44)) {SynErr(78); Get();}
		Expect(44);
		if (la.kind == 33 || la.kind == 59) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		EndBlock();
	}

	void BinaryExpr(out ElaExpression exp) {
		exp = null;
		var ot = t; 
		
		AssignExpr(out exp);
		while (la.kind == 68) {
			var cexp = default(ElaExpression); 
			Get();
			AssignExpr(out cexp);
			exp = new ElaBinary(t) { Operator = ElaOperator.Sequence, Left = exp, Right = cexp };
			
		}
	}

	void OrPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 2 || la.kind == 52) {
			VariantPattern(out pat);
		} else if (StartOf(10)) {
			AsPattern(out pat);
		} else SynErr(79);
	}

	void ConsPattern(ElaPattern prev, out ElaPattern exp) {
		var cexp = default(ElaPattern); 
		var ht = new ElaHeadTailPattern(t); 
		ht.Patterns.Add(prev);
		exp = ht;				
		
		Expect(55);
		AsPattern(out cexp);
		ht.Patterns.Add(cexp); 
		while (la.kind == 55) {
			Get();
			AsPattern(out cexp);
			ht.Patterns.Add(cexp); 
		}
	}

	void ParenPattern(out ElaPattern pat) {
		pat = null; 
		OrPattern(out pat);
		if (la.kind == 55) {
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
		if (StartOf(10)) {
			AsPattern(out cpat);
			if (la.kind == 55) {
				ConsPattern(cpat, out cpat);
			}
			seq.Patterns.Add(cpat); 
			while (la.kind == 50) {
				Get();
				AsPattern(out cpat);
				if (la.kind == 55) {
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
		} else if (la.kind == 52) {
			Get();
			vp = new ElaVariantPattern(t); 
			if (StartOf(10)) {
				AsPattern(out cpat);
				vp.Pattern = cpat; 
			}
			pat = vp; 
		} else SynErr(80);
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
		if (la.kind == 55) {
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
		case 5: case 6: case 7: case 8: case 41: case 42: {
			LiteralPattern(out pat);
			break;
		}
		case 19: {
			ListPattern(out pat);
			break;
		}
		case 17: {
			RecordPattern(out pat);
			break;
		}
		case 1: {
			IdentPattern(out pat);
			break;
		}
		case 53: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(81); break;
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
			ParenPattern(out pat);
		}
		Expect(49);
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
		case 41: {
			Get();
			lit = new ElaLiteralValue(true); 
			break;
		}
		case 42: {
			Get();
			lit = new ElaLiteralValue(false); 
			break;
		}
		default: SynErr(82); break;
		}
		pat = new ElaLiteralPattern(t) { Value = lit };				
		
	}

	void ListPattern(out ElaPattern pat) {
		var cexp = default(ElaPattern); 
		var ht = default(ElaHeadTailPattern); 
		pat = null;
		
		Expect(19);
		if (StartOf(10)) {
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
		
		Expect(20);
	}

	void RecordPattern(out ElaPattern pat) {
		pat = null; 
		var cpat = default(ElaFieldPattern);
		
		Expect(17);
		var rec = new ElaRecordPattern(t); 
		pat = rec; 
		
		if (la.kind == 1 || la.kind == 7) {
			FieldPattern(out cpat);
			rec.Fields.Add(cpat); 
			while (la.kind == 50) {
				Get();
				FieldPattern(out cpat);
				rec.Fields.Add(cpat); 
			}
		}
		Expect(18);
	}

	void IdentPattern(out ElaPattern pat) {
		pat = null; 
		Expect(1);
		var name = t.val; 
		if (la.kind == 54) {
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
		pat = eis; 			
		
		Expect(53);
		Expect(1);
		eis.TypeCode = GetType(t.val); 
		
		if (eis.TypeCode == ElaTypeCode.None)
			eis.TypeName = t.val;
		
	}

	void BindingPattern(out ElaPattern pat) {
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
		case 5: case 6: case 7: case 8: case 41: case 42: {
			LiteralPattern(out pat);
			break;
		}
		case 19: {
			ListPattern(out pat);
			break;
		}
		case 17: {
			RecordPattern(out pat);
			break;
		}
		case 53: {
			TypeCheckPattern(out pat);
			break;
		}
		default: SynErr(83); break;
		}
		if (la.kind == 30) {
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
		} else if (la.kind == 17) {
			RecordPattern(out pat);
		} else if (la.kind == 19) {
			ListPattern(out pat);
		} else if (la.kind == 48) {
			UnitPattern(out pat);
		} else SynErr(84);
	}

	void IsOperatorPattern(out ElaPattern pat) {
		pat = null; 
		if (la.kind == 53) {
			TypeCheckPattern(out pat);
		} else if (StartOf(1)) {
			LiteralPattern(out pat);
		} else if (la.kind == 17) {
			RecordPattern(out pat);
		} else if (la.kind == 19) {
			ListPattern(out pat);
		} else if (la.kind == 48) {
			UnitPattern(out pat);
		} else SynErr(85);
	}

	void FieldPattern(out ElaFieldPattern fld) {
		fld = null;
		var cpat = default(ElaPattern);
		
		if (la.kind == 7) {
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
			
		} else SynErr(86);
	}

	void RecordField(out ElaFieldDeclaration fld) {
		fld = null; 
		var cexp = default(ElaExpression);
		var mutable = false;
		
		if (la.kind == 56) {
			Get();
			mutable = true; 
		}
		if (la.kind == 1) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = t.val, Mutable = mutable }; 
			if (la.kind == 51) {
				Get();
				Expr(out cexp);
				fld.FieldValue = cexp; 
			}
			if (fld.FieldValue == null)
			fld.FieldValue = new ElaVariableReference(t) { VariableName = t.val };
			
		} else if (la.kind == 7) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = ReadString(t.val), Mutable = mutable }; 
			Expect(51);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else SynErr(87);
	}

	void RangeExpr(ElaExpression first, ElaExpression sec, out ElaRange rng) {
		rng = new ElaRange(t) { First = first, Second = sec };
		var cexp = default(ElaExpression);
		
		Expect(57);
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
		if (la.kind == 25 || la.kind == 50 || la.kind == 57) {
			if (la.kind == 25) {
				ComprehensionExpr(exp, out comp);
			} else if (la.kind == 57) {
				RangeExpr(exp, null, out rng);
			} else {
				var oexp = exp; 
				Get();
				Expr(out exp);
				if (la.kind == 57) {
					RangeExpr(oexp, exp, out rng);
				} else if (la.kind == 20 || la.kind == 50) {
					list = new List<ElaExpression>();
					list.Add(oexp);
					list.Add(exp);
					
					while (la.kind == 50) {
						Get();
						Expr(out exp);
						list.Add(exp); 
					}
				} else SynErr(88);
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
		
		Expect(25);
		ComprehensionEntry(sel, out it);
		exp = new ElaComprehension(ot) { Generator = it }; 
	}

	void LetBinding(out ElaExpression exp) {
		exp = null; 
		var inExp = default(ElaExpression);
		var flags = default(ElaVariableFlags);
		
		while (!(la.kind == 0 || la.kind == 32)) {SynErr(89); Get();}
		Expect(32);
		if (la.kind == 33 || la.kind == 59) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		ExpectWeak(27, 11);
		Expr(out inExp);
		((ElaBinding)exp).In = inExp; 
	}

	void VariableAttributes(out ElaVariableFlags flags) {
		flags = ElaVariableFlags.None; 
		if (la.kind == 33) {
			Get();
			flags |= ElaVariableFlags.Private; 
			if (la.kind == 59) {
				Get();
				flags |= ElaVariableFlags.Inline; 
			}
		} else if (la.kind == 59) {
			Get();
			flags |= ElaVariableFlags.Inline; 
			if (la.kind == 33) {
				Get();
				flags |= ElaVariableFlags.Private; 
			}
		} else SynErr(90);
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
				} else if (la.kind == 21) {
					BindingBodyGuards(varExp);
				} else {
					BindingBodyInit(varExp);
				}
			}
			SetObjectMetadata(varExp, cexp); 
		} else if (StartOf(13)) {
			BindingPattern(out pat);
			varExp.Pattern = pat; 
			if (la.kind == 51) {
				BindingBodyInit(varExp);
			}
		} else if (StartOf(14)) {
			BindingBodyOperator(varExp);
		} else SynErr(91);
		if (la.kind == 45) {
			ExpectWeak(45, 15);
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
		while (!(la.kind == 0 || la.kind == 32)) {SynErr(92); Get();}
		Expect(32);
		if (la.kind == 33 || la.kind == 59) {
			VariableAttributes(out flags);
		}
		BindingBody(flags, out exp);
		if (la.kind == 27) {
			ExpectWeak(27, 11);
			Expr(out inExp);
			((ElaBinding)exp).In = inExp; 
			EndBlock();
		} else if (la.kind == 46) {
			EndBlock();
		} else SynErr(93);
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
		
		Expect(21);
		if (StartOf(9)) {
			var newCond = new ElaCondition(t);
			cond.False = newCond;
			cond = newCond;
			
			BindingGuard(out gexp);
			cond.Condition = gexp; 
			Expect(51);
			Expr(out cexp);
			cond.True = cexp; 
			if (la.kind == 21) {
				BindingGuardList(ref cond);
			}
		} else if (la.kind == 37) {
			Get();
			Expect(51);
			Expr(out cexp);
			cond.False = cexp; 
		} else SynErr(94);
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
		
		Expect(21);
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
		if (StartOf(4)) {
			Expr(out cexp);
		} else if (la.kind == 60) {
			Get();
			Expect(1);
			cexp = new ElaBuiltin(t) { Kind = Builtins.Kind(t.val) }; 
		} else SynErr(95);
		varExp.InitExpression = cexp; 
		if (la.kind == 44) {
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
			
		} else if (la.kind == 51) {
			BindingBodyInit(varExp);
			if (varExp.InitExpression.Type == ElaNodeType.FunctionLiteral)
			((ElaFunctionLiteral)varExp.InitExpression).FunctionType = ElaFunctionType.Operator;
			
		} else SynErr(96);
	}

	void Operators() {
		switch (la.kind) {
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
		case 16: {
			Get();
			break;
		}
		case 55: {
			Get();
			break;
		}
		default: SynErr(97); break;
		}
	}

	void FunName(ElaFunctionLiteral fun) {
		if (la.kind == 1) {
			Get();
		} else if (StartOf(14)) {
			Operators();
		} else SynErr(98);
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
		if (la.kind == 21) {
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(51);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 44) {
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
		Expect(51);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
		if (la.kind == 44) {
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
		while (!(la.kind == 0 || la.kind == 23)) {SynErr(99); Get();}
		Expect(23);
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
		if (la.kind == 21) {
			var cexp = default(ElaExpression); 
			Guard(out cexp);
			entry.Guard = cexp; 
		}
		Expect(22);
		var fexp = default(ElaExpression); 
		Expr(out fexp);
		entry.Expression = fexp; 
	}

	void IncludeStat(out ElaExpression exp) {
		exp = null; 
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 34)) {SynErr(100); Get();}
		Expect(34);
		var inc = new ElaModuleInclude(t); 
		Qualident(inc.Path);
		var name = inc.Path[inc.Path.Count - 1];				
		inc.Path.RemoveAt(inc.Path.Count - 1);				
		inc.Alias = inc.Name = name;
		exp = inc;
		
		if (la.kind == 61) {
			Get();
			if (la.kind == 1) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 7) {
				Get();
				inc.DllName = ReadString(t.val); 
			} else SynErr(101);
		}
		if (la.kind == 30) {
			Get();
			Expect(1);
			inc.Alias = t.val; 
		}
		Expect(46);
	}

	void Qualident(List<String> path ) {
		var val = String.Empty; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 7) {
			Get();
			val = ReadString(t.val); 
		} else SynErr(102);
		path.Add(val); 
		if (la.kind == 26) {
			Get();
			Qualident(path);
		}
	}

	void IfExpr(out ElaExpression exp) {
		Expect(36);
		var cond = new ElaCondition(t); 
		var cexp = default(ElaExpression);	
		exp = cond;
		
		Expr(out cexp);
		cond.Condition = cexp; 
		Expect(38);
		Expr(out cexp);
		cond.True = cexp; 
		ExpectWeak(37, 11);
		Expr(out cexp);
		cond.False = cexp; 
	}

	void RaiseExpr(out ElaExpression exp) {
		Expect(39);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		var code = String.Empty;
		
		Expect(1);
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
		Expect(43);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		
		Expr(out cexp);
		r.Expression = cexp; 
		r.ErrorCode = "Failure"; 
		
	}

	void TryExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		Expect(40);
		var ot = t;
		var match = new ElaTry(t);
		exp = match; 
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(35);
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
		while (la.kind == 62 || la.kind == 63) {
			var cexp = default(ElaExpression); 
			var op = default(ElaOperator);
			
			if (la.kind == 62) {
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
		while (la.kind == 64) {
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
		while (la.kind == 65) {
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
		while (la.kind == 66) {
			var cexp = default(ElaExpression); 
			Get();
			AndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanOr, Right = cexp }; 
			
		}
	}

	void AndExpr(out ElaExpression exp) {
		OpExpr1(out exp);
		while (la.kind == 67) {
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
			while (la.kind == 9) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(18)) {
					OpExpr2(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 9) {
			Get();
			op = t.val; 
			if (StartOf(18)) {
				OpExpr2(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = op };
			
		} else SynErr(103);
	}

	void OpExpr2(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(19)) {
			OpExpr3(out exp);
			while (la.kind == 10) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(19)) {
					OpExpr3(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 10) {
			Get();
			op = t.val; 
			if (StartOf(19)) {
				OpExpr3(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(104);
	}

	void OpExpr3(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(20)) {
			OpExpr4(out exp);
			while (la.kind == 11 || la.kind == 55) {
				var cexp = default(ElaExpression); 
				if (la.kind == 11) {
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
		} else if (la.kind == 11 || la.kind == 55) {
			if (la.kind == 11) {
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
			
		} else SynErr(105);
	}

	void OpExpr4(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(21)) {
			OpExpr5(out exp);
			while (la.kind == 12) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(21)) {
					OpExpr5(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 12) {
			Get();
			op = t.val; 
			if (StartOf(21)) {
				OpExpr5(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(106);
	}

	void OpExpr5(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(22)) {
			CastExpr(out exp);
			while (la.kind == 13) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(22)) {
					CastExpr(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 13) {
			Get();
			op = t.val; 
			if (StartOf(22)) {
				CastExpr(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(107);
	}

	void CastExpr(out ElaExpression exp) {
		InfixExpr(out exp);
		while (la.kind == 24 || la.kind == 31 || la.kind == 54) {
			if (la.kind == 54) {
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
		exp = null;
		var ot = t; 
		var name = String.Empty;
		
		if (StartOf(23)) {
			OpExpr6(out exp);
			while (la.kind == 4) {
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
		} else if (la.kind == 4) {
			Get();
			name = t.val.Trim('`'); 
			if (StartOf(23)) {
				OpExpr6(out exp);
				exp = GetPrefixFun(name, exp, true);	
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = name };
			
		} else SynErr(108);
	}

	void ComprehensionOpExpr(ElaExpression init, out ElaExpression exp) {
		var list = default(List<ElaExpression>);
		var comp = default(ElaComprehension);
		var rng = default(ElaRange);
		exp = null;
		
		Expect(19);
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
		
		Expect(20);
	}

	void OpExpr6(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(24)) {
			OpExpr7(out exp);
			while (la.kind == 14) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(23)) {
					OpExpr6(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 14) {
			Get();
			op = t.val; 
			if (StartOf(24)) {
				OpExpr7(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(109);
	}

	void OpExpr7(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(25)) {
			OpExpr8(out exp);
			while (la.kind == 15) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(25)) {
					OpExpr8(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 15) {
			Get();
			op = t.val; 
			if (StartOf(25)) {
				OpExpr8(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(110);
	}

	void OpExpr8(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(26)) {
			Application(out exp);
			while (la.kind == 16) {
				Get();
				op = t.val; 
				exp = GetOperatorFun(op, exp, null); 
			}
		} else if (la.kind == 16) {
			Get();
			op = t.val; 
			if (StartOf(26)) {
				Application(out exp);
				exp = GetOperatorFun(op, exp, null); 
			}
			if (exp == null)
			exp = new ElaVariableReference(ot) { VariableName = t.val };
			
		} else SynErr(111);
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
		} else SynErr(112);
	}

	void AccessExpr(out ElaExpression exp) {
		Literal(out exp);
		while (la.kind == 26) {
			Get();
			if (la.kind == 19) {
				Get();
				var indExp = new ElaIndexer(t) { TargetObject = exp };
				exp = indExp;
				
				var cexp = default(ElaExpression); 
				Expr(out cexp);
				indExp.Index = cexp;	
				Expect(20);
			} else if (la.kind == 1) {
				Get();
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
			} else SynErr(113);
		}
	}

	void EmbExpr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 1: case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13: case 14: case 15: case 16: case 17: case 19: case 28: case 41: case 42: case 47: case 48: case 55: case 69: {
			BinaryExpr(out exp);
			break;
		}
		case 36: {
			IfExpr(out exp);
			break;
		}
		case 23: {
			LambdaExpr(out exp);
			break;
		}
		case 39: {
			RaiseExpr(out exp);
			break;
		}
		case 43: {
			FailExpr(out exp);
			break;
		}
		case 29: {
			MatchExpr(out exp);
			break;
		}
		case 40: {
			TryExpr(out exp);
			break;
		}
		default: SynErr(114); break;
		}
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
		
		if (la.kind == 21 || la.kind == 50) {
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
		while (StartOf(27)) {
			DeclarationBlock(b);
		}
	}

	void DeclarationBlock(ElaBlock b) {
		var exp = default(ElaExpression); 
		if (la.kind == 32) {
			RootLetBinding(out exp);
		} else if (la.kind == 34) {
			IncludeStat(out exp);
		} else if (StartOf(6)) {
			EmbExpr(out exp);
			if (FALSE) 
			Expect(46);
		} else SynErr(115);
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
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,T,x,x, T,x,T,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,T, x,x,x,T, x,x,x,x, T,T,x,x, T,x,x,x, T,x,x,T, T,T,T,T, x,x,x,T, T,x,x,x, x,x,x,T, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,T, x,x,x,T, x,x,x,x, T,T,x,x, T,x,x,x, T,x,x,T, T,T,T,T, x,x,x,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,x,T, x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,T, x,x,x,T, x,x,x,x, T,T,x,x, x,x,x,x, T,x,x,T, T,T,T,T, x,x,x,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,x, x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,T, x,x,x,T, x,x,x,x, T,T,x,x, T,x,T,x, T,x,x,T, T,T,T,T, T,x,x,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{T,T,x,x, x,T,T,T, T,T,T,T, T,T,T,T, T,T,x,T, x,x,x,T, x,x,x,x, x,T,x,x, T,x,T,x, x,x,x,x, x,T,T,x, T,x,x,T, T,x,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,x,x, x,T,T,T, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,T,T,T, T,x,T,T, T,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, T,T,T,T, T,x,x,T, T,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, T,T,T,T, T,x,x,x, T,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, T,T,T,T, T,x,x,x, x,x,T,T, T,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, x,T,T,T, T,x,x,x, x,x,T,T, T,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, x,T,T,T, T,x,x,x, x,x,x,T, T,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, x,T,T,T, T,x,x,x, x,x,x,x, T,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, x,T,T,T, T,x,x,x, x,x,x,x, x,T,x,T, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,T, x,x,x,T, x,x,x,x, T,T,x,x, T,x,T,x, T,x,x,T, T,T,T,T, x,x,x,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x}

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
			case 9: s = "operatorTok1 expected"; break;
			case 10: s = "operatorTok2 expected"; break;
			case 11: s = "operatorTok3 expected"; break;
			case 12: s = "operatorTok4 expected"; break;
			case 13: s = "operatorTok5 expected"; break;
			case 14: s = "operatorTok6 expected"; break;
			case 15: s = "operatorTok7 expected"; break;
			case 16: s = "operatorTok8 expected"; break;
			case 17: s = "LBRA expected"; break;
			case 18: s = "RBRA expected"; break;
			case 19: s = "LILB expected"; break;
			case 20: s = "LIRB expected"; break;
			case 21: s = "PIPE expected"; break;
			case 22: s = "ARROW expected"; break;
			case 23: s = "LAMBDA expected"; break;
			case 24: s = "COMPH expected"; break;
			case 25: s = "COMPO expected"; break;
			case 26: s = "DOT expected"; break;
			case 27: s = "IN expected"; break;
			case 28: s = "BASE expected"; break;
			case 29: s = "MATCH expected"; break;
			case 30: s = "ASAMP expected"; break;
			case 31: s = "IS expected"; break;
			case 32: s = "LET expected"; break;
			case 33: s = "PRIVATE expected"; break;
			case 34: s = "OPEN expected"; break;
			case 35: s = "WITH expected"; break;
			case 36: s = "IFS expected"; break;
			case 37: s = "ELSE expected"; break;
			case 38: s = "THEN expected"; break;
			case 39: s = "RAISE expected"; break;
			case 40: s = "TRY expected"; break;
			case 41: s = "TRUE expected"; break;
			case 42: s = "FALSE expected"; break;
			case 43: s = "FAIL expected"; break;
			case 44: s = "WHERE expected"; break;
			case 45: s = "ET expected"; break;
			case 46: s = "EBLOCK expected"; break;
			case 47: s = "\"_\" expected"; break;
			case 48: s = "\"(\" expected"; break;
			case 49: s = "\")\" expected"; break;
			case 50: s = "\",\" expected"; break;
			case 51: s = "\"=\" expected"; break;
			case 52: s = "\"`\" expected"; break;
			case 53: s = "\"?\" expected"; break;
			case 54: s = "\":\" expected"; break;
			case 55: s = "\"::\" expected"; break;
			case 56: s = "\"!\" expected"; break;
			case 57: s = "\"..\" expected"; break;
			case 58: s = "\"&\" expected"; break;
			case 59: s = "\"inline\" expected"; break;
			case 60: s = "\"__internal\" expected"; break;
			case 61: s = "\"#\" expected"; break;
			case 62: s = "\"<-\" expected"; break;
			case 63: s = "\"<->\" expected"; break;
			case 64: s = "\"<|\" expected"; break;
			case 65: s = "\"|>\" expected"; break;
			case 66: s = "\"||\" expected"; break;
			case 67: s = "\"&&\" expected"; break;
			case 68: s = "\"$\" expected"; break;
			case 69: s = "\"(&\" expected"; break;
			case 70: s = "??? expected"; break;
			case 71: s = "invalid Literal"; break;
			case 72: s = "invalid Primitive"; break;
			case 73: s = "invalid SimpleExpr"; break;
			case 74: s = "invalid VariableReference"; break;
			case 75: s = "invalid Expr"; break;
			case 76: s = "this symbol not expected in MatchExpr"; break;
			case 77: s = "invalid Guard"; break;
			case 78: s = "this symbol not expected in WhereBinding"; break;
			case 79: s = "invalid OrPattern"; break;
			case 80: s = "invalid VariantPattern"; break;
			case 81: s = "invalid SinglePattern"; break;
			case 82: s = "invalid LiteralPattern"; break;
			case 83: s = "invalid BindingPattern"; break;
			case 84: s = "invalid GeneratorPattern"; break;
			case 85: s = "invalid IsOperatorPattern"; break;
			case 86: s = "invalid FieldPattern"; break;
			case 87: s = "invalid RecordField"; break;
			case 88: s = "invalid ParamList"; break;
			case 89: s = "this symbol not expected in LetBinding"; break;
			case 90: s = "invalid VariableAttributes"; break;
			case 91: s = "invalid BindingBody"; break;
			case 92: s = "this symbol not expected in RootLetBinding"; break;
			case 93: s = "invalid RootLetBinding"; break;
			case 94: s = "invalid BindingGuardList"; break;
			case 95: s = "invalid BindingBodyInit"; break;
			case 96: s = "invalid BindingBodyOperator"; break;
			case 97: s = "invalid Operators"; break;
			case 98: s = "invalid FunName"; break;
			case 99: s = "this symbol not expected in LambdaExpr"; break;
			case 100: s = "this symbol not expected in IncludeStat"; break;
			case 101: s = "invalid IncludeStat"; break;
			case 102: s = "invalid Qualident"; break;
			case 103: s = "invalid OpExpr1"; break;
			case 104: s = "invalid OpExpr2"; break;
			case 105: s = "invalid OpExpr3"; break;
			case 106: s = "invalid OpExpr4"; break;
			case 107: s = "invalid OpExpr5"; break;
			case 108: s = "invalid InfixExpr"; break;
			case 109: s = "invalid OpExpr6"; break;
			case 110: s = "invalid OpExpr7"; break;
			case 111: s = "invalid OpExpr8"; break;
			case 112: s = "invalid Application"; break;
			case 113: s = "invalid AccessExpr"; break;
			case 114: s = "invalid EmbExpr"; break;
			case 115: s = "invalid DeclarationBlock"; break;

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

