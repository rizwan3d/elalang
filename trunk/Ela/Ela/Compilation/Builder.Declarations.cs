using System;
using Ela.CodeModel;
using Ela.Runtime;
using Ela.Debug;
using System.Collections.Generic;

namespace Ela.Compilation
{
	internal sealed partial class Builder
    {
        #region Forward Declaration
        private string typeName;
        private void RunTypes(ElaBlock block, LabelMap map, Hints hints)
        {
            var len = block.Expressions.Count;

            for (var idx = 0; idx < len; idx++)
            {
                var exp = block.Expressions[idx];
                var last = idx == len - 1;

                if (exp.Type == ElaNodeType.Newtype)
                {
                    var v = (ElaNewtype)exp;
                    var tc = -1;

                    if (v.Body == null)
                    {
                        tc = (Int32)TypeCodeFormat.GetTypeCode(v.Name);

                        if (tc == 0)
                            AddError(ElaCompilerError.OnlyBuiltinTypeNoDefinition, v, v.Name);
                       
                        var sca = AddVariable("$$" + v.Name, v, ElaVariableFlags.None, -1);
                        cw.Emit(Op.PushI4, tc);
                        PopVar(sca);
                    }

                    if (frame.Types.ContainsKey(v.Name))
                        AddError(ElaCompilerError.TypeAlreadyDeclared, v, v.Name);
                    else
                    {
                        frame.Types.Add(v.Name, tc);

                        if (v.Body != null)
                        {
                            typeName = v.Name;
                            CompileFunction(v, FunFlag.Newtype);
                            var addr = AddVariable(v.Name, v, ElaVariableFlags.TypeFun, -1);
                            AddLinePragma(exp);
                            PopVar(addr);
                            var sa = AddVariable("$$" + v.Name, v, ElaVariableFlags.None, -1);
                            cw.Emit(Op.Typeid, AddString(v.Name));
                            PopVar(sa);
                        }
                    }
                }                
            }
        }

        private void RunInstances(ElaBlock block, LabelMap map, Hints hints)
        {
            var len = block.Expressions.Count;

            for (var idx = 0; idx < len; idx++)
            {
                var exp = block.Expressions[idx];
                var last = idx == len - 1;

                if (exp.Type == ElaNodeType.ClassInstance)
                {
                    var v = (ElaClassInstance)exp;
                    CompileInstance(v, map, last ? Hints.None : hints);
                }
            }
        }

        private void RunForwardDeclaration(ElaBlock block, LabelMap map, Hints hints)
        {
            var len = block.Expressions.Count;

            for (var idx = 0; idx < len; idx++)
            {
                var exp = block.Expressions[idx];
                var last = idx == len - 1;

                switch (exp.Type)
                {
                    case ElaNodeType.TypeClass:
                        {
                            var v = (ElaTypeClass)exp;
                            CompileClass(v, map, last ? Hints.None : Hints.Left);
                        }
                        break;
                    case ElaNodeType.Binding:
                        {
                            var v = (ElaBinding)exp;

                            if (v.In != null)
                                break;

                            var and = v;

                            do
                            {
                                AddNoInitVariable(and);
                                and = and.And;
                            }
                            while (and != null);
                        }
                        break;
                    case ElaNodeType.ModuleInclude:
                        {
                            var s = (ElaModuleInclude)exp;
                            CompileModuleInclude(s, map, last ? Hints.None : Hints.Left);
                        }
                        break;
                }
            }
        }
        #endregion


        #region Variables
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
                    ed = CompileExpression(s.InitExpression, map, expHints);
                				
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
				CompileExpression(s.In, map, newHints | Hints.Scope);

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
		#endregion

