using System;
using Ela.CodeModel;
using Ela.Runtime;
using System.Collections.Generic;

namespace Ela.Compilation
{
	internal sealed partial class Builder
    {
        //Compile all declarations including function bindings, name bindings and bindings
        //defined by pattern matching.
        private void CompileDeclaration(ElaEquation s, LabelMap map, Hints hints)
        {
            //Check for some errors
            ValidateBinding(s);
            var partial = (s.VariableFlags & ElaVariableFlags.PartiallyApplied) == ElaVariableFlags.PartiallyApplied;
            var fun = partial || s.IsFunction();

            if ((s.Left.Type != ElaNodeType.NameReference || ((ElaNameReference)s.Left).Uppercase) && !fun)
            {
                if ((hints & Hints.Lazy) == Hints.Lazy)                    
                    CompileLazyPattern(s, map);
                else
                    CompileBindingPattern(s, map);
            }
            else
            {
                var nm = default(String);
                var sv = GetNoInitVariable(s, out nm);
                var addr = sv.Address;

                if (sv.IsEmpty())
                    addr = AddVariable(s.Left.GetName(), s, s.VariableFlags, -1);
                else
                    CurrentScope.AddFlags(nm, ElaVariableFlags.Self);

                //Compile expression and write it to a variable
                if (fun)
                {
                    //Here we automatically eta expand functions defined through partial application
                    if (partial)
                        EtaExpand(s.Left.GetName(), s.Right, map, s.Arguments);
                    else
                        CompileFunction(s);
                }
                else
                {
                    map.BindingName = s.Left.GetName();

                    if ((hints & Hints.Lazy) == Hints.Lazy)
                        CompileLazyExpression(s.Right, map, Hints.None);
                    else
                        CompileExpression(s.Right, map, Hints.None, s);
                }

                //Now, when done initialization, when can remove NoInit flags.
                if (!sv.IsEmpty())
                    CurrentScope.RemoveFlags(nm, ElaVariableFlags.NoInit | ElaVariableFlags.Self);

                AddLinePragma(s);
                PopVar(addr);
            }
        }

        //Compiles a binding defined by pattern matching.
        private void CompileBindingPattern(ElaEquation s, LabelMap map)
        {
            //Compile a right hand expression. Currently it is always compiled, event if right-hand
            //and both left-hand are tuples (however an optimization can be applied in such a case).
            var sys = AddVariable();
            CompileExpression(s.Right, map, Hints.None, s);
            AddLinePragma(s);
            PopVar(sys);

            //Labels needed for pattern compilation
            var next = cw.DefineLabel();
            var exit = cw.DefineLabel();

            //Here we compile a pattern a generate a 'handling logic' that raises a MatchFailed exception
            CompilePattern(sys, s.Left, next);
            cw.Emit(Op.Br, exit);
            cw.MarkLabel(next);
            cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
            cw.MarkLabel(exit);
            cw.Emit(Op.Nop);
        }

        //Validate correctness of a binding
        private void ValidateBinding(ElaEquation s)
        {
            //These errors are not critical and allow to continue compilation
            if (s.Right.Type == ElaNodeType.Builtin && s.Left.Type != ElaNodeType.NameReference)
                AddError(ElaCompilerError.InvalidBuiltinBinding, s);

            if ((s.VariableFlags & ElaVariableFlags.Private) == ElaVariableFlags.Private && CurrentScope != globalScope)
                AddError(ElaCompilerError.PrivateOnlyGlobal, s);
        }

        //Returns a variable from a local scope marked with NoInit flag
        //If such variable couldn't be found returns -1
        private ScopeVar GetNoInitVariable(ElaEquation s, out string name)
        {
            ScopeVar var;
            name = s.Left.GetName();

            if (s.IsFunction())
                name = s.GetFunctionName();
            
            if (CurrentScope.Locals.TryGetValue(name, out var))
            {
                //If it doesn't have a NoInit flag we are not good
                if ((var.Flags & ElaVariableFlags.NoInit) != ElaVariableFlags.NoInit)
                    return ScopeVar.Empty;
                else
                {
                    var.Address = 0 | var.Address << 8; //Aligning it to local scope
                    return var;
                }
            }

            return ScopeVar.Empty;
        }

