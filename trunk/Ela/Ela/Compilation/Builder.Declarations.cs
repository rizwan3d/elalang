using System;
using Ela.CodeModel;
using Ela.Runtime;
using Ela.Debug;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Main
		private void CompileDeclaration(ElaBinding s, LabelMap map, Hints hints)
		{
			if (s.InitExpression == null)
				AddError(ElaCompilerError.VariableDeclarationInitMissing, s);

            var data = -1;
			var flags = s.VariableFlags;

			if (s.InitExpression != null && s.InitExpression.Type == ElaNodeType.Builtin)
			{
				data = (Int32)((ElaBuiltin)s.InitExpression).Kind;
				flags |= ElaVariableFlags.Builtin;

                if (String.IsNullOrEmpty(s.VariableName))
                    AddError(ElaCompilerError.InvalidBuiltinBinding, s);
			}

            if ((s.VariableFlags & ElaVariableFlags.Extends) == ElaVariableFlags.Extends && s.InitExpression != null
                && s.InitExpression.Type != ElaNodeType.FunctionLiteral)
                AddError(ElaCompilerError.ExtendsOnlyFunctions, s, s);

            if ((s.VariableFlags & ElaVariableFlags.Extends) == ElaVariableFlags.Extends &&
                (hints & Hints.And) != Hints.And)
            {
                AddOldNames(s);
            }
			
			if (s.In != null)
				StartScope(false, s.In.Line, s.In.Column);

            if ((s.VariableFlags & ElaVariableFlags.Private) == ElaVariableFlags.Private &&
                CurrentScope != globalScope)
                AddError(ElaCompilerError.PrivateOnlyGlobal, s);

            var inline = (s.VariableFlags & ElaVariableFlags.Inline) == ElaVariableFlags.Inline;
			
			if (s.Pattern == null)
			{
				var addr = -1;
				var addSym = false;
                var lastSym = default(VarSym);
				
				if (s.InitExpression != null)// && s.InitExpression.Type == ElaNodeType.FunctionLiteral)
				{
					addr = (hints & Hints.And) == Hints.And ? GetNoInitVariable(s.VariableName) : AddVariable(s.VariableName, s, flags, data);
                    lastSym = pdb != null ? pdb.LastVarSym : null;

                    if (inline && s.InitExpression.Type == ElaNodeType.FunctionLiteral)
					{
                        var fun = (ElaFunctionLiteral)s.InitExpression;
                        inlineFuns.Remove(fun.Name);
						inlineFuns.Add(fun.Name, new InlineFun(fun, CurrentScope));
					}
				}
				else
				{
                    addr = GetNoInitVariable(s.VariableName);
                    addSym = true;
				}

				var po = cw.Offset;
				var and = s.And;
                var noInitCode = allowNoInits.Count;

				while (and != null && (hints & Hints.And) != Hints.And)
				{
                    AddNoInitVariable(and, noInitCode);
                    and = and.And;
				}

				if (s.Where != null)
					CompileWhere(s.Where, map, Hints.Left);

                allowNoInits.Push(new NoInit(noInitCode, !addSym));

                if (s.InitExpression.Type == ElaNodeType.Builtin)
                    map.BuiltinName = s.VariableName;

				var ed = s.InitExpression != null ? CompileExpression(s.InitExpression, map,
                    (s.VariableFlags & ElaVariableFlags.Extends) == ElaVariableFlags.Extends ? Hints.Extends : Hints.None) : default(ExprData);
				var fc = ed.Type == DataKind.FunCurry || ed.Type == DataKind.FunParams;
				allowNoInits.Pop();

				if (ed.Type == DataKind.FunParams && addSym)
				{
					pdb.StartFunction(s.VariableName, po, ed.Data);
					pdb.EndFunction(-1, cw.Offset);
				}

				if (s.Where != null)
					EndScope();

				if (addr != -1)
				{
                    if (fc)
                    {
                        CurrentScope.ChangeVariable(s.VariableName, new ScopeVar(s.VariableFlags | ElaVariableFlags.Function, addr >> 8, ed.Data));

                        if (lastSym != null)
                        {
                            lastSym.Flags = (Int32)(s.VariableFlags | ElaVariableFlags.Function);
                            lastSym.Data = ed.Data;
                        }
                    }
                    else
                    {
                        CurrentScope.ChangeVariable(s.VariableName, new ScopeVar(s.VariableFlags | flags, addr >> 8, data));

                        if (lastSym != null)
                        {
                            lastSym.Flags = (Int32)(s.VariableFlags | flags);
                            lastSym.Data = data;
                        }
                    }
				}
				else if (addr == -1)
				{
					if (fc)
						flags |= ElaVariableFlags.Function;

					if (ed.Type == DataKind.Builtin)
						flags |= ElaVariableFlags.Builtin;

					addr = AddVariable(s.VariableName, s, flags, data != -1 ? data : ed.Data);
				}

				AddLinePragma(s);
				cw.Emit(Op.Popvar, addr);
			}
			else
				CompileBindingPattern(s, map, hints);

			var newHints = hints | (s.In != null ? Hints.Scope : Hints.None) | Hints.And;

			if (s.And != null)
				CompileExpression(s.And, map, s.In != null ? (newHints | Hints.Left) : newHints);
			
			if (s.In != null)
				CompileIn(s, map, newHints);
			else if ((hints & Hints.Left) != Hints.Left && s.And == null)
				cw.Emit(Op.Pushunit);

			if (s.In != null)
				EndScope();
		}

        private void AddOldNames(ElaBinding s)
        {
            var and = s;
            
            while (and != null)
            {
                var sa = default(ScopeVar);

                if (and.Pattern != null)
                    AddError(ElaCompilerError.ExtendsNameMissing, and, and.Pattern);
                else
                    sa = GetVariable(and.VariableName, and.Line, and.Column);

                EmitVar(sa);
                var a = AddVariable("$" + and.VariableName, and, ElaVariableFlags.SpecialName | ElaVariableFlags.Private, -1);
                cw.Emit(Op.Popvar, a);
                and = and.And;
            }            
        }

        private int GetNoInitVariable(string name)
        {
            ScopeVar var;

            if (CurrentScope.Locals.TryGetValue(name, out var))
            {
                if ((var.Flags & ElaVariableFlags.NoInit) != ElaVariableFlags.NoInit)
                    return -1;
                else
                    return 0 | var.Address << 8;
            }

            return -1;
        }

        private void AddNoInitVariable(ElaBinding exp, int noInitCode)
        {
            if (!String.IsNullOrEmpty(exp.VariableName))
                AddVariable(exp.VariableName, exp, exp.VariableFlags | ElaVariableFlags.NoInit, noInitCode);
            else
                AddPatternVariables(exp.Pattern, noInitCode);
        }

        private void AddPatternVariables(ElaPattern pat, int noInitCode)
        {
            switch (pat.Type)
            {
                case ElaNodeType.VariantPattern:
                    {
                        var vp = (ElaVariantPattern)pat;

                        if (vp.Pattern != null)
                            AddPatternVariables(vp.Pattern, noInitCode);
                    }
                    break;
                case ElaNodeType.IsPattern:
                case ElaNodeType.UnitPattern: //Idle
                    break;
                case ElaNodeType.AsPattern:
                    {
                        var asPat = (ElaAsPattern)pat;
                        AddVariable(asPat.Name, asPat, ElaVariableFlags.NoInit, noInitCode);
                        AddPatternVariables(asPat.Pattern, noInitCode);
                    }
                    break;
                case ElaNodeType.LiteralPattern: //Idle
                    break;
                case ElaNodeType.VariablePattern:
                    {
                        var vexp = (ElaVariablePattern)pat;
                        AddVariable(vexp.Name, vexp, ElaVariableFlags.NoInit, noInitCode);
                    }
                    break;
                case ElaNodeType.RecordPattern:
                    {
                        var rexp = (ElaRecordPattern)pat;

                        foreach (var e in rexp.Fields)
                            if (e.Value != null)
                                AddPatternVariables(e.Value, noInitCode);
                    }
                    break;
                case ElaNodeType.PatternGroup:
                case ElaNodeType.TuplePattern:
                    {
                        var texp = (ElaTuplePattern)pat;

                        foreach (var e in texp.Patterns)
                            AddPatternVariables(e, noInitCode);
                    }
                    break;
                case ElaNodeType.DefaultPattern: //Idle
                    break;
                case ElaNodeType.HeadTailPattern:
                    {
                        var hexp = (ElaHeadTailPattern)pat;

                        foreach (var e in hexp.Patterns)
                            AddPatternVariables(e, noInitCode);
                    }
                    break;
                case ElaNodeType.NilPattern: //Idle
                    break;
            }
        }

		private void CompileWhere(ElaBinding s, LabelMap map, Hints hints)
		{
			StartScope(false, s.Line, s.Column);
			CompileExpression(s, map, hints);
		}
        
		private void CompileIn(ElaBinding s, LabelMap map, Hints hints)
		{
			if (s.In != null)
			{
				if ((hints & Hints.Scope) != Hints.Scope)
					StartScope(false, s.Line, s.Column);

				var newHints = (hints & Hints.And) == Hints.And ?
					hints ^ Hints.And : hints;
				CompileExpression(s.In, map, newHints | Hints.Scope);

				if ((hints & Hints.Scope) != Hints.Scope)
					EndScope();
			}
		}
        
		private void CompileBindingPattern(ElaBinding s, LabelMap map, Hints hints)
		{
            //if (s.Pattern.Type == ElaNodeType.DefaultPattern)
            //{
            //    if (s.Where != null)
            //        CompileWhere(s.Where, map, Hints.Left);

            //    CompileExpression(s.InitExpression, map, Hints.Nested);

            //    if (s.Where != null)
            //        EndScope();

            //    cw.Emit(Op.Pop);
            //}
            //else
			{
				var next = cw.DefineLabel();
				var exit = cw.DefineLabel();
				var addr = -1;
				var tuple = default(ElaTupleLiteral);

				if (s.InitExpression.Type == ElaNodeType.TupleLiteral && s.Pattern.Type == ElaNodeType.TuplePattern && s.Where == null)
					tuple = (ElaTupleLiteral)s.InitExpression;
				else
				{
                    var po = cw.Offset;
                    var and = s.And;
                    var noInitCode = allowNoInits.Count;
                    var allow = s.InitExpression != null;// && s.InitExpression.Type == ElaNodeType.FunctionLiteral;
                    AddNoInitVariable(s, noInitCode);

                    while (and != null && (hints & Hints.And) != Hints.And)
                    {
                        AddNoInitVariable(and, noInitCode);
                        and = and.And;
                    }

                    if (s.Where != null)
                        CompileWhere(s.Where, map, Hints.Left);

                    allowNoInits.Push(new NoInit(noInitCode, allow));
                    
                    if (s.InitExpression != null) 
                        CompileExpression(s.InitExpression, map, Hints.None);
                    
                    allowNoInits.Pop();

                    if (s.Where != null)
                        EndScope();

                    addr = AddVariable();
                    cw.Emit(Op.Popvar, addr);
				}

				CompilePattern(addr, tuple, s.Pattern, map, next, s.VariableFlags, Hints.Silent);
				cw.Emit(Op.Br, exit);
				cw.MarkLabel(next);
				cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
				cw.MarkLabel(exit);
				cw.Emit(Op.Nop);
			}
		}
		#endregion
	}
}