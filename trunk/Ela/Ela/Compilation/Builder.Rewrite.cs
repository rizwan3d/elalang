using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part is responsible for compilation of top level equations. It does rewrite order of equations as well.
    internal sealed partial class Builder
    {
        //Main compilation method that runs compilation in seven steps by rewriting binding order.
        private void CompileProgram(ElaProgram prog, LabelMap map)
        {
            ProcessIncludesClassesTypes(prog, map);
            var list = RunForwardDeclaration(prog.TopLevel.Equations, map);
            list = ProcessFunctions(list, map);
            ProcessInstances(prog, map);
            list = ProcessBindings(list, map);
            ProcessExpressions(list, map);
        }

        //Compiles a provided set of equations. This method is to be used for local
        //scopes only.
        private void CompileEquationSet(ElaEquationSet set, LabelMap map)
        {
            var list = RunForwardDeclaration(set.Equations, map);
            list = ProcessFunctions(list, map);
            list = ProcessBindings(list, map);

            //Expressions are not allowed in this context
            if (list.Count > 0)
                for (var i = 0; i < list.Count; i++)
                    AddError(ElaCompilerError.InvalidExpression, list[i], FormatNode(list[i]));
        }

        //Type class declarations, modules includes and type declarations can be compiled in the first place.
        private void ProcessIncludesClassesTypes(ElaProgram prog, LabelMap map)
        {
            CompileModuleIncludes(prog, map);
            
            if (prog.Classes != null)
                CompileClass(prog.Classes, map);

            if (prog.Types != null)
                CompileTypes(prog.Types, map);
        }

        //This method declares all names from global bindings in advance (so that top level can be mutualy recursive).
        //It also compiles built-ins (all of them don't reference any names but instead do a lot of bindings that are 
        //used by the rest of the typeId).
        private FastList<ElaEquation> RunForwardDeclaration(List<ElaEquation> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaEquation>(len);
            var head = default(ElaHeader);

            //We walk through all expressions and create a new list of expression that contains
            //only elements that are not compiled by this routine
            for (var i = 0; i < len; i++)
            {
                var b = exps[i];

                if (b.Right != null)
                {
                    //If a header is not null, we need to append some attributes to this binding
                    if (head != null)
                    {
                        //We add attributes if this binding is either a function or a name binding.
                        if (b.IsFunction() && b.GetFunctionName() == head.Name ||
                            b.Left.Type == ElaNodeType.NameReference && b.Left.GetName() == head.Name)
                            b.VariableFlags |= head.Attributes;
                        else
                            AddError(ElaCompilerError.HeaderNotConnected, head, FormatNode(head));

                        head = null;
                    }

                    if (AddNoInitVariable(b))
                    {
                        //This is a global binding that is initialized with a built-in. Or a 'safe'
                        //expression (no variable references). It is perfectly safe to compile it right away. 
                        if (b.Right.Type == ElaNodeType.Builtin || (!b.IsFunction() && b.Right.Safe()))
                            CompileDeclaration(b, map, Hints.Left);
                        else
                            list.Add(b);
                    }
                }
                else if (b.Left.Type == ElaNodeType.Header)
                {
                    //One header can't follow another another
                    if (head != null)
                        AddError(ElaCompilerError.HeaderNotConnected, head, FormatNode(head));

                    head = (ElaHeader)b.Left;
                }
                else
                {
                    //A header before an expression, that is not right
                    if (head != null)
                    {
                        AddError(ElaCompilerError.HeaderNotConnected, head, FormatNode(head));
                        head = null;
                    }

                    list.Add(b); //The rest will be compiled later
                }
            }

            return list;
        }

        //This step compiles instances - this should be done after types (as soon as instances
        //reference types) and after the first step as well (as soon as instances reference type classes
        //and can reference any other local and non-local names). It is important however to compile
        //instances before any user typeId gets executed because they effectively mutate function tables.
        private void ProcessInstances(ElaProgram prog, LabelMap map)
        {
            if (prog.Instances != null)
                CompileInstances(prog.Instances, map);
        }
            
        //Now we can compile global user defined functions and lazy sections. This is
        //user typeId however it is not executed when bindings are done therefore we wouldn't need to enforce
        //laziness here. TopLevel done through pattern matching are rejected on this stage.
        private FastList<ElaEquation> ProcessFunctions(FastList<ElaEquation> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaEquation>(len);

            for (var i = 0; i < len; i++)
            {
                var b = exps[i];

                if (b.Right != null)
                {
                    //We need to ensure that this is a global binding that it is not defined by pattern matching
                    if (b.IsFunction() || (b.Right.Type == ElaNodeType.LazyLiteral && 
                        (b.Left.Type == ElaNodeType.NameReference || b.Left.Type == ElaNodeType.LazyLiteral)))
                        CompileDeclaration(b, map, Hints.None);
                    else
                        list.Add(b);
                }
                else
                    list.Add(b);
            }

            return list;
        }

        //This step is to compile all global bindings, including regular bindings and bindings defined
        //by pattern matching.
        private FastList<ElaEquation> ProcessBindings(FastList<ElaEquation> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaEquation>(len);

            for (var i = 0; i < len; i++)
            {
                var b = exps[i];

                if (b.Right != null && b.Left.Type != ElaNodeType.Placeholder)
                    CompileDeclaration(b, map, Hints.Left);
                else
                    list.Add(b);
            }

            return list;
        }
        
        //The last step is to compile let/in (local bindings) and the rest of expressions - we do not enforce
        //thunks here and in some cases execution of such typeId may result in 'BottomReached' run-time error.
        private void ProcessExpressions(FastList<ElaEquation> exps, LabelMap map)
        {
            var len = exps.Count;
            var expCount = 0;
            
            for (var i = 0; i < len; i++)
            {
                var e = exps[i];

                //This is a binding in the form '_ = exp' which we consider to be
                //just a hanging expression "without a warning".
                if (e.Left != null && e.Right != null)
                {
                    CompileExpression(e.Right, map, Hints.None, e);
                    cw.Emit(Op.Pop);
                }
                else
                {
                    var hints = i == len - 1 ? Hints.None : Hints.Left;

                    //Compile everything that is left
                    CompileExpression(e.Left, map, hints, e);
                    expCount++;
                }
            }

            //It may happens that nothing is left on this stage, however, Ela program have to return
            //something. Therefore just return unit.
            if (expCount == 0)
                cw.Emit(Op.Pushunit);
        }

        //**********************************TEMPORARY DISABLED
        //This method checks if a given expression is a regular function definition or a function
        //defined through partial application of another function.
        private bool IsFunction(ElaEquation eq)
        {
            //A simple case - regular function definition
            if (eq.IsFunction())
                return true;

            //This may be a function defined through partial application. Here we only
            //recognize two cases - when the head is an ordinary identifier and if a head is
            //a fully qualified name.
            if (eq.Right != null && eq.Right.Type == ElaNodeType.Juxtaposition)
            {
                var jx = (ElaJuxtaposition)eq.Right;

                //A head is an identifier
                if (jx.Target.Type == ElaNodeType.NameReference)
                {
                    var sv = GetVariable(jx.Target.GetName(), CurrentScope, GetFlags.NoError, 0, 0);
                    
                    //This is a partially applied function, therefore we can "count" this as a function.
                    return IfFunction(eq, sv, jx.Parameters.Count, false);
                }
                else if (jx.Target.Type == ElaNodeType.FieldReference) //This might be a fully qualified name
                {
                    var tr = (ElaFieldReference)jx.Target;

                    //A target can only be a name reference; otherwise we don't see it as a function.
                    if (tr.TargetObject.Type == ElaNodeType.NameReference)
                    {
                        CodeFrame _;
                        var sv = FindByPrefix(tr.TargetObject.GetName(), tr.FieldName, out _);

                        return IfFunction(eq, sv, jx.Parameters.Count, false);
                    }
                }
            }
            else if (eq.Right != null && eq.Right.Type == ElaNodeType.NameReference)
            {
                //This may be a function defined as an alias for another function.
                var sv = GetVariable(eq.Right.GetName(), CurrentScope, GetFlags.NoError, 0, 0);

                //This is an alias, we can "count" this as a function.
                return IfFunction(eq, sv, 0, true);
            }

            return false;
        }

        //**********************************TEMPORARY DISABLED
        //Here we try to check if a right side is a function reference of a partially applied function.
        //This function might (or might not) set a 'PartiallyApplied' flag. If this flag is set, than a
        //function is eta expanded during compilation. Normally this flag is not needed when functions
        //are defined as simple aliases for other functions.
        private bool IfFunction(ElaEquation eq, ScopeVar sv, int curArgs, bool noPartial)
        {
            //This is a partially applied function, therefore we can "count" this as a function.
            if ((sv.Flags & ElaVariableFlags.Function) == ElaVariableFlags.Function && sv.Data > curArgs)
            {
                if (!noPartial)
                {
                    eq.VariableFlags |= ElaVariableFlags.PartiallyApplied;
                    eq.Arguments = sv.Data - curArgs;
                }

                return true;
            }
            else if ((sv.Flags & ElaVariableFlags.Builtin) == ElaVariableFlags.Builtin)
            {
                //If this a built-in, we need to use another method to determine number of arguments.
                var args = BuiltinParams((ElaBuiltinKind)sv.Data);

                if (args > curArgs)
                {
                    if (!noPartial)
                    {
                        eq.VariableFlags |= ElaVariableFlags.PartiallyApplied;
                        eq.Arguments = args - curArgs;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