        //Adds a variable with NoInit flag to the current scope
        //This method also calculates additional flags and metadata for variables.
        //If a given binding if defined by pattern matching then all variables from
        //patterns are traversed using AddPatternVariable method.
        private bool AddNoInitVariable(ElaEquation exp)
        {
            var flags = exp.VariableFlags | ElaVariableFlags.NoInit;
            
            //This binding is not defined by PM
            if (exp.IsFunction())
            {
                var name = exp.GetFunctionName();

                if (name == null || Char.IsUpper(name[0]))
                {
                    AddError(ElaCompilerError.InvalidFunctionDeclaration, exp, FormatNode(exp.Left));
                    return false;
                }

                AddVariable(name, exp.Left, flags | ElaVariableFlags.Function, exp.GetArgumentNumber());
            }
            else if (exp.Left.Type == ElaNodeType.NameReference && !((ElaNameReference)exp.Left).Uppercase)
            {
               var data = -1;

               if (exp.Right.Type == ElaNodeType.Builtin)
               {
                   //Adding required hints for a built-in
                   data = (Int32)((ElaBuiltin)exp.Right).Kind;
                   flags |= ElaVariableFlags.Builtin;
               }
               else if (exp.Right.Type == ElaNodeType.Lambda)
               {
                   var fun = (ElaLambda)exp.Right;
                   flags |= ElaVariableFlags.Function;
                   data = fun.GetParameterCount();
               }
               else if (exp.Right.IsLiteral())
                   flags |= ElaVariableFlags.ObjectLiteral;
               
               AddVariable(exp.Left.GetName(), exp, flags, data);
            }
            else
                AddPatternVariables(exp.Left);

            return true;
        }

        //Adding all variables from pattern as NoInit's. This method recursively walks 
        //all patterns. Currently we don't associate any additional metadata or flags 
        //(except of NoInit) with variables inferred in such a way.
        private void AddPatternVariables(ElaExpression pat)
        {
            switch (pat.Type)
            {
                case ElaNodeType.LazyLiteral:
                    {
                        //Not all forms of lazy patterns are supported,
                        //but it is easier to process them anyways. Errors will be caught
                        //during pattern compilation.
                        var vp = (ElaLazyLiteral)pat;
                        AddPatternVariables(vp.Expression);
                    }
                    break;
                case ElaNodeType.UnitLiteral: //Idle
                    break;
                case ElaNodeType.As:
                    {
                        var asPat = (ElaAs)pat;
                        AddVariable(asPat.Name, asPat, ElaVariableFlags.NoInit, -1);
                        AddPatternVariables(asPat.Expression);
                    }
                    break;
                case ElaNodeType.Primitive: //Idle
                    break;
                case ElaNodeType.NameReference:
                    {
                        var vexp = (ElaNameReference)pat;
                        
                        if (!vexp.Uppercase) //Uppercase is constructor
                            AddVariable(vexp.Name, vexp, ElaVariableFlags.NoInit, -1);
                    }
                    break;
                case ElaNodeType.RecordLiteral:
                    {
                        var rexp = (ElaRecordLiteral)pat;

                        foreach (var e in rexp.Fields)
                            if (e.FieldValue != null)
                                AddPatternVariables(e.FieldValue);
                    }
                    break;
                case ElaNodeType.TupleLiteral:
                    {
                        var texp = (ElaTupleLiteral)pat;

                        foreach (var e in texp.Parameters)
                            AddPatternVariables(e);
                    }
                    break;
                case ElaNodeType.Placeholder: //Idle
                    break;
                case ElaNodeType.Juxtaposition:
                    {
                        var hexp = (ElaJuxtaposition)pat;

                        foreach (var e in hexp.Parameters)
                            AddPatternVariables(e);
                    }
                    break;
                case ElaNodeType.ListLiteral: //Idle
                    {
                        var l = (ElaListLiteral)pat;

                        if (l.HasValues())
                        {
                            foreach (var e in l.Values)
                                AddPatternVariables(e);
                        }
                    }
                    break;
            }
        }

        private void CompileLazyPattern(ElaEquation eq, LabelMap map)
        {
            var names = new List<String>();
            ExtractPatternNames(eq.Left, names);
            CompileLazyExpression(eq.Right, map, Hints.None);
            var sys = AddVariable();
            PopVar(sys);

            for (var i = 0; i < names.Count; i++)
            {
                var n = names[i];
                Label funSkipLabel;
                int address;
                LabelMap newMap;

                CompileFunctionProlog(null, 1, eq.Line, eq.Column, out funSkipLabel, out address, out newMap);

                var next = cw.DefineLabel();
                var exit = cw.DefineLabel();
                CompilePattern(1 | ((sys >> 8) << 8), eq.Left, next);
                cw.Emit(Op.Br, exit);
                cw.MarkLabel(next);
                cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
                cw.MarkLabel(exit);
                cw.Emit(Op.Nop);

                var sv = GetVariable(n, eq.Line, eq.Column);
                PushVar(sv);
                CompileFunctionEpilog(null, 1, address, funSkipLabel);
                cw.Emit(Op.Newlazy);

                ScopeVar var;
                if (CurrentScope.Locals.TryGetValue(n, out var))
                {
                    //If it doesn't have a NoInit flag we are not good
                    if ((var.Flags & ElaVariableFlags.NoInit) == ElaVariableFlags.NoInit)
                    {
                        PopVar(var.Address = 0 | var.Address << 8); //Aligning it to local scope
                        CurrentScope.RemoveFlags(n, ElaVariableFlags.NoInit);
                    }
                }
            }
        }
        
