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
            ProcessInstances(prog, map);
            list = ProcessFunctions(list, map);
            list = ProcessBindings(list, map);
            list = ProcessPatternBindings(list, map);
            ProcessExpressions(list, map);
        }

        //Compiles a provided set of equations
        private void CompileEquationSet(ElaEquationSet set, LabelMap map)
        {
            var list = RunForwardDeclaration(set.Equations, map);
            list = ProcessFunctions(list, map);
            list = ProcessBindings(list, map);
            list = ProcessPatternBindings(list, map);
            ProcessExpressions(list, map);
        }

        //Type class declarations, modules includes and type declarations can be compiled in the first place.
        private void ProcessIncludesClassesTypes(ElaProgram prog, LabelMap map)
        {
            if (prog.Includes != null)
                CompileModuleInclude(prog.Includes, map);
            
            if (prog.Classes != null)
                CompileClass(prog.Classes, map);

            if (prog.Types != null)
                CompileType(prog.Types, map);
        }

        //This method declares all names from global bindings in advance (so that top level can be mutualy recursive).
        //It also compiles built-ins (all of them don't reference any names but instead do a lot of bindings that are 
        //used by the rest of the code).
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

                    //Forward declaration
                    AddNoInitVariable(b);

                    //This is a global binding that is initialized with a built-in. Or a 'safe'
                    //expression (no variable references). It is perfectly safe to compile it right away. 
                    if (b.Right.Type == ElaNodeType.Builtin || b.Safe())
                        CompileDeclaration(b, map, Hints.Left);
                    else
                        list.Add(b);
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
        //instances before any user code gets executed because they effectively mutate function tables.
        private void ProcessInstances(ElaProgram prog, LabelMap map)
        {
            if (prog.Instances != null)
                CompileInstance(prog.Instances, map);
        }
            
        //Now we can compile global user defined functions and lazy sections. This is
        //user code however it is not executed when bindings are done therefore we wouldn't need to enforce
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
                    if (b.IsFunction() || b.Left.Type == ElaNodeType.LazyLiteral)
                        CompileDeclaration(b, map, Hints.None);
                    else
                        list.Add(b);
                }
                else
                    list.Add(b);
            }

            return list;
        }

        //This step is to compile the rest of global bindings except of bindings defined by pattern
        //matching. This is the first stage when laziness can be enforce - e.g. compiler would create thunks
        //when needed.
        private FastList<ElaEquation> ProcessBindings(FastList<ElaEquation> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaEquation>(len);

            for (var i = 0; i < len; i++)
            {
                var b = exps[i];

                //We still only process bindings without pattern matching and only global bindings
                if (b.Right != null && b.Left.Type == ElaNodeType.NameReference)
                    CompileDeclaration(b, map, Hints.Left);
                else
                    list.Add(b);
            }

            return list;
        }

        //This step is to compile global bindings defined by pattern matching - we do not enforce
        //thunks here and in some cases execution of such code may result in 'BottomReached' run-time error.
        private FastList<ElaEquation> ProcessPatternBindings(FastList<ElaEquation> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaEquation>(len);

            for (var i = 0; i < len; i++)
            {
                var b = exps[i];

                if (b.Right != null)
                {
                    //Type constructors cannot be defined by PM
                    if (b.AssociatedType != null)
                        AddError(ElaCompilerError.TypeMemberNoPatterns, b, FormatNode(b));

                    CompileDeclaration(b, map, Hints.Left);
                }
                else
                    list.Add(b);
            }

            return list;
        }
        
        //The last step is to compile let/in (local bindings) and the rest of expressions - we do not enforce
        //thunks here and in some cases execution of such code may result in 'BottomReached' run-time error.
        private void ProcessExpressions(FastList<ElaEquation> exps, LabelMap map)
        {
            var len = exps.Count;
            
            for (var i = 0; i < len; i++)
            {
                var e = exps[i];
                var hints = i == len - 1 ? Hints.None : Hints.Left;

                //Type constructors cannot be just expressions
                if (e.AssociatedType != null)
                    AddError(ElaCompilerError.TypeMemberNoPatterns, e.Left, FormatNode(e));

                //Compile everything that is left
                CompileExpression(e.Left, map, hints);
            }

            //It may happens that nothing is left on this stage however Ela program have to return
            //something. Therefore just return unit.
            if (len == 0)
                cw.Emit(Op.Pushunit);
        }
    }
}
