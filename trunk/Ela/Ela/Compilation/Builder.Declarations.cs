using System;
using Ela.CodeModel;
using Ela.Runtime;

namespace Ela.Compilation
{
	internal sealed partial class Builder
    {
        //Processes a binding or a chain of bindings (done through 'et').
        private void WalkDeclarations(ElaBinding s, LabelMap map, Hints hints)
        {
            var inExp = s.In;

            //These bindings introduce a new lexical scope
            if (inExp != null)
                StartScope(false, s.In.Line, s.In.Column);

            //Loop through all bindings and do the forward declaration
            foreach (var b in s)
                AddNoInitVariable(b);

            //Compile
            foreach (var b in s)
                CompileDeclaration(b, map, hints);

            //Compile 'in' expression if present
            if (inExp != null)
            {
                CompileExpression(inExp, map, hints);
                EndScope(); //Closing a scope
            }
        }
        
        private void CompileDeclaration(ElaBinding s, LabelMap map, Hints hints)
        {
            //Check for some errors
            ValidateBinding(s);

            if (s.Pattern != null)
                CompileBindingPattern(s, map);
            else
            {
                var addr = GetNoInitVariable(s);

                //Compile expression and write it to a variable
                CompileExpression(s.InitExpression, map, Hints.None);
                AddLinePragma(s);
                PopVar(addr);    
            }
        }

        private void CompileBindingPattern(ElaBinding s, LabelMap map)
        {
            var tuple = default(ElaTupleLiteral);
            var sys = -1;

            //If both pattern and init are tuples provide this tuple to pattern compiler
            //explicitely - in such a case a tuple won't be actually created.
            if (s.InitExpression.Type == ElaNodeType.TupleLiteral && s.Pattern.Type == ElaNodeType.TuplePattern)
                tuple = (ElaTupleLiteral)s.InitExpression;
            else
            {
                //If InitExpression is not a tuple we need to actually compile it and deconstruct
                sys = AddVariable();
                CompileStrictOrLazy(s.InitExpression, map, Hints.None);
                AddLinePragma(s);
                PopVar(sys);
            }

            //Labels needed for pattern compilation
            var next = cw.DefineLabel();
            var exit = cw.DefineLabel();

            //Here we compile a pattern a generate a 'handling logic' that raises a MatchFailed exception
            CompilePattern(sys, tuple, s.Pattern, map, next, s.VariableFlags, Hints.None);
            cw.Emit(Op.Br, exit);
            cw.MarkLabel(next);
            cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
            cw.MarkLabel(exit);
            cw.Emit(Op.Nop);
        }

        //Validate correctness of a binding
        private void ValidateBinding(ElaBinding s)
        {
            //These errors are not critical and allow to continue compilation
            if (s.InitExpression.Type == ElaNodeType.Builtin && String.IsNullOrEmpty(s.VariableName))
                AddError(ElaCompilerError.InvalidBuiltinBinding, s);

            if ((s.VariableFlags & ElaVariableFlags.Private) == ElaVariableFlags.Private && CurrentScope != globalScope)
                AddError(ElaCompilerError.PrivateOnlyGlobal, s);
        }

        //Returns a variable from a local scope marked with NoInit flag
        //If such variable couldn't be found returns -1
        private int GetNoInitVariable(ElaBinding s)
        {
            ScopeVar var;
            
            if (CurrentScope.Locals.TryGetValue(s.VariableName, out var))
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
        private void AddNoInitVariable(ElaBinding exp)
        {
            //This binding is not defined by PM
            if (!String.IsNullOrEmpty(exp.VariableName))
            {
                var flags = exp.VariableFlags | ElaVariableFlags.NoInit;
                var data = -1;

                if (exp.InitExpression.Type == ElaNodeType.Builtin)
                {
                    //Adding required hints for a built-in
                    data = (Int32)((ElaBuiltin)exp.InitExpression).Kind;
                    flags |= ElaVariableFlags.Builtin;
                }
                else if (exp.InitExpression.Type == ElaNodeType.FunctionLiteral)
                {                
                    var fun = (ElaFunctionLiteral)exp.InitExpression;
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
                        data = fun.ParameterCount;
                }

                AddVariable(exp.VariableName, exp, flags, data);
            }
            else
                AddPatternVariables(exp.Pattern);
        }

        //Adding all variables from pattern as NoInit's. This method recursively walks 
        //all patterns. Currently we don't associate any additional metadata or flags 
        //(except of NoInit) with variables inferred in such a way.
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
    }
}