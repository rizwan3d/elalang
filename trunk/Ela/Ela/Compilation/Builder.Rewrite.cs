using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
    internal sealed partial class Builder
    {
        //Main compilation method that runs compilation in seven steps by rewriting binding order.
        private void CompileProgram(List<ElaExpression> exps, LabelMap map)
        {
            var list = ProcessSafeExpressions(exps, map);
            list = ProcessTypes(list, map);
            list = ProcessInstances(list, map);
            list = ProcessFunctions(list, map);
            list = ProcessBindings(list, map);
            list = ProcessPatternBindings(list, map);
            ProcessExpressions(list, map);
        }

        //Safe expressions are expressions that can be compiled first - these are type class declarations,
        //modules includes and built-ins (all of them don't reference any names but instead do a lot of bindings
        //that are used by the rest of the code). This method also declares all names from global bindings in
        //advance (so that top level can be mutualy recursive).
        private FastList<ElaExpression> ProcessSafeExpressions(List<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaExpression>(len);

            //We walk through all expressions and create a new list of expression that contains
            //only elements that are not compiled by this routine
            for (var i = 0; i < len; i++)
            {
                var e = exps[i];

                if (e.Type == ElaNodeType.Binding)
                {
                    var b = (ElaBinding)e;

                    //An invalid binding, we should throw this one as soon as possible
                    if (b.InitExpression == null)
                        AddError(ElaCompilerError.VariableDeclarationInitMissing, b);
                    else if (b.In == null)
                    {
                        //This error is not strictly neccessary, but I don't really want
                        //to have write some additional boilerplate to handle this right now
                        if (b.And != null)
                            AddError(ElaCompilerError.BindingsAndRestrictedGlobal, b);

                        //Forward declaration
                        AddNoInitVariable(b);

                        //This is a global binding that is initialized with a built-in. Or a 'safe'
                        //expression (no variable references). It is perfectly safe to compile it right away. 
                        if (b.InitExpression.Type == ElaNodeType.Builtin || b.Safe())
                            CompileDeclaration(b, map, Hints.Left);
                        else
                            list.Add(b);
                    }
                    else
                        list.Add(b);
                }
                else if (e.Type == ElaNodeType.TypeClass)
                {
                    //A type class is compiled right away
                    var v = (ElaTypeClass)e;
                    CompileClass(v, map, Hints.Left);
                }
                else if (e.Type == ElaNodeType.ModuleInclude)
                {
                    //Open module is compiled right away
                    var s = (ElaModuleInclude)e;
                    CompileModuleInclude(s, map, Hints.Left);
                }
                else
                    list.Add(e); //The rest will be compiled later
            }

            return list;
        }

        //The second step is to compile types - we need to compile them separately because type definitions
        //can reference other names (declared on a previous stage) and types themselves can be widely referenced
        //in the code (e.g. in instance, in regular expressions, etc.).
        private FastList<ElaExpression> ProcessTypes(FastList<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaExpression>(len);

            for (var i = 0; i < len; i++)
            {
                var e = exps[i];
                
                //The only node that we need to process here. It is always compiled here
                if (e.Type == ElaNodeType.Newtype)
                {
                    var t = (ElaNewtype)e;
                    CompileType(t, map, Hints.Left);
                }
                else
                    list.Add(e);
            }

            return list;
        }

        //The third step is to compile instances - this should be done after type (as soon as instances
        //reference types) and after the first step as well (as soon as instances reference type classes
        //and can reference any other local and non-local names). It is important however to compile
        //instances before any user code gets executed because they effectively mutate function tables.
        private FastList<ElaExpression> ProcessInstances(FastList<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaExpression>(len);

            for (var i = 0; i < len; i++)
            {
                var e = exps[i];
              
                //The only node that we need to process here. It is always compiled here
                if (e.Type == ElaNodeType.ClassInstance)
                {
                    var v = (ElaClassInstance)e;
                    CompileInstance(v, map, Hints.Left);
                }
                else
                    list.Add(e);
            }

            return list;
        }
            
        //The fourth step - now we can compile global user defined functions and lazy sections. This is
        //user code however it is not executed when bindings are done therefore we wouldn't need to enforce
        //laziness here. Bindings done through pattern matching are rejected on this stage.
        private FastList<ElaExpression> ProcessFunctions(FastList<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaExpression>(len);

            for (var i = 0; i < len; i++)
            {
                var e = exps[i];

                if (e.Type == ElaNodeType.Binding)
                {
                    var b = (ElaBinding)e;

                    //We need to ensure that this is a global binding that it is not defined by pattern matching
                    if (b.In == null && b.Pattern == null &&
                       (b.InitExpression.Type == ElaNodeType.FunctionLiteral || b.InitExpression.Type == ElaNodeType.LazyLiteral))
                    {
                        CompileDeclaration(b, map, Hints.Left);
                    }
                    else
                        list.Add(e);
                }
                else
                    list.Add(e);
            }

            return list;
        }

        //The fifth step is to compile the rest of global bindings except of bindings defined by pattern
        //matching. This is the first stage when laziness can be enforce - e.g. compiler would create thunks
        //when needed.
        private FastList<ElaExpression> ProcessBindings(FastList<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaExpression>(len);

            for (var i = 0; i < len; i++)
            {
                var e = exps[i];

                if (e.Type == ElaNodeType.Binding)
                {
                    var b = (ElaBinding)e;

                    //We still only process bindings without pattern matching and only global bindings
                    if (b.Pattern == null && b.In == null)
                        CompileDeclaration(b, map, Hints.Left);
                    else
                        list.Add(e);
                }
                else
                    list.Add(e);
            }

            return list;
        }

        //The sixth step is to compile global bindings defined by pattern matching - we do not enforce
        //thunks here and in some cases execution of such code may result in 'BottomReached' run-time error.
        private FastList<ElaExpression> ProcessPatternBindings(FastList<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaExpression>(len);

            for (var i = 0; i < len; i++)
            {
                var e = exps[i];

                if (e.Type == ElaNodeType.Binding)
                {
                    var b = (ElaBinding)e;

                    //Only globals are taken
                    if (b.In == null)
                        CompileDeclaration(b, map, Hints.Left);
                    else
                        list.Add(e);
                }
                else
                    list.Add(e);
            }

            return list;
        }
        
        //The seventh step is to compile let/in (local bindings) and the rest of expressions - we do not enforce
        //thunks here and in some cases execution of such code may result in 'BottomReached' run-time error.
        private void ProcessExpressions(FastList<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            
            for (var i = 0; i < len; i++)
            {
                var e = exps[i];
                var hints = i == len - 1 ? Hints.None : Hints.Left;

                //Compile everything that is left
                CompileExpression(e, map, hints);
            }

            //It may happens that nothing is left on this stage however Ela program have to return
            //something. Therefore just return unit.
            if (len == 0)
                cw.Emit(Op.Pushunit);
        }


        private bool TryStrict(ElaExpression exp)
        {
            if (exp == null)
                return true;

            switch (exp.Type)
            {
                case ElaNodeType.Builtin:
                    return true;
                case ElaNodeType.Generator:
                    {
                        var e = (ElaGenerator)exp;
                        return TryStrict(e.Body) && TryStrict(e.Guard) && TryStrict(e.Target);
                    }
                case ElaNodeType.Range:
                    {
                        var e = (ElaRange)exp;
                        return TryStrict(e.First) && TryStrict(e.Last) && TryStrict(e.Second);
                    }
                case ElaNodeType.VariantLiteral:
                    return true;
                case ElaNodeType.LazyLiteral:
                    return true;
                case ElaNodeType.Try:
                    {
                        var e = (ElaTry)exp;

                        if (!TryStrict(e.Expression))
                            return false;

                        foreach (var en in e.Entries)
                            if (!TryStrict(en))
                                return false;

                        return true;
                    }
                case ElaNodeType.Comprehension:
                    {
                        var e = (ElaComprehension)exp;
                        return TryStrict(e.Generator);
                    }
                case ElaNodeType.Match:
                    {
                        var e = (ElaMatch)exp;

                        if (!TryStrict(e.Expression))
                            return false;

                        foreach (var en in e.Entries)
                            if (!TryStrict(en))
                                return false;

                        return true;
                    }
                case ElaNodeType.MatchEntry:
                    {
                        var e = (ElaMatchEntry)exp;
                        return TryStrict(e.Expression) && TryStrict(e.Guard);
                    }
                case ElaNodeType.FunctionLiteral:
                    return true;
                case ElaNodeType.Binding:
                    {
                        var e = (ElaBinding)exp;
                        return e.Pattern == null || TryStrict(e.InitExpression);
                    }
                case ElaNodeType.Condition:
                    {
                        var e = (ElaCondition)exp;
                        return TryStrict(e.Condition) && TryStrict(e.True) && TryStrict(e.False);
                    }
                case ElaNodeType.Raise:
                    {
                        var e = (ElaRaise)exp;
                        return TryStrict(e.Expression);
                    }
                case ElaNodeType.Binary:
                    {
                        var e = (ElaBinary)exp;
                        return TryStrict(e.Left) && TryStrict(e.Right);
                    }
                case ElaNodeType.Primitive:
                    return true;
                case ElaNodeType.RecordLiteral:
                    {
                        var e = (ElaRecordLiteral)exp;

                        foreach (var f in e.Fields)
                            if (!TryStrict(f.FieldValue))
                                return false;

                        return true;
                    }
                case ElaNodeType.FieldReference:
                    {
                        var e = (ElaFieldReference)exp;
                        return TryStrict(e.TargetObject);
                    }
                case ElaNodeType.ListLiteral:
                    {
                        var e = (ElaListLiteral)exp;
                        
                        if (!e.HasValues())
                            return true;

                        foreach (var v in e.Values)
                            if (!TryStrict(v))
                                return false;

                        return true;
                    }
                case ElaNodeType.VariableReference:
                    {
                        var v = (ElaVariableReference)exp;
                        var sv = GetVariable(v.VariableName, CurrentScope, GetFlags.NoError, 0, 0);

                        //It is OK if a name is not initialized yet but is a function as long as we
                        //know that functions are initialized at an early stage
                        return (sv.Flags & ElaVariableFlags.NoInit) != ElaVariableFlags.NoInit ||
                            (sv.Flags & ElaVariableFlags.Function) == ElaVariableFlags.Function;
                    }
                case ElaNodeType.Placeholder:
                    return true;
                case ElaNodeType.FunctionCall:
                    {
                        var v = (ElaFunctionCall)exp;

                        if (v.Target.Type != ElaNodeType.VariableReference)
                            return false;

                        var sv = GetVariable(v.Target.GetName(), CurrentScope, GetFlags.NoError, 0, 0);

                        if ((sv.Flags & ElaVariableFlags.External) == ElaVariableFlags.External)
                        {
                            for (var i = 0; i < v.Parameters.Count; i++)
                                if (!TryStrict(v.Parameters[i]))
                                    return false;

                            return true;
                        }

                        if ((sv.Flags & ElaVariableFlags.Function) != ElaVariableFlags.Function || sv.Data <= 0)
                            return false;

                        return sv.Data < v.Parameters.Count;
                    }
                case ElaNodeType.Is:
                    {
                        var v = (ElaIs)exp;
                        return TryStrict(v.Expression);
                    }
                case ElaNodeType.UnitLiteral:
                    return true;
                case ElaNodeType.TupleLiteral:
                    {
                        var v = (ElaTupleLiteral)exp;

                        if (!v.HasParameters)
                            return true;

                        foreach (var e in v.Parameters)
                            if (!TryStrict(e))
                                return false;

                        return true;
                    }
                default:
                    return false;
            }
        }

        
        private ExprData CompileStrictOrLazy(ElaExpression exp, LabelMap map, Hints hints)
        {
            //if (!TryStrict(exp))
            //{
            //    //if ((var.VariableFlags & ElaVariableFlags.NoInit) == ElaVariableFlags.NoInit &&
            //    //    (var.VariableFlags & ElaVariableFlags.Function) != ElaVariableFlags.Function &&
            //    //    (var.VariableFlags & ElaVariableFlags.Builtin) != ElaVariableFlags.Builtin)
            //    {
            //        AddWarning(ElaCompilerWarning.BottomValue, exp.Line, exp.Column, FormatNode(exp));
            //        AddHint(ElaCompilerHint.UseThunk, exp.Line, exp.Column, FormatNode(exp));
            //    }
            //}
                
            return CompileExpression(exp, map, hints);
            //else
            //    return CompileLazyExpression(exp, map, hints);
        }
    }
}