        #region Classes
        public void CompileClass(ElaTypeClass s, LabelMap map, Hints hints)
        {
            if (s.BuiltinName != null)
                CompileBuiltinClass(s, map, hints);
            else
            {
                for (var i = 0; i < s.Members.Count; i++)
                {
                    var m = s.Members[i];

                    if (m.Mask == 0)
                        AddError(ElaCompilerError.InvalidMemberSignature, m, m.Name);

                    var addr = AddVariable(m.Name, m, ElaVariableFlags.ClassFun, m.Arguments);
                    cw.Emit(Op.PushI4, m.Mask);
                    cw.Emit(Op.PushI4, m.Arguments);
                    cw.Emit(Op.Newfunc, AddString(m.Name));
                    PopVar(addr);
                }
            }

            var sa = AddVariable("$$$" + s.Name, s, ElaVariableFlags.None, -1);
            cw.Emit(Op.Classid, AddString(s.Name));
            PopVar(sa);
            
            if (frame.Classes.ContainsKey(s.Name))
                AddError(ElaCompilerError.ClassAlreadyDeclared, s, s.Name);
            else
                frame.Classes.Add(s.Name, new ClassData(s.Members.ToArray()));

            //A class declaration might be the last statement in a module
            if ((hints & Hints.Left) != Hints.Left)
                cw.Emit(Op.Pushunit);
        }

        private void CompileBuiltinClass(ElaTypeClass s, LabelMap map, Hints hints)
        {
            switch (s.BuiltinName)
            {
                case "Typeable":
                    CompileBuiltinMember(ElaBuiltinKind.Cast, s, 0, map);
                    break;
                case "Eq":
                    CompileBuiltinMember(ElaBuiltinKind.Equal, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.NotEqual, s, 1, map);
                    break;
                case "Ord":
                    CompileBuiltinMember(ElaBuiltinKind.Greater, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Lesser, s, 1, map);
                    CompileBuiltinMember(ElaBuiltinKind.GreaterEqual, s, 2, map);
                    CompileBuiltinMember(ElaBuiltinKind.LesserEqual, s, 3, map);
                    break;
                case "Num":
                    CompileBuiltinMember(ElaBuiltinKind.Add, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Subtract, s, 1, map);
                    CompileBuiltinMember(ElaBuiltinKind.Multiply, s, 2, map);
                    CompileBuiltinMember(ElaBuiltinKind.Divide, s, 3, map);
                    CompileBuiltinMember(ElaBuiltinKind.Power, s, 4, map);
                    CompileBuiltinMember(ElaBuiltinKind.Remainder, s, 5, map);
                    CompileBuiltinMember(ElaBuiltinKind.Negate, s, 6, map);
                    break;
                case "Bit":
                    CompileBuiltinMember(ElaBuiltinKind.BitwiseAnd, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.BitwiseOr, s, 1, map);
                    CompileBuiltinMember(ElaBuiltinKind.BitwiseXor, s, 2, map);
                    CompileBuiltinMember(ElaBuiltinKind.BitwiseNot, s, 3, map);
                    CompileBuiltinMember(ElaBuiltinKind.ShiftLeft, s, 4, map);
                    CompileBuiltinMember(ElaBuiltinKind.ShiftRight, s, 5, map);
                    break;
                case "Enum":
                    CompileBuiltinMember(ElaBuiltinKind.Succ, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Pred, s, 1, map);
                    break;
                case "Seq":
                    CompileBuiltinMember(ElaBuiltinKind.Head, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Tail, s, 1, map);
                    CompileBuiltinMember(ElaBuiltinKind.IsNil, s, 2, map);
                    break;
                case "Ix":
                    CompileBuiltinMember(ElaBuiltinKind.Get, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Length, s, 1, map);
                    break;
                case "Cons":
                    CompileBuiltinMember(ElaBuiltinKind.Cons, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Nil, s, 1, map);
                    break;
                case "Cat":
                    CompileBuiltinMember(ElaBuiltinKind.Concat, s, 0, map);
                    break;
                case "Show":
                    CompileBuiltinMember(ElaBuiltinKind.Showf, s, 0, map);
                    break;
                default:
                    AddError(ElaCompilerError.InvalidBuiltinClass, s, s.BuiltinName);
                    break;
            }
        }

        private void CompileBuiltinMember(ElaBuiltinKind kind, ElaTypeClass s, int memIndex, LabelMap map)
        {
            var flags = ElaVariableFlags.Builtin | ElaVariableFlags.ClassFun;

            if (memIndex > s.Members.Count - 1)
            {
                AddError(ElaCompilerError.InvalidBuiltinClassDefinition, s, s.BuiltinName);
                return;
            }

            var m = s.Members[memIndex];
            CompileBuiltin(kind, m, map);
            PopVar(AddVariable(m.Name, m, flags, (Int32)kind));
        }

