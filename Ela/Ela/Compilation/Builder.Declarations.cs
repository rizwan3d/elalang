using System;
using Ela.CodeModel;
using Ela.Runtime;

namespace Ela.Compilation
{
	internal sealed partial class Builder
    {
        //Processes a binding or a chain of bindings.
        private void WalkDeclarations(ElaEquationSet s, LabelMap map, Hints hints)
        {
            //Do the forward declaration
            foreach (var e in s.Equations)
                AddNoInitVariable(e);

            //Compile
            foreach (var e in s.Equations)
                CompileDeclaration(e, map, hints);
        }
        
        //Compile all declarations including function bindings, name bindings and bindings
        //defined by pattern matching.
        private void CompileDeclaration(ElaEquation s, LabelMap map, Hints hints)
        {
            //Check for some errors
            ValidateBinding(s);
            var fun = s.IsFunction();

            if (s.Left.Type != ElaNodeType.NameReference && !fun)
                CompileBindingPattern(s, map);
            else
            {
                var addr = GetNoInitVariable(s);

                //Compile expression and write it to a variable
                if (fun)
                    CompileFunction(s, 
                        (s.VariableFlags & ElaVariableFlags.Inline) == ElaVariableFlags.Inline ? FunFlag.Inline : FunFlag.None);
                else
                {
                    CompileExpression(s.Right, map, Hints.None);

                    //If this a type member we need to construct a new type instance
                    if (s.AssociatedType != null)
                        cw.Emit(Op.Newtype, AddString(s.AssociatedType));
                }

                AddLinePragma(s);
                PopVar(addr);    
            }
        }

        private void CompileBindingPattern(ElaEquation s, LabelMap map)
        {
            var tuple = default(ElaTupleLiteral);
            var sys = -1;

            //If both pattern and init are tuples provide this tuple to pattern compiler
            //explicitely - in such a case a tuple won't be actually created.
            if (s.Right.Type == ElaNodeType.TupleLiteral && s.Left.Type == ElaNodeType.TupleLiteral)
                tuple = (ElaTupleLiteral)s.Right;
            else
            {
                //If InitExpression is not a tuple we need to actually compile it and deconstruct
                sys = AddVariable();
                CompileExpression(s.Right, map, Hints.None);
                AddLinePragma(s);
                PopVar(sys);
            }

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
        private int GetNoInitVariable(ElaEquation s)
        {
            ScopeVar var;
            var name = s.Left.GetName();

            if (s.IsFunction())
                name = s.GetFunctionName();
            
            if (CurrentScope.Locals.TryGetValue(name, out var))
            {
                //If it doesn't have a NoInit flag we are not good
                if ((var.Flags & ElaVariableFlags.NoInit) != ElaVariableFlags.NoInit)
                    return -1;
                else
                    return 0 | var.Address << 8; //Aligning it to local scope
            }

            return -1;
        }

        //Adds a variable with NoInit flag to the current scope
        //This method also calculates additional flags and metadata for variables.
        //If a given binding if defined by pattern matching then all variables from
        //patterns are traversed using AddPatternVariable method.
        private void AddNoInitVariable(ElaEquation exp)
        {
            var flags = exp.VariableFlags | ElaVariableFlags.NoInit;
            
            //This binding is not defined by PM
            if (exp.IsFunction())
            {
                var name = exp.GetFunctionName();
                AddVariable(name, exp.Left, flags | ElaVariableFlags.Function, -1);
            }
            else if (exp.Left.Type == ElaNodeType.NameReference)
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

                    //For an inline function logic is slightly different which is somewhat
                    //ugly at the moment. Instead of function arguments 'data' contains a
                    //unique ID of an inline function in 'inlineFuns' array.
                    if ((flags & ElaVariableFlags.Inline) == ElaVariableFlags.Inline)
                    {
                        data = inlineFuns.Count;
                        inlineFuns.Add(new InlineFun(fun, CurrentScope));
                    }
                    else
                        data = fun.GetParameterCount();
                }

                AddVariable(exp.Left.GetName(), exp, flags, data);
            }
            else
                AddPatternVariables(exp.Left);
        }

        //Adding all variables from pattern as NoInit's. This method recursively walks 
        //all patterns. Currently we don't associate any additional metadata or flags 
        //(except of NoInit) with variables inferred in such a way.
        private void AddPatternVariables(ElaExpression pat)
        {
            switch (pat.Type)
            {
                case ElaNodeType.VariantLiteral:
                    {
                        var vp = (ElaVariantLiteral)pat;

                        if (vp.Expression != null)
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
    }
}