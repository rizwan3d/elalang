
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
	public const int _operatorTok0 = 6;
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
	public const int _INSTANCE = 41;
	public const int _TYPE = 42;
	public const int _CLASS = 43;
	public const int _IMPORT = 44;
	public const int _DATA = 45;
	public const int _EBLOCK = 46;
	public const int maxT = 63;

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
		Expect(46);
		if (!t.virt) scanner.PopIndent(); 
	}

	void Literal(out ElaExpression exp) {
		exp = null; 
		if (StartOf(1)) {
			Primitive(out exp);
		} else if (la.kind == 16) {
			RecordLiteral(out exp);
		} else if (la.kind == 18 || la.kind == 54) {
			ListLiteral(out exp);
		} else if (la.kind == 49) {
			TupleLiteral(out exp);
		} else if (la.kind == 1 || la.kind == 48) {
			SimpleExpr(out exp);
		} else SynErr(64);
		if (la.kind == 27) {
			As(ref exp);
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
		default: SynErr(65); break;
		}
	}

	void RecordLiteral(out ElaExpression exp) {
		exp = null; 
		var fld = default(ElaFieldDeclaration);
		
		Expect(16);
		var rec = new ElaRecordLiteral(t); exp = rec; 
		RecordField(out fld);
		rec.Fields.Add(fld); 
		while (la.kind == 51) {
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
			
			if (StartOf(2)) {
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
		} else if (la.kind == 54) {
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
		} else SynErr(66);
	}

	void TupleLiteral(out ElaExpression exp) {
		var ot = default(Token);
		exp = null; 
		
		Expect(49);
		ot = t; 
		if (StartOf(2)) {
			GroupExpr(out exp);
		}
		if (exp != null)
		   exp.Parens = true;
		
		Expect(50);
		if (exp == null)
		exp = new ElaUnitLiteral(ot);
		
	}

	void SimpleExpr(out ElaExpression exp) {
		VariableReference(out exp);
	}

	void As(ref ElaExpression exp) {
		Expect(27);
		var @as = new ElaAs(t) { Expression = exp }; 
		exp = @as; 
		
		if (la.kind == 1) {
			Get();
			@as.Name = t.val; 
		} else if (la.kind == 49) {
			Get();
			Operators();
			@as.Name = t.val; 
			Expect(50);
		} else SynErr(67);
	}

	void Attribute(ref ElaExpression head) {
		scanner.InjectBlock(); 
		var flags = ElaVariableFlags.None;
		
		if (head.Type != ElaNodeType.NameReference)
		    AddError(ElaParserError.InvalidAttribute, head.ToString());
		
		Expect(47);
		Expect(1);
		flags = ProcessAttribute(t.val,flags); 
		while (la.kind == 1) {
			Get();
			flags = ProcessAttribute(t.val,flags); 
		}
		EndBlock();
		var h = new ElaHeader { Name = head.GetName(), Attributes = flags };
		h.SetLinePragma(head.Line, head.Column);
		head = h;
		
	}

	void VariableReference(out ElaExpression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = new ElaNameReference(t) { Name = t.val }; 
		} else if (la.kind == 48) {
			Get();
			exp = new ElaPlaceholder(t); 
		} else SynErr(68);
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
			if (StartOf(2)) {
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
		EmbExpr(out exp);
	}

	void RecordField(out ElaFieldDeclaration fld) {
		fld = null; 
		var cexp = default(ElaExpression);
		
		if (la.kind == 1) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = t.val }; 
			if (la.kind == 52) {
				Get();
				Expr(out cexp);
				fld.FieldValue = cexp; 
			}
			if (fld.FieldValue == null)
			fld.FieldValue = new ElaNameReference(t) { Name = t.val };
			
		} else if (la.kind == 4) {
			Get();
			fld = new ElaFieldDeclaration(t) { FieldName = ReadString(t.val) }; 
			Expect(52);
			Expr(out cexp);
			fld.FieldValue = cexp; 
		} else SynErr(69);
	}

	void RangeExpr(ElaExpression first, ElaExpression sec, out ElaRange rng) {
		rng = new ElaRange(t) { First = first, Second = sec };
		var cexp = default(ElaExpression);
		
		Expect(53);
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
		if (la.kind == 23 || la.kind == 51 || la.kind == 53) {
			if (la.kind == 23) {
				ComprehensionExpr(exp, out comp);
			} else if (la.kind == 53) {
				RangeExpr(exp, null, out rng);
			} else {
				var oexp = exp; 
				Get();
				Expr(out exp);
				if (la.kind == 53) {
					RangeExpr(oexp, exp, out rng);
				} else if (la.kind == 19 || la.kind == 51) {
					list = new List<ElaExpression>();
					list.Add(oexp);
					list.Add(exp);
					
					while (la.kind == 51) {
						Get();
						Expr(out exp);
						list.Add(exp); 
					}
				} else SynErr(70);
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

	void Operators() {
		switch (la.kind) {
		case 6: {
			Get();
			break;
		}
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
		case 55: {
			Get();
			break;
		}
		default: SynErr(71); break;
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
		ExpectWeak(33, 3);
		Expr(out cexp);
		cond.False = cexp; 
	}

	void MatchExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		bindings.Push(unit);
		
		while (!(la.kind == 0 || la.kind == 26)) {SynErr(72); Get();}
		Expect(26);
		var match = new ElaMatch(t);
		exp = match; 
		var block = default(ElaEquationSet);
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(31);
		BindingChain(out block);
		match.Entries = block; 
		bindings.Pop();
		
		EndBlock();
	}

	void BindingChain(out ElaEquationSet block) {
		block = new ElaEquationSet(); 
		scanner.InjectBlock();
		
		Binding(block,null);
		if (RequireEndBlock()) 
		EndBlock();
		while (StartOf(2)) {
			scanner.InjectBlock(); 
			Binding(block,null);
			if (RequireEndBlock()) 
			EndBlock();
		}
	}

	void TryExpr(out ElaExpression exp) {
		scanner.InjectBlock(); 
		bindings.Push(unit);
		
		while (!(la.kind == 0 || la.kind == 36)) {SynErr(73); Get();}
		Expect(36);
		var match = new ElaTry(t);
		exp = match; 
		var block = default(ElaEquationSet);
		var cexp = default(ElaExpression);
		
		Expr(out cexp);
		match.Expression = cexp; 
		Expect(31);
		BindingChain(out block);
		match.Entries = block; 
		bindings.Pop();
		
		EndBlock();
	}

	void OrExpr(out ElaExpression exp) {
		AndExpr(out exp);
		while (la.kind == 56) {
			var cexp = default(ElaExpression); 
			Get();
			AndExpr(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanOr, Right = cexp }; 
			
		}
	}

	void AndExpr(out ElaExpression exp) {
		OpExpr0(out exp);
		while (la.kind == 57) {
			var cexp = default(ElaExpression); 
			Get();
			OpExpr0(out cexp);
			exp = new ElaBinary(t) { 
			Left = exp, Operator = ElaOperator.BooleanAnd, Right = cexp }; 
			
		}
	}

	void OpExpr0(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(4)) {
			OpExpr1b(out exp);
			while (la.kind == 6) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(4)) {
					OpExpr1b(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 6) {
			Get();
			op = t.val; 
			if (StartOf(4)) {
				OpExpr1b(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = op };
			
		} else SynErr(74);
	}

	void OpExpr1b(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(5)) {
			OpExpr1(out exp);
			while (la.kind == 15) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(4)) {
					OpExpr1b(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 15) {
			Get();
			op = t.val; 
			if (StartOf(5)) {
				OpExpr1(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = t.val };
			
		} else SynErr(75);
	}

	void OpExpr1(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(6)) {
			OpExpr2(out exp);
			while (la.kind == 7) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(6)) {
					OpExpr2(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 7) {
			Get();
			op = t.val; 
			if (StartOf(6)) {
				OpExpr2(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = op };
			
		} else SynErr(76);
	}

	void OpExpr2(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(7)) {
			OpExpr3(out exp);
			while (la.kind == 8) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(7)) {
					OpExpr3(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 8) {
			Get();
			op = t.val; 
			if (StartOf(7)) {
				OpExpr3(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = t.val };
			
		} else SynErr(77);
	}

	void OpExpr3(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(8)) {
			OpExpr4(out exp);
			while (la.kind == 9 || la.kind == 55) {
				var cexp = default(ElaExpression); 
				if (la.kind == 9) {
					Get();
				} else {
					Get();
				}
				op = t.val; 
				if (StartOf(7)) {
					OpExpr3(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 9 || la.kind == 55) {
			if (la.kind == 9) {
				Get();
			} else {
				Get();
			}
			op = t.val; 
			if (StartOf(8)) {
				OpExpr4(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = t.val };
			
		} else SynErr(78);
	}

	void OpExpr4(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(9)) {
			OpExpr5(out exp);
			while (la.kind == 10) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(9)) {
					OpExpr5(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 10) {
			Get();
			op = t.val; 
			if (StartOf(9)) {
				OpExpr5(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = t.val };
			
		} else SynErr(79);
	}

	void OpExpr5(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(10)) {
			InfixExpr(out exp);
			while (la.kind == 11) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(10)) {
					InfixExpr(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 11) {
			Get();
			op = t.val; 
			if (StartOf(10)) {
				InfixExpr(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = t.val };
			
		} else SynErr(80);
	}

	void InfixExpr(out ElaExpression exp) {
		exp = null;
		var ot = t; 
		var funexp = default(ElaExpression);
		
		if (StartOf(11)) {
			OpExpr6(out exp);
			while (la.kind == 58) {
				var cexp = default(ElaExpression); 
				ot = t;
				
				Get();
				Literal(out funexp);
				Expect(58);
				if (StartOf(11)) {
					OpExpr6(out cexp);
				}
				var fc = new ElaJuxtaposition(ot) { 
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
			if (StartOf(11)) {
				OpExpr6(out exp);
				exp = GetPrefixFun(funexp, exp, true);	
			}
			if (exp == null)
			exp = funexp;
			
		} else SynErr(81);
	}

	void OpExpr6(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(12)) {
			OpExpr7(out exp);
			while (la.kind == 12) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(11)) {
					OpExpr6(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 12) {
			Get();
			op = t.val; 
			if (StartOf(12)) {
				OpExpr7(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = t.val };
			
		} else SynErr(82);
	}

	void OpExpr7(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(13)) {
			OpExpr8(out exp);
			while (la.kind == 13) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(13)) {
					OpExpr8(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 13) {
			Get();
			op = t.val; 
			if (StartOf(13)) {
				OpExpr8(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = t.val };
			
		} else SynErr(83);
	}

	void OpExpr8(out ElaExpression exp) {
		var op = String.Empty; 
		exp = null;
		var ot = t; 
		
		if (StartOf(14)) {
			Application(out exp);
			while (la.kind == 14) {
				var cexp = default(ElaExpression); 
				Get();
				op = t.val; 
				if (StartOf(14)) {
					Application(out cexp);
				}
				exp = GetOperatorFun(op, exp, cexp); 
			}
		} else if (la.kind == 14) {
			Get();
			op = t.val; 
			if (StartOf(14)) {
				Application(out exp);
				exp = GetOperatorFun(op, null, exp); 
			}
			if (exp == null)
			exp = new ElaNameReference(ot) { Name = t.val };
			
		} else SynErr(84);
	}

	void Application(out ElaExpression exp) {
		exp = null; 
		AccessExpr(out exp);
		var ot = t;
		var mi = default(ElaJuxtaposition); 
		var cexp = default(ElaExpression);
		
		while (StartOf(14)) {
			AccessExpr(out cexp);
			if (mi == null)
			{
			    mi = new ElaJuxtaposition(ot) { Target = exp };
				exp = mi;
			}
			
			    if (mi != null)
				    mi.Parameters.Add(cexp); 
			
		}
	}

	void AccessExpr(out ElaExpression exp) {
		Literal(out exp);
		while (la.kind == 24) {
			Get();
			if (la.kind == 49) {
				Get();
				if (la.kind == 1) {
					Get();
				} else if (StartOf(15)) {
					Operators();
				} else SynErr(85);
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
				Expect(50);
			} else if (la.kind == 1) {
				Get();
				exp = new ElaFieldReference(t) { FieldName = t.val, TargetObject = exp }; 
			} else SynErr(86);
		}
	}

	void BinaryExpr(out ElaExpression exp) {
		exp = null;
		var ot = t; 
		
		OrExpr(out exp);
		while (la.kind == 59) {
			var cexp = default(ElaExpression); 
			Get();
			OrExpr(out cexp);
			exp = new ElaBinary(t) { Operator = ElaOperator.Sequence, Left = exp, Right = cexp };
			
		}
	}

	void EmbExpr(out ElaExpression exp) {
		exp = null; 
		switch (la.kind) {
		case 1: case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11: case 12: case 13: case 14: case 15: case 16: case 18: case 37: case 38: case 48: case 49: case 54: case 55: case 58: {
			BinaryExpr(out exp);
			if (la.kind == 28) {
				TypeCheckExpr(ref exp);
			}
			break;
		}
		case 32: {
			IfExpr(out exp);
			break;
		}
		case 61: {
			LazyExpr(out exp);
			break;
		}
		case 29: {
			LetBinding(out exp);
			break;
		}
		case 60: {
			Builtin(out exp);
			break;
		}
		case 22: {
			LambdaExpr(out exp);
			break;
		}
		case 39: {
			FailExpr(out exp);
			break;
		}
		case 26: {
			MatchExpr(out exp);
			break;
		}
		case 36: {
			TryExpr(out exp);
			break;
		}
		default: SynErr(87); break;
		}
	}

	void TypeCheckExpr(ref ElaExpression pat) {
		Expect(28);
		var eis = new ElaTypeCheck(t) { Expression = pat };
		pat = eis; 			
		var fst = default(String); 
		var snd = default(String);
		
		while (la.kind == 1) {
			Get();
			fst = t.val; snd = null; 
			if (la.kind == 24) {
				Get();
				Expect(1);
				snd = t.val; 
			}
			var ti = new TraitInfo(snd != null ? fst : null, snd != null ? snd : fst);
			eis.Traits.Add(ti);
			
		}
	}

	void LazyExpr(out ElaExpression exp) {
		Expect(61);
		var lazy = new ElaLazyLiteral(t); 
		Expr(out exp);
		lazy.Expression = exp;
		exp = lazy;
		
	}

	void LetBinding(out ElaExpression exp) {
		exp = null; 
		while (!(la.kind == 0 || la.kind == 29)) {SynErr(88); Get();}
		Expect(29);
		var bid = new ElaLetBinding(t);
		var block = default(ElaEquationSet);
		var cexp = default(ElaExpression);
		exp = bid;
		
		BindingChain(out block);
		Expect(25);
		Expr(out cexp);
		bid.Equations = block;
		bid.Expression = cexp;
		
	}

	void Builtin(out ElaExpression exp) {
		Expect(60);
		Expect(1);
		exp = new ElaBuiltin(t) { Kind = Builtins.Kind(t.val) }; 
	}

	void LambdaExpr(out ElaExpression exp) {
		while (!(la.kind == 0 || la.kind == 22)) {SynErr(89); Get();}
		Expect(22);
		var lambda = new ElaLambda(t);
		exp = lambda;
		var left = default(ElaExpression);
		var right = default(ElaExpression);
		
		Expr(out left);
		if (la.kind == 21) {
			Get();
			Expr(out right);
		} else if (la.kind == 20) {
			Get();
			LambdaGuard(out right);
		} else SynErr(90);
		lambda.Left = left;
		lambda.Right = right;
		
	}

	void FailExpr(out ElaExpression exp) {
		Expect(39);
		var r = new ElaRaise(t);
		exp = r;
		var cexp = default(ElaExpression); 
		
		Expr(out cexp);
		r.Expression = cexp; 
		
	}

	void ComprehensionEntry(ElaExpression body, out ElaGenerator it) {
		it = new ElaGenerator(t);
		it.Body = body;
		var cexp = default(ElaExpression);
		var pat = default(ElaExpression);
		
		Expr(out pat);
		Expect(62);
		Expr(out cexp);
		it.Pattern = pat;
		it.Target = cexp;
		
		if (la.kind == 20 || la.kind == 51) {
			if (la.kind == 51) {
				var cit = default(ElaGenerator); 
				Get();
				ComprehensionEntry(body, out cit);
				it.Body = cit; 
			} else {
				Get();
				Expr(out cexp);
				it.Guard = cexp; 
				if (la.kind == 51) {
					var cit = default(ElaGenerator); 
					Get();
					ComprehensionEntry(body, out cit);
					it.Body = cit; 
				}
			}
		}
	}

	void LambdaGuard(out ElaExpression exp) {
		var ifExp = new ElaCondition(t);
		exp = ifExp;
		
		var cond = default(ElaExpression);
		var trueExp = default(ElaExpression);
		var falseExp = default(ElaExpression);
		
		BinaryExpr(out cond);
		ifExp.Condition = cond; 
		Expect(21);
		Expr(out trueExp);
		ifExp.True = trueExp; 
		Expect(20);
		if (la.kind == 33) {
			Get();
			Expect(21);
			Expr(out falseExp);
		} else if (StartOf(16)) {
			LambdaGuard(out falseExp);
		} else SynErr(91);
		ifExp.False = falseExp; 
	}

	void WhereBinding(out ElaEquationSet block) {
		scanner.InjectBlock(); 
		bindings.Push(null);
		var skip = false;
		
		while (!(la.kind == 0 || la.kind == 40)) {SynErr(92); Get();}
		Expect(40);
		if (la.kind == 46) {
			EndBlock();
			skip=true; 
		}
		BindingChain(out block);
		bindings.Pop(); 
		if (!skip) 
		EndBlock();
	}

	void Binding(ElaEquationSet block, string type) {
		var bid = default(ElaEquation);
		var left = default(ElaExpression);
		var right = default(ElaExpression);
		
		Expr(out left);
		bid = new ElaEquation(t); 
		if (la.kind == 20 || la.kind == 47 || la.kind == 52) {
			if (la.kind == 52) {
				Get();
				Expr(out right);
			} else if (la.kind == 20) {
				Get();
				Guard(out right);
			} else {
				Attribute(ref left);
			}
		}
		if (la.kind == 40) {
			var cb = default(ElaEquationSet); 
			WhereBinding(out cb);
			if (left != null && left.Type == ElaNodeType.Header)
			   AddError(ElaParserError.InvalidAttributeWhere);
			
			    var letb = new ElaLetBinding();                    
			       if (cb != null) letb.SetLinePragma(cb.Line, cb.Column);                    
			       letb.Equations = cb;
			        
			    if (right != null)
			    {
			        letb.Expression = right;
			           right = letb;
			    }
			    else
			    {
			        letb.Expression = left;
			        left = letb;
			    }	                    
			
		}
		ProcessBinding(block, bid, left, right, type); 
	}

	void Guard(out ElaExpression exp) {
		var ifExp = new ElaCondition(t);
		exp = ifExp;
		
		var cond = default(ElaExpression);
		var trueExp = default(ElaExpression);
		var falseExp = default(ElaExpression);
		
		BinaryExpr(out cond);
		ifExp.Condition = cond; 
		Expect(52);
		Expr(out trueExp);
		ifExp.True = trueExp; 
		Expect(20);
		if (la.kind == 33) {
			Get();
			Expect(52);
			Expr(out falseExp);
		} else if (StartOf(16)) {
			Guard(out falseExp);
		} else SynErr(93);
		ifExp.False = falseExp; 
	}

	void IncludeStat() {
		var qual = false; 
		scanner.InjectBlock(); 
		while (!(la.kind == 0 || la.kind == 30 || la.kind == 44)) {SynErr(94); Get();}
		if (la.kind == 30) {
			Get();
		} else if (la.kind == 44) {
			Get();
			qual=true; 
		} else SynErr(95);
		IncludeStatElement(qual);
		EndBlock();
	}

	void IncludeStatElement(bool qual) {
		var inc = new ElaModuleInclude(t) { RequireQualified=qual }; 
		Qualident(inc.Path);
		var name = inc.Path[inc.Path.Count - 1];				
		inc.Path.RemoveAt(inc.Path.Count - 1);				
		inc.Alias = inc.Name = name;
		
		if (Program.Includes == null)
		    Program.Includes = inc;
		else
		{
		    inc.And = Program.Includes;
		    Program.Includes = inc;
		}
		
		if (la.kind == 47) {
			Get();
			if (la.kind == 1) {
				Get();
				inc.DllName = t.val; 
			} else if (la.kind == 4) {
				Get();
				inc.DllName = ReadString(t.val); 
			} else SynErr(96);
		}
		if (la.kind == 27) {
			Get();
			Expect(1);
			inc.Alias = t.val; 
		}
		if (la.kind == 49) {
			var imp = default(ElaImportedVariable); 
			Get();
			ImportName(out imp);
			inc.ImportList.Add(imp); 
			while (la.kind == 51) {
				Get();
				ImportName(out imp);
				inc.ImportList.Add(imp); 
			}
			Expect(50);
		}
		if (la.kind == 1 || la.kind == 4) {
			IncludeStatElement(qual);
		}
	}

	void Qualident(List<String> path ) {
		var val = String.Empty; 
		if (la.kind == 1) {
			Get();
			val = t.val; 
		} else if (la.kind == 4) {
			Get();
			val = ReadString(t.val); 
		} else SynErr(97);
		path.Add(val); 
		if (la.kind == 24) {
			Get();
			Qualident(path);
		}
	}

	void ImportName(out ElaImportedVariable imp) {
		imp = new ElaImportedVariable(t); 
		
		Expect(1);
		imp.Name = imp.LocalName = t.val; 
		if (la.kind == 1) {
			Get();
			if (imp.Name != "private")
			   AddError(ElaParserError.UnknownAttribute, imp.Name);
			
			imp.Private = true;
			imp.Name = imp.LocalName = t.val; 
			
		}
		if (la.kind == 52) {
			Get();
			Expect(1);
			imp.Name = t.val; 
		}
	}

	void TypeClass() {
		var tc = default(ElaTypeClass);
		scanner.InjectBlock();
		
		Expect(43);
		tc = new ElaTypeClass(t); 	            
		tc.And = Program.Classes;
		Program.Classes = tc;
		
		Expect(1);
		var nm = default(String);
		tc.Name = t.val;
		
		if (la.kind == 60) {
			Get();
			Expect(1);
			tc.BuiltinName = t.val; 
			while (StartOf(17)) {
				if (la.kind == 1) {
					Get();
					nm=t.val; 
				} else if (StartOf(15)) {
					Operators();
					nm=t.val; 
				} else {
					Get();
					if (StartOf(15)) {
						Operators();
					} else if (la.kind == 1) {
						Get();
					} else SynErr(98);
					nm=t.val; 
					Expect(50);
				}
				tc.Members.Add(new ElaClassMember(t) { Name = nm });
				
			}
		} else if (la.kind == 1) {
			var targ = String.Empty; 
			Get();
			targ = t.val; 
			Expect(40);
			if (la.kind == 1) {
				Get();
				nm=t.val; 
			} else if (StartOf(15)) {
				Operators();
				nm=t.val; 
			} else if (la.kind == 49) {
				Get();
				if (StartOf(15)) {
					Operators();
				} else if (la.kind == 1) {
					Get();
				} else SynErr(99);
				nm=t.val; 
				Expect(50);
			} else SynErr(100);
			var count = 0;
			var mask = 0;
			
			if (la.kind == 1) {
				Get();
			} else if (la.kind == 48) {
				Get();
			} else SynErr(101);
			BuildMask(ref count, ref mask, t.val, targ); 
			while (la.kind == 21) {
				Get();
				if (la.kind == 1) {
					Get();
				} else if (la.kind == 48) {
					Get();
				} else SynErr(102);
				BuildMask(ref count, ref mask, t.val, targ); 
			}
			tc.Members.Add(new ElaClassMember(t) { Name = nm, Arguments = count, Mask = mask });
			
			while (StartOf(17)) {
				if (la.kind == 1) {
					Get();
					nm=t.val; 
				} else if (StartOf(15)) {
					Operators();
					nm=t.val; 
				} else {
					Get();
					if (StartOf(15)) {
						Operators();
					} else if (la.kind == 1) {
						Get();
					} else SynErr(103);
					nm=t.val; 
					Expect(50);
				}
				count = 0;
				mask = 0;
				
				if (la.kind == 1) {
					Get();
				} else if (la.kind == 48) {
					Get();
				} else SynErr(104);
				BuildMask(ref count, ref mask, t.val, targ); 
				while (la.kind == 21) {
					Get();
					if (la.kind == 1) {
						Get();
					} else if (la.kind == 48) {
						Get();
					} else SynErr(105);
					BuildMask(ref count, ref mask, t.val, targ); 
				}
				tc.Members.Add(new ElaClassMember(t) { Name = nm, Arguments = count, Mask = mask });
				
			}
		} else SynErr(106);
		EndBlock();
	}

	void NewType() {
		scanner.InjectBlock(); 
		var ns0 = default(String);
		var ns1 = default(String);
		var extends = false;
		
		if (la.kind == 42) {
			Get();
		} else if (la.kind == 45) {
			Get();
			extends=true; 
		} else SynErr(107);
		Expect(1);
		ns0 = t.val; 
		if (la.kind == 24) {
			Get();
			Expect(1);
			ns1 = t.val; 
		}
		var nt = new ElaNewtype(t) { Name = ns1 ?? ns0, Prefix = ns1==null?null:ns0, Extends = extends };                	            
		nt.And = Program.Types;
		Program.Types = nt;
		
		if (la.kind == 52) {
			var cexp = default(ElaExpression); 
			Get();
			Expect(1);
			var c = new ElaConstructor(t) { Name = t.val };
			nt.Constructors.Add(c); 
			
			while (StartOf(14)) {
				Literal(out cexp);
				c.Expressions.Add(cexp); 
			}
			while (la.kind == 20) {
				Get();
				Expect(1);
				c = new ElaConstructor(t) { Name = t.val };
				nt.Constructors.Add(c); 
				
				while (StartOf(14)) {
					Literal(out cexp);
					c.Expressions.Add(cexp); 
				}
			}
		}
		EndBlock();
	}

	void ClassInstance() {
		var fst0 = default(String);
		var snd0 = default(String);
		var fst = default(String);
		var snd = default(String);
		var block = default(ElaEquationSet);
		var ot = t;
		var list = default(List<ElaClassInstance>);
		
		Expect(41);
		var ci = new ElaClassInstance(t);	            
		ci.And = Program.Instances;
		Program.Instances = ci;
		ot = t;
		
		Expect(1);
		fst0 = t.val; 
		if (la.kind == 24) {
			Get();
			Expect(1);
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
		
		while (la.kind == 1) {
			if (list == null)
			{
			    list = new List<ElaClassInstance>();
			    list.Add(ci);
			}
			
			ci = new ElaClassInstance();	   
			
			if (fst0 != null && snd0 != null)
			{
			    ci.TypeClassPrefix = fst0;
			    ci.TypeClassName = snd0;
			}
			else
			    ci.TypeClassName = fst0;	                
			         
			ci.SetLinePragma(ot.line, ot.col);
			ci.And = Program.Instances;
			Program.Instances = ci;
			list.Add(ci);
			
			Get();
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
			
		}
		if (la.kind == 40) {
			WhereBinding(out block);
			if (list == null)
			   ci.Where = block; 
			else
			{
			    for (var i = 0; i < list.Count; i++)
			        list[i].Where = block;
			}
			
		}
	}

	void TopLevel() {
		scanner.InjectBlock(); 
		if (StartOf(2)) {
			Binding(Program.TopLevel,null);
		} else if (la.kind == 30 || la.kind == 44) {
			IncludeStat();
		} else if (la.kind == 43) {
			TypeClass();
		} else if (la.kind == 42 || la.kind == 45) {
			NewType();
		} else if (la.kind == 41) {
			ClassInstance();
		} else SynErr(108);
		EndBlock();
		if (StartOf(18)) {
			TopLevel();
		}
	}

	void Ela() {
		Program = new ElaProgram(); 
		TopLevel();
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Ela();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,T,x, x,T,T,x, x,x,x,x, T,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,T,x, x,T,x,x, T,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, x,x,T,x, T,T,x,x, x},
		{T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,T,x, x,T,T,x, T,x,x,x, T,T,T,T, T,x,x,x, T,x,x,x, T,T,x,x, x,x,T,T, x,x,T,x, T,T,x,x, x},
		{x,T,T,T, T,T,x,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,T, T,T,T,T, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, T,T,T,T, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, x,T,T,T, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, x,x,T,T, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, x,x,x,T, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, x,x,x,x, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, x,x,x,x, T,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, x,x,x,x, x,T,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, x,x,x,x, x,x,T,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,T,T, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x},
		{x,T,x,x, x,x,T,T, T,T,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,T,x, x,x,T,x, x,T,T,x, T,x,x,x, T,T,T,T, x,T,T,T, T,T,x,x, T,T,x,x, x,x,T,T, x,x,T,x, T,T,x,x, x}

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
			case 6: s = "operatorTok0 expected"; break;
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
			case 41: s = "INSTANCE expected"; break;
			case 42: s = "TYPE expected"; break;
			case 43: s = "CLASS expected"; break;
			case 44: s = "IMPORT expected"; break;
			case 45: s = "DATA expected"; break;
			case 46: s = "EBLOCK expected"; break;
			case 47: s = "\"#\" expected"; break;
			case 48: s = "\"_\" expected"; break;
			case 49: s = "\"(\" expected"; break;
			case 50: s = "\")\" expected"; break;
			case 51: s = "\",\" expected"; break;
			case 52: s = "\"=\" expected"; break;
			case 53: s = "\"..\" expected"; break;
			case 54: s = "\"[&\" expected"; break;
			case 55: s = "\"::\" expected"; break;
			case 56: s = "\"or\" expected"; break;
			case 57: s = "\"and\" expected"; break;
			case 58: s = "\"`\" expected"; break;
			case 59: s = "\"$\" expected"; break;
			case 60: s = "\"__internal\" expected"; break;
			case 61: s = "\"&\" expected"; break;
			case 62: s = "\"<-\" expected"; break;
			case 63: s = "??? expected"; break;
			case 64: s = "invalid Literal"; break;
			case 65: s = "invalid Primitive"; break;
			case 66: s = "invalid ListLiteral"; break;
			case 67: s = "invalid As"; break;
			case 68: s = "invalid VariableReference"; break;
			case 69: s = "invalid RecordField"; break;
			case 70: s = "invalid ParamList"; break;
			case 71: s = "invalid Operators"; break;
			case 72: s = "this symbol not expected in MatchExpr"; break;
			case 73: s = "this symbol not expected in TryExpr"; break;
			case 74: s = "invalid OpExpr0"; break;
			case 75: s = "invalid OpExpr1b"; break;
			case 76: s = "invalid OpExpr1"; break;
			case 77: s = "invalid OpExpr2"; break;
			case 78: s = "invalid OpExpr3"; break;
			case 79: s = "invalid OpExpr4"; break;
			case 80: s = "invalid OpExpr5"; break;
			case 81: s = "invalid InfixExpr"; break;
			case 82: s = "invalid OpExpr6"; break;
			case 83: s = "invalid OpExpr7"; break;
			case 84: s = "invalid OpExpr8"; break;
			case 85: s = "invalid AccessExpr"; break;
			case 86: s = "invalid AccessExpr"; break;
			case 87: s = "invalid EmbExpr"; break;
			case 88: s = "this symbol not expected in LetBinding"; break;
			case 89: s = "this symbol not expected in LambdaExpr"; break;
			case 90: s = "invalid LambdaExpr"; break;
			case 91: s = "invalid LambdaGuard"; break;
			case 92: s = "this symbol not expected in WhereBinding"; break;
			case 93: s = "invalid Guard"; break;
			case 94: s = "this symbol not expected in IncludeStat"; break;
			case 95: s = "invalid IncludeStat"; break;
			case 96: s = "invalid IncludeStatElement"; break;
			case 97: s = "invalid Qualident"; break;
			case 98: s = "invalid TypeClass"; break;
			case 99: s = "invalid TypeClass"; break;
			case 100: s = "invalid TypeClass"; break;
			case 101: s = "invalid TypeClass"; break;
			case 102: s = "invalid TypeClass"; break;
			case 103: s = "invalid TypeClass"; break;
			case 104: s = "invalid TypeClass"; break;
			case 105: s = "invalid TypeClass"; break;
			case 106: s = "invalid TypeClass"; break;
			case 107: s = "invalid NewType"; break;
			case 108: s = "invalid TopLevel"; break;

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