        private string currentType;
        private string currentTypePrefix;
        public void CompileInstance(ElaClassInstance s, LabelMap map, Hints hints)
        {
            currentType = s.TypeName;
            currentTypePrefix = s.TypePrefix;
            var mod = default(CodeFrame);
            var mbr = default(ClassData);
            var modId = -1;

            if (s.TypeClassPrefix == null)
            {
                if (!frame.Classes.TryGetValue(s.TypeClassName, out mbr))
                {
                    var sv = GetVariable("$$$" + s.TypeClassName, CurrentScope, GetFlags.NoError, s.Line, s.Column);

                    if (sv.IsEmpty())
                    {
                        AddError(ElaCompilerError.UnknownClass, s, s.TypeClassName);
                        return;
                    }

                    modId = sv.Address & Byte.MaxValue;
                    mod = refs[modId];
                }
            }
            else
            {
                var sv = GetVariable(s.TypeClassPrefix, s.Line, s.Column);

                if (sv.IsEmpty())
                    return;

                if ((sv.Flags & ElaVariableFlags.Module) != ElaVariableFlags.Module)
                {
                    AddError(ElaCompilerError.InvalidQualident, s, s.TypeClassPrefix);
                    return;
                }

                modId = frame.References[s.TypeClassPrefix].LogicalHandle;
                mod = refs[modId];
            }
            
            if (mbr == null && (mod == null || !mod.Classes.TryGetValue(s.TypeClassName, out mbr)))
            {
                AddError(ElaCompilerError.UnknownClass, s, s.TypeClassName);
                return;
            }

            var typCode = -1;
            
            if (s.TypePrefix == null)
            {
                if (!frame.Types.ContainsKey(s.TypeName))
                {
                    var sv = GetVariable("$$" + s.TypeName, CurrentScope, GetFlags.NoError, s.Line, s.Column);

                    if (sv.IsEmpty())
                    {
                        AddError(ElaCompilerError.UndefinedType, s, s.TypeName);
                        return;
                    }

                    typCode = sv.Address & Byte.MaxValue;
                }
            }
            else
            {
                var sv = GetVariable(s.TypePrefix, s.Line, s.Column);

                if (sv.IsEmpty())
                    return;

                if ((sv.Flags & ElaVariableFlags.Module) != ElaVariableFlags.Module)
                {
                    AddError(ElaCompilerError.InvalidQualident, s, s.TypePrefix);
                    return;
                }

                typCode = frame.References[s.TypeClassPrefix].LogicalHandle;
            }

            frame.Instances.Add(new InstanceData(s.TypeName, s.TypeClassName, typCode, modId, s.Line, s.Column));

            var classMembers = new List<String>(mbr.Members.Length);

            for (var i = 0; i < mbr.Members.Length; i++)
                classMembers.Add(mbr.Members[i].Name);

            var b = s.Where;
            var err = false;

            while (b != null)
            {
                if (b.Pattern != null)
                {
                    AddError(ElaCompilerError.MemberNoPatterns, b.Pattern, b.Pattern.ToString());
                    err = true;
                }

                if (!String.IsNullOrEmpty(b.VariableName))
                {
                    if (classMembers.Contains(b.VariableName))
                    {
                        b.MemberBinding = true;
                        classMembers.Remove(b.VariableName);
                    }
                }

                b = b.And;
            }

            if (s.Where == null && IsAutoGenerated(s))
                return;

            if (classMembers.Count > 0)
                AddError(ElaCompilerError.MemberNotAll, s, s.TypeClassName, s.TypeClassName + " " + s.TypeName);

            if (!err)
            {
                StartScope(false, s.Where.Line, s.Where.Column);
                CompileDeclaration(s.Where, map, hints);
                EndScope();
            }
        }

        private bool IsAutoGenerated(ElaClassInstance b)
        {
            if (b.TypePrefix != null || b.TypeClassPrefix != null ||
                TypeCodeFormat.GetTypeCode(b.TypeName) == ElaTypeCode.None)
                return false;

            switch (b.TypeClassName)
            {
                case "Typeable":
                case "Eq":
                case "Ord":
                case "Num":
                case "Bit":
                case "Enum":
                case "Seq":
                case "Ix":
                case "Cons":
                case "Cat":
                case "Show":
                    return true;
                default:
                    return false;
            }
        }
        #endregion
    }
}