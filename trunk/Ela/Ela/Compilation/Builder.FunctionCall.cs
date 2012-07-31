using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part contains implementation of different function application techniques.
    internal sealed partial class Builder
    {
        //Compiling a regular function call.
        private ExprData CompileFunctionCall(ElaJuxtaposition v, LabelMap map, Hints hints)
        {
            var ed = ExprData.Empty;
            var bf = default(ElaNameReference);
            var sv = default(ScopeVar);

            if (TryOptimizeConstructor(v, map))
                return ed;

            if (v.Target.Type == ElaNodeType.NameReference)
            {
                bf = (ElaNameReference)v.Target;
                sv = GetVariable(bf.Name, bf.Line, bf.Column);

                //If the target is one of the built-in application function we need to transform this
                //to a regular function call, e.g. 'x |> f' is translated into 'f x' by manually creating
                //an appropriates AST node. This is done to simplify compilation - so that all optimization
                //of a regular function call would be applied to pipes as well.
                if ((sv.Flags & ElaVariableFlags.Builtin) == ElaVariableFlags.Builtin)
                {
                    var k = (ElaBuiltinKind)sv.Data;

                    if (k == ElaBuiltinKind.BackwardPipe && v.Parameters.Count == 2)
                    {
                        var fc = new ElaJuxtaposition { Target = v.Parameters[0] };
                        fc.SetLinePragma(v.Line, v.Column);
                        fc.Parameters.Add(v.Parameters[1]);
                        return CompileFunctionCall(fc, map, hints);
                    }
                    else if (k == ElaBuiltinKind.ForwardPipe && v.Parameters.Count == 2)
                    {
                        var fc = new ElaJuxtaposition { Target = v.Parameters[1] };
                        fc.SetLinePragma(v.Line, v.Column);
                        fc.Parameters.Add(v.Parameters[0]);
                        return CompileFunctionCall(fc, map, hints);
                    }
                }
            }

            var tail = (hints & Hints.Tail) == Hints.Tail;
            var safeHints = (hints & Hints.Lazy) == Hints.Lazy ? Hints.Lazy : Hints.None;
            var len = v.Parameters.Count;

            //Compile arguments to which a function is applied
            for (var i = 0; i < len; i++)
                CompileExpression(v.Parameters[len - i - 1], map, safeHints);

            //If this a tail call and we effectively call the same function we are currently in,
            //than do not emit an actual function call, just do a goto. (Tail recursion optimization).
            if (tail && map.FunctionName != null && map.FunctionName == v.GetName() &&
                map.FunctionParameters == len && map.FunctionScope == GetScope(map.FunctionName))
            {
                AddLinePragma(v);
                cw.Emit(Op.Br, map.FunStart);
                return ed;
            }

            if (bf != null)
            {
                //The target is one of built-in functions and therefore can be inlined for optimization.
                if ((sv.Flags & ElaVariableFlags.Builtin) == ElaVariableFlags.Builtin)
                {
                    var kind = (ElaBuiltinKind)sv.Data;
                    var pars = BuiltinParams(kind);

                    //We inline built-ins only when all arguments are provided
                    //If this is not the case a built-in is compiled into a function in-place
                    //and than called.
                    if (len != pars)
                    {
                        AddLinePragma(bf);
                        CompileBuiltin(kind, v.Target, map, bf.Name);

                        if (v.FlipParameters)
                            cw.Emit(Op.Flip);

                        for (var i = 0; i < len; i++)
                            cw.Emit(Op.Call);
                    }
                    else
                        CompileBuiltinInline(kind, v.Target, map, hints);

                    return ed;
                }
                else
                {
                    //Regular situation, just push a target name
                    AddLinePragma(v.Target);
                    PushVar(sv);

                    if ((sv.VariableFlags & ElaVariableFlags.Function) == ElaVariableFlags.Function)
                        ed = new ExprData(DataKind.FunParams, sv.Data);
                    else if ((sv.VariableFlags & ElaVariableFlags.ObjectLiteral) == ElaVariableFlags.ObjectLiteral)
                        ed = new ExprData(DataKind.VarType, (Int32)ElaVariableFlags.ObjectLiteral);
                }
            }
            else
                ed = CompileExpression(v.Target, map, Hints.None);

            //Why it comes from AST? Because parser do not save the difference between pre-, post- and infix applications.
            //However Ela does support left and right sections for operators - and for such cases an additional flag is used
            //to signal about a section.
            if (v.FlipParameters)
                cw.Emit(Op.Flip);

            //It means that we are trying to call "not a function". Ela is a dynamic language, still it's worth to generate
            //a warning in such a case.
            if (ed.Type == DataKind.VarType)
                AddWarning(ElaCompilerWarning.FunctionInvalidType, v.Target, FormatNode(v.Target));

            AddLinePragma(v);

            for (var i = 0; i < len; i++)
            {
                var last = i == v.Parameters.Count - 1;

                //Use a tail call if this function call is a tail expression and this function
                //is not marked with 'inline' attribute.
                if (last && tail && opt)
                    cw.Emit(Op.Callt);
                else
                    cw.Emit(Op.Call);
            }

            return ed;
        }

        //Here we check if a function application is actually a constructor application.
        //The latter case can be inlined. We support both direct reference and a qualified reference.
        //Only constructors without type constraints can be inlined.
        private bool TryOptimizeConstructor(ElaJuxtaposition juxta, LabelMap map)
        {
            if (juxta.Target.Type == ElaNodeType.NameReference)
            {
                var nr = (ElaNameReference)juxta.Target;

                if (nr.Uppercase || Format.IsSymbolic(nr.Name))
                {
                    var sv = GetGlobalVariable("$-" + nr.Name, GetFlags.NoError, 0, 0);

                    if (!sv.IsEmpty() && sv.Data == juxta.Parameters.Count)
                    {
                        CompileTupleParameters(juxta, juxta.Parameters, map);
                        PushVar(sv);
                        var sv2 = GetGlobalVariable("$--" + nr.Name, GetFlags.None, juxta.Line, juxta.Column);
                        PushVar(sv2);
                        cw.Emit(Op.Newtype);
                        return true;
                    }
                }
            }
            else if (juxta.Target.Type == ElaNodeType.FieldReference)
            {
                var fr = (ElaFieldReference)juxta.Target;

                if (fr.TargetObject.Type == ElaNodeType.NameReference)
                {
                    var prefix = fr.TargetObject.GetName();
                    CodeFrame _;
                    var sv = FindByPrefix(prefix, "$-" + fr.FieldName, out _);

                    if (!sv.IsEmpty() && sv.Data == juxta.Parameters.Count)
                    {
                        CompileTupleParameters(juxta, juxta.Parameters, map);
                        PushVar(sv);
                        var sv2 = FindByPrefix(prefix, "$--" + fr.FieldName, out _);
                        PushVar(sv2);
                        cw.Emit(Op.Newtype);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