        //Checks whether a given expression can be compiled
        private bool CanCompileStrict(ElaExpression exp, List<String> locals)
        {
            if (exp == null)
                return true;

            switch (exp.Type)
            {
                case ElaNodeType.Binary:
                    {
                        var b = (ElaBinary)exp;
                        return CanCompileStrict(b.Left, locals) && CanCompileStrict(b.Right, locals);
                    }
                case ElaNodeType.Comprehension:
                    {
                        var b = (ElaComprehension)exp;
                        return CanCompileStrict(b.Generator, locals);
                    }
                case ElaNodeType.Condition:
                    {
                        var b = (ElaCondition)exp;
                        return CanCompileStrict(b.Condition, locals) && CanCompileStrict(b.True, locals) && CanCompileStrict(b.False, locals);
                    }
                case ElaNodeType.Context:
                    {
                        var c = (ElaContext)exp;
                        return CanCompileStrict(c.Expression, locals);
                    }
                case ElaNodeType.Equation:
                    return true;
                case ElaNodeType.FieldDeclaration:
                    {
                        var b = (ElaFieldDeclaration)exp;
                        return CanCompileStrict(b.FieldValue, locals);
                    }
                case ElaNodeType.FieldReference:
                    {
                        var b = (ElaFieldReference)exp;
                        return CanCompileStrict(b.TargetObject, locals);
                    }
                case ElaNodeType.Generator:
                    {
                        var g = (ElaGenerator)exp;
                        return CanCompileStrict(g.Target, locals) && CanCompileStrict(g.Guard, locals) && CanCompileStrict(g.Body, locals);
                    }
                case ElaNodeType.Juxtaposition:
                    {
                        //We are very conservative here. Basically we can't compile in a strict manner any application of a function
                        //defined in the current module (except of a partial application).
                        var g = (ElaJuxtaposition)exp;

                        if (g.Target.Type == ElaNodeType.NameReference)
                        {
                            var n = g.Target.GetName();

                            if (!IsLocal(locals, n))
                            {
                                var sv = GetVariable(n, CurrentScope, GetFlags.NoError, 0, 0);

                                if ((sv.Flags & ElaVariableFlags.External) != ElaVariableFlags.External &&
                                    (sv.Flags & ElaVariableFlags.TypeFun) != ElaVariableFlags.TypeFun &&
                                    (sv.Flags & ElaVariableFlags.Builtin) != ElaVariableFlags.Builtin &&
                                    (sv.Flags & ElaVariableFlags.Parameter) != ElaVariableFlags.Parameter)
                                {
                                    if ((sv.Flags & ElaVariableFlags.Function) != ElaVariableFlags.Function ||
                                        sv.Data <= g.Parameters.Count)
                                        return false;
                                }
                            }
                        }
                        else if (g.Target.Type == ElaNodeType.FieldReference)
                        {
                            var fr = (ElaFieldReference)g.Target;

                            if (fr.TargetObject.Type != ElaNodeType.NameReference)
                                return false;

                            var sv = GetVariable(fr.TargetObject.GetName(), CurrentScope, GetFlags.NoError, 0, 0);

                            if ((sv.Flags & ElaVariableFlags.Module) != ElaVariableFlags.Module)
                                return false;
                        }
                        else
                            return false;

                        foreach (var p in g.Parameters)
                            if (!CanCompileStrict(p, locals))
                                return false;

                        return true;
                    }
                case ElaNodeType.LetBinding:
                    {
                        var g = (ElaLetBinding)exp;

                        if (locals == null)
                            locals = new List<String>();

                        foreach (var e in g.Equations.Equations)
                        {
                            if (e.IsFunction())
                                locals.Add(e.GetFunctionName());
                            else
                                ExtractPatternNames(e, locals);
                        }

                        return CanCompileStrict(g.Expression, locals);
                    }
                case ElaNodeType.ListLiteral:
                    {
                        var b = (ElaListLiteral)exp;

                        if (!b.HasValues())
                            return true;

                        foreach (var v in b.Values)
                            if (!CanCompileStrict(v, locals))
                                return false;

                        return true;
                    }
                case ElaNodeType.Try:
                case ElaNodeType.Match:
                    {
                        var b = (ElaMatch)exp;

                        if (!CanCompileStrict(b.Expression, locals))
                            return false;

                        foreach (var e in b.Entries.Equations)
                            if (!CanCompileStrict(e.Right, locals))
                                return false;

                        return true;
                    }
                case ElaNodeType.NameReference:
                    {
                        var b = (ElaNameReference)exp;

                        if (!IsLocal(locals, b.Name))
                        {
                            var sv = GetVariable(b.Name, CurrentScope, GetFlags.NoError | GetFlags.Local, 0, 0);
                            return (sv.Flags & ElaVariableFlags.NoInit) != ElaVariableFlags.NoInit;
                        }

                        return true;
                    }
                case ElaNodeType.Raise:
                    {
                        var r = (ElaRaise)exp;
                        return CanCompileStrict(r.Expression, locals);
                    }
                case ElaNodeType.Range:
                    {
                        var r = (ElaRange)exp;
                        return CanCompileStrict(r.First, locals) && CanCompileStrict(r.Second, locals) && CanCompileStrict(r.Last, locals);
                    }
                case ElaNodeType.RecordLiteral:
                    {
                        var r = (ElaRecordLiteral)exp;

                        foreach (var f in r.Fields)
                            if (!CanCompileStrict(f.FieldValue, locals))
                                return false;

                        return true;
                    }
                case ElaNodeType.TupleLiteral:
                    {
                        var t = (ElaTupleLiteral)exp;

                        foreach (var v in t.Parameters)
                            if (!CanCompileStrict(v, locals))
                                return false;

                        return true;
                    }
                default:
                    return true;
            }
        }

