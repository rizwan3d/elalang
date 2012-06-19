using System;
using Ela.CodeModel;
using Ela.Runtime;
using Ela.Debug;
using System.Collections.Generic;

namespace Ela.Compilation
{
	internal sealed partial class Builder
    {
        private void CompileDeclaration(ElaBinding s, LabelMap map, Hints hints)
		{
            if (s.InitExpression == null)
            {
                AddError(ElaCompilerError.VariableDeclarationInitMissing, s);
                return;
            }

            var data = -1;
			var flags = s.VariableFlags;

			if (s.InitExpression.Type == ElaNodeType.Builtin)
			{
				data = (Int32)((ElaBuiltin)s.InitExpression).Kind;
				flags |= ElaVariableFlags.Builtin;

                if (String.IsNullOrEmpty(s.VariableName))
                    AddError(ElaCompilerError.InvalidBuiltinBinding, s);
			}
			
			if (s.In != null)
				StartScope(false, s.In.Line, s.In.Column);

            if ((s.VariableFlags & ElaVariableFlags.Private) == ElaVariableFlags.Private && CurrentScope != globalScope)
                AddError(ElaCompilerError.PrivateOnlyGlobal, s);

            var inline = (s.VariableFlags & ElaVariableFlags.Inline) == ElaVariableFlags.Inline;
            var inlineData = -1;
			
			if (s.Pattern == null)
			{
				var addr = -1;
				var addSym = false;
                var lastSym = default(VarSym);

                if (!s.MemberBinding)
                {
                    var andHint = (hints & Hints.And) == Hints.And;

                    if (CurrentScope == globalScope || andHint)
                        addr = GetNoInitVariable(s.VariableName);

                    if (addr == -1)
                        addr = AddVariable(s.VariableName, s, flags, data);
                    
                    lastSym = pdb != null ? pdb.LastVarSym : null;

                    if (inline && s.InitExpression.Type == ElaNodeType.FunctionLiteral)
                    {
                        var fun = (ElaFunctionLiteral)s.InitExpression;
                        inlineData = inlineFuns.Count;
                        inlineFuns.Add(new InlineFun(fun, CurrentScope));
                    }
                }
                else
                {
                    addr = AddVariable();
                    addSym = true;
                }

				var po = cw.Offset;
				var and = s.And;

                if ((hints & Hints.And) != Hints.And && CurrentScope != globalScope)
                {
                    while (and != null)
                    {
                        if (!and.MemberBinding)
                            AddNoInitVariable(and);

                        and = and.And;
                    }
                }

                if (s.InitExpression.Type == ElaNodeType.Builtin)
                    map.BuiltinName = s.VariableName;

                var expHints = s.InitExpression.Type != ElaNodeType.LazyLiteral && s.InitExpression.Type != ElaNodeType.FunctionLiteral ? Hints.Lazy : Hints.None;

                var ed = default(ExprData);

                if (s.MemberBinding)
                {
                    var btVar = GetVariable(s.VariableName, s.Line, s.Column);
                    var args = 1;

                    if ((btVar.Flags & ElaVariableFlags.Builtin) == ElaVariableFlags.Builtin)
                        args = BuiltinParams((ElaBuiltinKind)btVar.Data);
                    else
                        args = btVar.Data;

                    if (s.InitExpression.Type != ElaNodeType.FunctionLiteral)
                        EtaExpand(s.InitExpression, map, expHints, args);
                    else if (((ElaFunctionLiteral)s.InitExpression).ParameterCount < args)
                        EtaExpand(s.InitExpression, map, expHints, args);
                    else
                        CompileExpression(s.InitExpression, map, expHints);
                }
                else
                    ed = CompileStrictOrLazy(s, s.InitExpression, map, expHints);
                				
				var fc = ed.Type == DataKind.FunParams;
				
				if (ed.Type == DataKind.FunParams && addSym)
				{
					pdb.StartFunction(s.VariableName, po, ed.Data);
					pdb.EndFunction(-1, cw.Offset);
				}

				if (!s.MemberBinding)
				{
                    if (fc)
                    {
                        CurrentScope.ChangeVariable(s.VariableName, new ScopeVar(s.VariableFlags | ElaVariableFlags.Function, addr >> 8, 
                            inline ? inlineData : ed.Data));

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

				AddLinePragma(s);
                PopVar(addr);

                if (s.MemberBinding)
                {
                    PushVar(addr);
                    var sv = GetVariable(s.VariableName, CurrentScope.Parent, GetFlags.None, s.Line, s.Column);

                    if ((sv.Flags & ElaVariableFlags.Builtin) != ElaVariableFlags.Builtin)
                        PushVar(sv);
                    else
                        cw.Emit(Op.PushI4, (Int32)sv.Data);

                    EmitSpecName(currentTypePrefix, "$$" + currentType, s, ElaCompilerError.UndefinedType);
                    cw.Emit(Op.Addmbr);
                }
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

        private void EtaExpand(ElaExpression exp, LabelMap map, Hints hints, int args)
        {
            StartSection();
            StartScope(true, exp.Line, exp.Column);
            cw.StartFrame(args);
            var funSkipLabel = cw.DefineLabel();
            cw.Emit(Op.Br, funSkipLabel);
            var address = cw.Offset;
            CompileExpression(exp, map, (hints & Hints.Lazy) == Hints.Lazy ? (hints ^ Hints.Lazy) : hints);

            for (var i = 0; i < args; i++)
                cw.Emit(Op.Call);

            cw.Emit(Op.Ret);
            frame.Layouts.Add(new MemoryLayout(currentCounter, cw.FinishFrame(), address));
            EndSection();
            EndScope();
            cw.MarkLabel(funSkipLabel);
            cw.Emit(Op.PushI4, args);
            cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
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

        private void AddNoInitVariable(ElaBinding exp)
        {
            if (!String.IsNullOrEmpty(exp.VariableName))
                AddVariable(exp.VariableName, exp, exp.VariableFlags | ElaVariableFlags.NoInit, -1);
            else
                AddPatternVariables(exp.Pattern);
        }

        private void AddPatternVariables(ElaPattern pat)
        {
            switch (pat.Type)
            {
                case ElaNodeType.VariantPattern:
                    {
                        var vp = (ElaVariantPattern)pat;

                        if (vp.Pattern != null)
                            AddPatternVariables(vp.Pattern);
                    }
                    break;
                case ElaNodeType.IsPattern:
                case ElaNodeType.UnitPattern: //Idle
                    break;
                case ElaNodeType.AsPattern:
                    {
                        var asPat = (ElaAsPattern)pat;
                        AddVariable(asPat.Name, asPat, ElaVariableFlags.NoInit, -1);
                        AddPatternVariables(asPat.Pattern);
                    }
                    break;
                case ElaNodeType.LiteralPattern: //Idle
                    break;
                case ElaNodeType.VariablePattern:
                    {
                        var vexp = (ElaVariablePattern)pat;
                        AddVariable(vexp.Name, vexp, ElaVariableFlags.NoInit, -1);
                    }
                    break;
                case ElaNodeType.RecordPattern:
                    {
                        var rexp = (ElaRecordPattern)pat;

                        foreach (var e in rexp.Fields)
                            if (e.Value != null)
                                AddPatternVariables(e.Value);
                    }
                    break;
                case ElaNodeType.PatternGroup:
                case ElaNodeType.TuplePattern:
                    {
                        var texp = (ElaTuplePattern)pat;

                        foreach (var e in texp.Patterns)
                            AddPatternVariables(e);
                    }
                    break;
                case ElaNodeType.DefaultPattern: //Idle
                    break;
                case ElaNodeType.HeadTailPattern:
                    {
                        var hexp = (ElaHeadTailPattern)pat;

                        foreach (var e in hexp.Patterns)
                            AddPatternVariables(e);
                    }
                    break;
                case ElaNodeType.NilPattern: //Idle
                    break;
            }
        }
        
		private void CompileIn(ElaBinding s, LabelMap map, Hints hints)
		{
			if (s.In != null)
			{
				if ((hints & Hints.Scope) != Hints.Scope)
					StartScope(false, s.Line, s.Column);

				var newHints = (hints & Hints.And) == Hints.And ?
					hints ^ Hints.And : hints;
                CompileStrictOrLazy(null, s.In, map, newHints | Hints.Scope);

				if ((hints & Hints.Scope) != Hints.Scope)
					EndScope();
			}
		}
        
		private void CompileBindingPattern(ElaBinding s, LabelMap map, Hints hints)
		{
            var next = cw.DefineLabel();
			var exit = cw.DefineLabel();
			var addr = -1;
			var tuple = default(ElaTupleLiteral);

			if (s.InitExpression.Type == ElaNodeType.TupleLiteral && s.Pattern.Type == ElaNodeType.TuplePattern)
				tuple = (ElaTupleLiteral)s.InitExpression;
			
			var po = cw.Offset;
            var and = s.And;

            if (CurrentScope != globalScope && (hints & Hints.And) != Hints.And)
            {
                AddNoInitVariable(s);

                while (and != null)
                {
                    AddNoInitVariable(and);
                    and = and.And;
                }
            }

            if (s.InitExpression != null) 
                CompileExpression(s.InitExpression, map, Hints.None);

            addr = AddVariable();
            PopVar(addr);

			CompilePattern(addr, tuple, s.Pattern, map, next, s.VariableFlags, Hints.None);
			cw.Emit(Op.Br, exit);
			cw.MarkLabel(next);
			cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
			cw.MarkLabel(exit);
			cw.Emit(Op.Nop);
		}
    }
}