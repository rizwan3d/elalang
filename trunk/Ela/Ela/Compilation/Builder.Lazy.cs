using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part is responsible for compilation of lazy sections
    internal sealed partial class Builder
    {
        private ExprData CompileLazy(ElaLazyLiteral exp, LabelMap map, Hints hints)
        {
            var ed = default(ExprData);

            //Try to optimize lazy section for a case
            //when a function application is marked as lazy
            if (!TryOptimizeLazy(exp, map, hints))
            {
                //Regular lazy section compilation
                //Create a closure around section
                ed = CompileLazyExpression(exp.Expression, map, hints);
            }

            if ((hints & Hints.Left) == Hints.Left)
                AddValueNotUsed(exp);

            return ed;
        }

        //Compiles any given expression as lazy, can be used to automatically generate thunks
        //as well as to compile an explicit lazy literal.
        private ExprData CompileLazyExpression(ElaExpression exp, LabelMap map, Hints hints)
        {
            StartSection();
            StartScope(true, exp.Line, exp.Column);
            cw.StartFrame(1);
            var funSkipLabel = cw.DefineLabel();
            cw.Emit(Op.Br, funSkipLabel);
            var address = cw.Offset;
            var ed = CompileExpression(exp, map, hints);
            cw.Emit(Op.Ret);
            frame.Layouts.Add(new MemoryLayout(currentCounter, cw.FinishFrame(), address));
            EndSection();
            EndScope();
            cw.MarkLabel(funSkipLabel);
            cw.Emit(Op.PushI4, 1);
            cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
            cw.Emit(Op.Newlazy);
            return ed;
        }

        //This methods tries to optimize lazy section. It would only work when a lazy
        //section if a function application that result in saturation (no partial applications)
        //allowed. In such a case this method eliminates "double" function call (which would be
        //the result of a regular compilation logic). If this method fails than regular compilation
        //logic is used.
        private bool TryOptimizeLazy(ElaLazyLiteral lazy, LabelMap map, Hints hints)
        {
            var body = default(ElaExpression);

            //Only function application is accepted
            if ((body = lazy.Expression).Type != ElaNodeType.FunctionCall)
                return false;

            var funCall = (ElaFunctionCall)body;

            //If a target is not a variable we can't check what is actually called
            if (funCall.Target.Type != ElaNodeType.VariableReference)
                return false;

            var varRef = (ElaVariableReference)funCall.Target;
            var scopeVar = GetVariable(varRef.VariableName, varRef.Line, varRef.Column);
            var len = funCall.Parameters.Count;

            //If a target is not function we can't optimize it
            if ((scopeVar.VariableFlags & ElaVariableFlags.Function) != ElaVariableFlags.Function)
                return false;

            //Only saturation case is optimized
            if (scopeVar.Data != funCall.Parameters.Count)
                return false;

            for (var i = 0; i < len; i++)
                CompileExpression(funCall.Parameters[len - i - 1], map, Hints.None);

            var sl = len - 1;
            AddLinePragma(varRef);
            PushVar(scopeVar);

            //We partially apply function and create a new function
            for (var i = 0; i < sl; i++)
                cw.Emit(Op.Call);

            AddLinePragma(lazy);

            //LazyCall uses a function provided to create a thunk
            //and remembers last function arguments as ElaFunction.LastParameter
            cw.Emit(Op.LazyCall, len);
            return true;
        }
    }
}