        private bool IsRecursive(ElaEquation eq)
        {
            if (eq.Left.Type == ElaNodeType.NameReference && !((ElaNameReference)eq.Left).Uppercase)
            {
                var sv = GetVariable(eq.Left.GetName(), CurrentScope, GetFlags.NoError, 0, 0);
                return IsRecursive(eq.Right, sv.Address, null);
            }
            else
            {
                var names = new List<String>();
                ExtractPatternNames(eq.Left, names);
                var arr = new int[names.Count];

                for (var i = 0; i < names.Count; i++)
                    arr[i] = GetVariable(names[i], CurrentScope, GetFlags.NoError, 0, 0).Address;

                if (IsRecursive(eq.Right, -1, arr))
                    return true;

                return false;
            }
        }

        private bool IsRecursive(ElaExpression exp, int addr, int[] arr)
        {
            if (exp == null)
                return false;

            switch (exp.Type)
            {
                case ElaNodeType.Binary:
                    {
                        var b = (ElaBinary)exp;
                        return IsRecursive(b.Left, addr, arr) && IsRecursive(b.Right, addr, arr);
                    }
                case ElaNodeType.Comprehension:
                    {
                        var b = (ElaComprehension)exp;
                        return IsRecursive(b.Generator, addr, arr);
                    }
                case ElaNodeType.Condition:
                    {
                        var b = (ElaCondition)exp;
                        return IsRecursive(b.Condition, addr, arr) && IsRecursive(b.True, addr, arr) && IsRecursive(b.False, addr, arr);
                    }
                case ElaNodeType.Context:
                    {
                        var c = (ElaContext)exp;
                        return IsRecursive(c.Expression, addr, arr);
                    }
                case ElaNodeType.Equation:
                    return true;
                case ElaNodeType.FieldDeclaration:
                    {
                        var b = (ElaFieldDeclaration)exp;
                        return IsRecursive(b.FieldValue, addr, arr);
                    }
                case ElaNodeType.FieldReference:
                    {
                        var b = (ElaFieldReference)exp;
                        return IsRecursive(b.TargetObject, addr, arr);
                    }
                case ElaNodeType.Generator:
                    {
                        var g = (ElaGenerator)exp;
                        return IsRecursive(g.Target, addr, arr) && IsRecursive(g.Guard, addr, arr) && IsRecursive(g.Body, addr, arr);
                    }
                case ElaNodeType.Juxtaposition:
                    {
                        var g = (ElaJuxtaposition)exp;

                        if (IsRecursive(g.Target, addr, arr))
                            return true;

                        foreach (var p in g.Parameters)
                            if (IsRecursive(p, addr, arr))
                                return true;

                        return false;
                    }
                case ElaNodeType.LetBinding:
                    {
                        var g = (ElaLetBinding)exp;

                        foreach (var e in g.Equations.Equations)
                        {
                            if (!e.IsFunction() && IsRecursive(e.Right, addr, arr))
                                return true;
                        }

                        return IsRecursive(g.Expression, addr, arr);
                    }
                case ElaNodeType.ListLiteral:
                    {
                        var b = (ElaListLiteral)exp;

                        if (!b.HasValues())
                            return false;

                        foreach (var v in b.Values)
                            if (IsRecursive(v, addr, arr))
                                return true;

                        return false;
                    }
                case ElaNodeType.Try:
                case ElaNodeType.Match:
                    {
                        var b = (ElaMatch)exp;

                        if (IsRecursive(b.Expression, addr, arr))
                            return true;

                        foreach (var e in b.Entries.Equations)
                            if (IsRecursive(e.Right, addr, arr))
                                return true;

                        return false;
                    }
                case ElaNodeType.NameReference:
                    {
                        var b = (ElaNameReference)exp;
                        var sv = GetVariable(b.Name, CurrentScope, GetFlags.NoError | GetFlags.Local, 0, 0);
                            
                        if (arr == null)
                            return sv.Address == addr;

                        for (var i = 0; i < arr.Length; i++)
                            if (arr[i] == sv.Address)
                                return true;

                        return false;
                    }
                case ElaNodeType.Raise:
                    {
                        var r = (ElaRaise)exp;
                        return IsRecursive(r.Expression, addr, arr);
                    }
                case ElaNodeType.Range:
                    {
                        var r = (ElaRange)exp;
                        return IsRecursive(r.First, addr, arr) && IsRecursive(r.Second, addr, arr) && IsRecursive(r.Last, addr, arr);
                    }
                case ElaNodeType.RecordLiteral:
                    {
                        var r = (ElaRecordLiteral)exp;

                        foreach (var f in r.Fields)
                            if (IsRecursive(f.FieldValue, addr, arr))
                                return true;

                        return false;
                    }
                case ElaNodeType.TupleLiteral:
                    {
                        var t = (ElaTupleLiteral)exp;

                        foreach (var v in t.Parameters)
                            if (IsRecursive(v, addr, arr))
                                return true;

                        return false;
                    }
                default:
                    return false;
            }
        }
        
        private bool IsLocal(List<String> names, string n)
        {
            if (names == null)
                return false;

            for (var i = 0; i < names.Count; i++)
                if (names[i] == n)
                    return true;

            return false;
        }

        private void ExtractPatternNames(ElaExpression pat, List<String> names)
        {
            switch (pat.Type)
            {
                case ElaNodeType.LazyLiteral:
                    {
                        //Not all forms of lazy patterns are supported,
                        //but it is easier to process them anyways. Errors will be caught
                        //during pattern compilation.
                        var vp = (ElaLazyLiteral)pat;
                        ExtractPatternNames(vp.Expression, names);
                    }
                    break;
                case ElaNodeType.UnitLiteral: //Idle
                    break;
                case ElaNodeType.As:
                    {
                        var asPat = (ElaAs)pat;
                        names.Add(asPat.Name);
                        ExtractPatternNames(asPat.Expression, names);
                    }
                    break;
                case ElaNodeType.Primitive: //Idle
                    break;
                case ElaNodeType.NameReference:
                    {
                        var vexp = (ElaNameReference)pat;

                        if (!vexp.Uppercase) //Uppercase is constructor
                            names.Add(vexp.Name);
                    }
                    break;
                case ElaNodeType.RecordLiteral:
                    {
                        var rexp = (ElaRecordLiteral)pat;

                        foreach (var e in rexp.Fields)
                            if (e.FieldValue != null)
                                ExtractPatternNames(e.FieldValue, names);
                    }
                    break;
                case ElaNodeType.TupleLiteral:
                    {
                        var texp = (ElaTupleLiteral)pat;

                        foreach (var e in texp.Parameters)
                            ExtractPatternNames(e, names);
                    }
                    break;
                case ElaNodeType.Placeholder: //Idle
                    break;
                case ElaNodeType.Juxtaposition:
                    {
                        var hexp = (ElaJuxtaposition)pat;

                        foreach (var e in hexp.Parameters)
                            ExtractPatternNames(e, names);
                    }
                    break;
                case ElaNodeType.ListLiteral: //Idle
                    {
                        var l = (ElaListLiteral)pat;

                        if (l.HasValues())
                        {
                            foreach (var e in l.Values)
                                ExtractPatternNames(e, names);
                        }
                    }
                    break;
            }
        }
    }
}