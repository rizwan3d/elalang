using System;
using Ela.CodeModel;
using Ela.Runtime;
using System.Collections.Generic;

namespace Ela.Compilation
{
    //This part contains compilation logic for pattern matching.
	internal sealed partial class Builder
	{
        //Used to compile a 'try' expression.
        private void CompileTryExpression(ElaTry n, LabelMap map, Hints hints)
        {
            var catchLab = cw.DefineLabel();
            var exitLab = cw.DefineLabel();
            
            //Generate a start of a 'try' section
            AddLinePragma(n);
            cw.Emit(Op.Start, catchLab);

            CompileExpression(n.Expression, map, Hints.None);

            //Leaving 'try' section
            cw.Emit(Op.Leave);
            cw.Emit(Op.Br, exitLab);
            cw.MarkLabel(catchLab);
            cw.Emit(Op.Leave);

            //Throw hint is to tell match compiler to generate a different typeId if 
            //all pattern fail - to rethrow an original error instead of generating a
            //new MatchFailed error.
            CompileSimpleMatch(n.Entries.Equations, map, hints | Hints.Throw);

            cw.MarkLabel(exitLab);
            cw.Emit(Op.Nop);

            if ((hints & Hints.Left) == Hints.Left)
                AddValueNotUsed(n);
        }

        //Used to compile a 'match' expression.
        private void CompileMatchExpression(ElaMatch n, LabelMap map, Hints hints)
        {
            CompileExpression(n.Expression, map, Hints.None);
            AddLinePragma(n);
            CompileSimpleMatch(n.Entries.Equations, map, hints);

            if ((hints & Hints.Left) == Hints.Left)
                AddValueNotUsed(n);
        }

        //This method contains main compilation logic for 'match' expressions and for 'try' expressions. 
        //This method only supports a single pattern per entry.
        private void CompileSimpleMatch(IEnumerable<ElaEquation> bindings, LabelMap map, Hints hints)
        {
            var failLab = cw.DefineLabel();
            var endLab = cw.DefineLabel();

            //We need to remembers the first set of system addresses because we will have
            //to push them manually for the entries other than first.
            var firstSys = -1;
            var first = true;

            //Loops through all bindings
            //For the first iteration we assume that all values are already on stack.
            //For the next iteration we manually push all values.
            foreach (var b in bindings)
            {
                //Each match entry starts a lexical scope
                StartScope(false, b.Line, b.Column);

                //For second+ entries we put a label where we would jump if a previous 
                //pattern fails
                if (!first)
                {
                    cw.MarkLabel(failLab);
                    failLab = cw.DefineLabel();
                }

                var p = b.Left;
                var sysVar = -1;

                //We apply an optimization if a we have a name reference (only for the first iteration).
                if (p.Type == ElaNodeType.NameReference && first)
                    sysVar = AddVariable(p.GetName(), p, ElaVariableFlags.Parameter, -1);
                else
                    sysVar = AddVariable(); //Create an unnamed system variable

                //This is not the first entry, so we have to push values first
                if (!first)
                    PushVar(firstSys);

                PopVar(sysVar);

                //We have to remember addresses calculated in a first iteration
                //to be able to push them once again in a second iteration.
                if (first)
                    firstSys = sysVar;

                CompilePattern(sysVar, p, failLab);
                first = false;

                //Compile entry expression
                if (b.Right == null)
                    AddError(ElaCompilerError.InvalidMatchEntry, b.Left, FormatNode(b));
                else
                    CompileExpression(b.Right, map, Hints.None);

                //Close current scope
                EndScope();

                //If everything is OK skip through 'fail' clause
                cw.Emit(Op.Br, endLab);
            }

            //We fall here if all patterns have failed
            cw.MarkLabel(failLab);

            //If this hints is set than we generate a match for a 'try'
            //(exception handling) block. Instead of MatchFailed we need
            //to rethrow an original exception. Block 'try' always have
            //just a single expression to match, therefore firstSys[0] is
            //pretty safe here.
            if ((hints & Hints.Throw) == Hints.Throw)
            {
                PushVar(firstSys);
                cw.Emit(Op.Rethrow);
            }
            else
                cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);

            //Happy path for match
            cw.MarkLabel(endLab);
            cw.Emit(Op.Nop);
        }

        //Pattern match compilation method which is used by functions defined by pattern matching. 
        //Argument patNum contains number of patterns that should be in each.
        private void CompileFunctionMatch(int patNum, IEnumerable<ElaEquation> bindings, LabelMap map, Hints hints)
        {
            var failLab = cw.DefineLabel();
            var endLab = cw.DefineLabel();
            
            //We need to remembers the first set of system addresses because we will have
            //to push them manually for the entries other than first.
            var firstSys = new int[patNum];
            var first = true;

            //Loops through all bindings
            //For the first iteration we assume that all values are already on stack.
            //For the next iteration we manually push all values.
            foreach (var b in bindings)
            {
                //Each match entry starts a lexical scope
                StartScope(false, b.Line, b.Column);

                //This match compilation is used for functions only,
                //therefore the left-hand should always be a juxtaposition.
                var fc = (ElaJuxtaposition)b.Left;
                var len = fc.Parameters.Count;
                var pars = fc.Parameters;

                //Entries have different number of patterns, so generate an error and continue
                //compilation in order to prevent generation of 'redundant' error messages.
                if (len < patNum)
                    AddError(ElaCompilerError.PatternsTooFew, b.Left, FormatNode(b.Left), patNum, len);
                else if (len > patNum)
                    AddError(ElaCompilerError.PatternsTooMany, b.Left, FormatNode(b.Left), patNum, len);

                //For second+ entries we put a label where we would jump if a previous 
                //pattern fails
                if (!first)
                {
                    cw.MarkLabel(failLab);
                    failLab = cw.DefineLabel();
                }

                for (var i = 0; i < len; i++)
                {
                    var p = pars[i];
                    var sysVar = -1;

                    //We apply an optimization if a we have a name reference (only for the first iteration).
                    if (p.Type == ElaNodeType.NameReference && first)
                        sysVar = AddVariable(p.GetName(), p, ElaVariableFlags.Parameter, -1);
                    else
                        sysVar = AddVariable(); //Create an unnamed system variable

                    //This is not the first entry, so we have to push values first
                    if (!first && firstSys.Length > i)
                        PushVar(firstSys[i]);

                    PopVar(sysVar);

                    //We have to remember addresses calculated in a first iteration
                    //to be able to push them once again in a second iteration.
                    if (first && firstSys.Length > i)
                        firstSys[i] = sysVar;

                    
                }

                //Now compile patterns
                for (var i = 0; i < len; i++)
                    if (firstSys.Length > i && pars.Count > i)
                        CompilePattern(firstSys[i], pars[i], failLab);
                
                first = false;

                //Compile entry expression
                if (b.Right == null)
                    AddError(ElaCompilerError.InvalidMatchEntry, b.Left, FormatNode(b));
                else
                    CompileExpression(b.Right, map, hints);

                //Close current scope
                EndScope();
                
                //If everything is OK skip through 'fail' clause
                cw.Emit(Op.Br, endLab);
            }

            //We fall here if all patterns have failed
            cw.MarkLabel(failLab);
            cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);

            //Happy path for match
            cw.MarkLabel(endLab);
            cw.Emit(Op.Nop);
        }

        //Compile a given expression as a pattern. If match fails proceed to failLab.
        private void CompilePattern(int sysVar, ElaExpression exp, Label failLab)
        {
            AddLinePragma(exp);

            switch (exp.Type)
            {
                case ElaNodeType.LazyLiteral:
                    {
                        var n = (ElaLazyLiteral)exp;

                        //Currently we support only lazy tuple patterns
                        if (n.Expression.Type == ElaNodeType.TupleLiteral)
                            CompileLazyTuplePattern(sysVar, (ElaTupleLiteral)n.Expression, failLab);
                        else
                            AddError(ElaCompilerError.InvalidLazyPattern, exp, FormatNode(exp));
                    }
                    break;
                case ElaNodeType.FieldReference:
                    {
                        //We treat this expression as a constructor with a module alias
                        var n = (ElaFieldReference)exp;
                        var fn = n.FieldName;
                        var alias = n.TargetObject.GetName();
                        PushVar(sysVar);

                        if (n.TargetObject.Type != ElaNodeType.NameReference)
                            AddError(ElaCompilerError.InvalidPattern, n, FormatNode(n));
                        else
                            EmitSpecName(alias, "$$$$" + fn, n, ElaCompilerError.UndefinedName);

                        cw.Emit(Op.Skiptag);
                        cw.Emit(Op.Br, failLab);
                    }
                    break;
                case ElaNodeType.NameReference:
                    {
                        //Irrefutable pattern, always binds expression to a name, unless it is 
                        //a constructor pattern
                        var n = (ElaNameReference)exp;

                        if (n.Uppercase) //This is a constructor
                        {
                            if (sysVar != -1)
                                PushVar(sysVar); 
                            
                            EmitSpecName(null, "$$$$" + n.Name, n, ElaCompilerError.UndefinedName);
            
                            //This op codes skips one offset if an expression
                            //on the top of the stack has a specified tag.
                            cw.Emit(Op.Skiptag);
                            cw.Emit(Op.Br, failLab);
                        }
                        else
                        {
                            var newV = false;
                            var addr = AddMatchVariable(n.Name, n, out newV);

                            //This is a valid situation, it means that the value is
                            //already on the top of the stack.
                            if (sysVar > -1 && newV)
                                PushVar(sysVar);

                            //The binding is already done, so just idle.
                            if (newV)
                                PopVar(addr);
                        }
                    }
                    break;
                case ElaNodeType.UnitLiteral:
                    {
                        //Unit pattern is redundant, it is essentially the same as checking
                        //the type of an expression which is what we do here.
                        PushVar(sysVar);
                        cw.Emit(Op.PushI4, (Int32)ElaTypeCode.Unit);
                        cw.Emit(Op.Ctypei);

                        //Types are not equal, proceed to fail.
                        cw.Emit(Op.Brfalse, failLab);
                    }
                    break;
                case ElaNodeType.Primitive:
                    {
                        var n = (ElaPrimitive)exp;

                        //Compare a given value with a primitive
                        PushVar(sysVar);
                        PushPrimitive(n.Value);
                        cw.Emit(Op.Cneq);

                        //Values not equal, proceed to fail.
                        cw.Emit(Op.Brtrue, failLab);
                    }
                    break;
                case ElaNodeType.As:
                    {
                        var n = (ElaAs)exp;
                        CompilePattern(sysVar, n.Expression, failLab);
                        var newV = false;
                        var addr = AddMatchVariable(n.Name, n, out newV);
                        PushVar(sysVar);
                        PopVar(addr);
                    }
                    break;
                case ElaNodeType.Placeholder:
                    //This is a valid situation, it means that the value is
                    //already on the top of the stack. Otherwise - nothing have to be done.
                    if (sysVar == -1)
                        cw.Emit(Op.Pop);
                    break;
                case ElaNodeType.RecordLiteral:
                    {
                        var n = (ElaRecordLiteral)exp;
                        CompileRecordPattern(sysVar, n, failLab);
                    }
                    break;
                case ElaNodeType.TupleLiteral:
                    {
                        var n = (ElaTupleLiteral)exp;
                        CompileTuplePattern(sysVar, n, failLab);
                    }
                    break;
                case ElaNodeType.Juxtaposition:
                    {
                        //An infix pattern, currently the only case is head/tail pattern.
                        var n = (ElaJuxtaposition)exp;
                        CompileComplexPattern(sysVar, n, failLab);
                    }
                    break;                
                case ElaNodeType.ListLiteral:
                    {
                        var n = (ElaListLiteral)exp;

                        //We a have a nil pattern '[]'
                        if (!n.HasValues())
                        {
                            PushVar(sysVar);
                            cw.Emit(Op.Isnil);
                            cw.Emit(Op.Brfalse, failLab);
                        }
                        else
                        {
                            //We don't want to write the same compilation logic twice,
                            //so here we transform a list literal into a chain of function calls, e.g.
                            //[1,2,3] goes to 1::2::3::[] with a mandatory nil at the end.
                            var len = n.Values.Count;
                            ElaExpression last = ElaListLiteral.Empty;
                            var fc = default(ElaJuxtaposition);

                            //Loops through all elements in literal backwards
                            for (var i = 0; i < len; i++)
                            {
                                var nn = n.Values[len - i - 1];
                                fc = new ElaJuxtaposition();
                                fc.SetLinePragma(nn.Line, nn.Column);
                                fc.Parameters.Add(nn);
                                fc.Parameters.Add(last);
                                last = fc;
                            }

                            //Now we can compile it as head/tail pattern
                            CompilePattern(sysVar, fc, failLab);
                        }
                    }
                    break;
                default:
                    AddError(ElaCompilerError.InvalidPattern, exp, FormatNode(exp));
                    break;
            }
        }
        
        //Compile a record pattern in the form: {fieldName=pat,..}. Here we don't check the
        //type of an expression on the top of the stack - in a case if try to match a non-record
        //using this pattern the whole match would fail on Pushfld operation.
        private void CompileRecordPattern(int sysVar, ElaRecordLiteral rec, Label failLab)
        {
            //Loops through all record fields
            for (var i = 0; i < rec.Fields.Count; i++)
            {
                var fld = rec.Fields[i];
                var addr = AddVariable();

                cw.Emit(Op.Pushstr, AddString(fld.FieldName));
                PushVar(sysVar);
                cw.Emit(Op.Pushelem);
                PopVar(addr);

                //We obtain a value of field, now we need to match it using a pattern in
                //a field value (it could be a name reference or a non-irrefutable pattern).
                CompilePattern(addr, fld.FieldValue, failLab);
            }
        }

        //Compile a tuple pattern in the form: {fieldName=pat,..}. This pattern can fail at
        //run-time if a given expression doesn't support Len and Pushelem op codes.
        private void CompileTuplePattern(int sysVar, ElaTupleLiteral tuple, Label failLab)
        {
            var len = tuple.Parameters.Count;
            
            //Check the length first
            PushVar(sysVar);
            cw.Emit(Op.Len);            
            cw.Emit(Op.PushI4, len);
            cw.Emit(Op.Cneq);
            cw.Emit(Op.Brtrue, failLab); //Length not equal, proceed to fail

            //Loops through all tuple patterns
            for (var i = 0; i < len; i++)
            {
                var pat = tuple.Parameters[i];

                //Generate a 'short' op typeId for the first entry
                if (i == 0)
                    cw.Emit(Op.PushI4_0);
                else
                    cw.Emit(Op.PushI4, i);

                PushVar(sysVar);
                cw.Emit(Op.Pushelem);

                //Here we need to bind a value of an element to a new system
                //variable in order to match it.
                var sysVar2 = AddVariable();
                PopVar(sysVar2);
                
                //Match an element of a tuple
                CompilePattern(sysVar2, pat, failLab);
            }
        }

        //Currently this method only compiles head/tail pattern which is processed by parser
        //as function application. However it can be extended to support custom 'infix' patterns in future.
        private void CompileComplexPattern(int sysVar, ElaJuxtaposition call, Label failLab)
        {
            if (call.Target == null)
                CompileHeadTail(sysVar, call, failLab);
            else if (call.Target.Type == ElaNodeType.NameReference)
            {
                var targetName = call.Target.GetName();
                var sv = GetVariable(call.Target.GetName(), CurrentScope, GetFlags.NoError, call.Target.Line, call.Target.Column);

                //The head symbol corresponds to a constructor, this is a special case of pattern
                if ((sv.VariableFlags & ElaVariableFlags.Builtin) == ElaVariableFlags.Builtin && (ElaBuiltinKind)sv.Data == ElaBuiltinKind.Cons)
                    CompileHeadTail(sysVar, call, failLab);
                else
                    CompileConstructorPattern(sysVar, call, failLab);
            }
            else if (call.Target.Type == ElaNodeType.FieldReference)
                CompileConstructorPattern(sysVar, call, failLab);
            else
            {
                //We don't yet support other cases
                AddError(ElaCompilerError.InvalidPattern, call.Target, FormatNode(call.Target));
                return;
            }
        }

        //A generic case of constructor pattern
        private void CompileConstructorPattern(int sysVar, ElaJuxtaposition call, Label failLab)
        {
            var n = String.Empty;
            PushVar(sysVar);
            
            //We have a qualified name here, in such case we don't just check
            //the presence of a constructor but ensure that this constructor originates
            //from a given module
            if (call.Target.Type == ElaNodeType.FieldReference)
            {
                var fr = (ElaFieldReference)call.Target;
                n = fr.FieldName;
                var alias = fr.TargetObject.GetName();

                if (fr.TargetObject.Type != ElaNodeType.NameReference)
                    AddError(ElaCompilerError.InvalidPattern, fr, FormatNode(fr));
                else
                    EmitSpecName(alias, "$$$$" + n, fr, ElaCompilerError.UndefinedName);
            }
            else
            {
                //Here we simply check that a constructor symbol is defined
                n = call.Target.GetName();
                EmitSpecName(null, "$$$$" + n, call.Target, ElaCompilerError.UndefinedName);
            }

            //This op codes skips one offset if an expression
            //on the top of the stack has a specified tag.
            cw.Emit(Op.Skiptag);
            cw.Emit(Op.Br, failLab); //We will skip this if tags are equal

            for (var i = 0; i < call.Parameters.Count; i++)
            {
                PushVar(sysVar);
                cw.Emit(Op.Untag, i); //Unwrap it
                                
                //Now we need to create a new system variable to hold
                //an unwrapped value.
                var sysVar2 = -1;
                var p = call.Parameters[i];
                
                //Don't do redundant bindings for simple patterns
                if (!IsSimplePattern(p))
                {
                    sysVar2 = AddVariable();
                    PopVar(sysVar2);
                }

                CompilePattern(sysVar2, p, failLab);
            }
        }

        //Compiles a special case of constructor pattern - head/tail pattern.
        private void CompileHeadTail(int sysVar, ElaJuxtaposition call, Label failLab)
        {
            var fst = call.Parameters[0];
            var snd = call.Parameters[1];

            //Now check if a list is nil. If this is the case proceed to fail.
            PushVar(sysVar);
            cw.Emit(Op.Isnil);
            cw.Emit(Op.Brtrue, failLab);

            //Take a head of a list
            PushVar(sysVar);
            cw.Emit(Op.Head);
            var sysVar2 = -1;

            //For a case of a simple pattern we don't need to create to additional system
            //variable - these patterns are aware that they might accept -1 and it means that
            //the value is already on the top of the stack.
            if (!IsSimplePattern(fst))
            {
                sysVar2 = AddVariable();
                PopVar(sysVar2);
            }

            CompilePattern(sysVar2, fst, failLab);

            //Take a tail of a list
            PushVar(sysVar);
            cw.Emit(Op.Tail);
            sysVar2 = -1;

            //Again, don't do redundant bindings for simple patterns
            if (!IsSimplePattern(snd))
            {
                sysVar2 = AddVariable();
                PopVar(sysVar2);
            }

            CompilePattern(sysVar2, snd, failLab);
        }

        //With a lazy tuple pattern an equation like so: '(x,y) = exp' is semantically transformed 
        //into '(x,y) = (&fst exp,&snd exp)', but as long as we now have two tuples of the same
        //length on both sides there is no need to create any of them - so we just assign variables
        //with generated thunks.
        private void CompileLazyTuplePattern(int sysVar, ElaTupleLiteral tuple, Label failLab)
        {
            var len = tuple.Parameters.Count;

            for (var i = 0; i < len; i++)
            {
                var e = tuple.Parameters[i];

                //Compile the first part of the thunk function
                Label funSkipLabel;
                int address;
                LabelMap newMap;
                CompileFunctionProlog(null, 1, e.Line, e.Column, out funSkipLabel, out address, out newMap);

                //Here we compile a thunk body that obtains a tuple element
                //Here we have to manually patch an address because we refer
                //to a captured variable that doesn't belong to this function scope.
                var a = (sysVar & Byte.MaxValue) + 1 | (sysVar << 8) >> 8;
                PushVar(a);
                //Check for length in order to generate "nice" error instead of IndexOutOfRange.
                //We have to do for each element because they may be evaluated independently.
                cw.Emit(Op.Len);
                cw.Emit(Op.PushI4, len);
                cw.Emit(Op.Cneq);
                cw.Emit(Op.Brtrue, failLab);
                //Obtain an element
                if (i == 0) cw.Emit(Op.PushI4_0); else cw.Emit(Op.PushI4, i);
                PushVar(a);
                cw.Emit(Op.Pushelem);

                //Compile thunk epilog, create a new thunk object
                CompileFunctionEpilog(null, 1, address, funSkipLabel);
                cw.Emit(Op.Newlazy);

                //Move on to the nested pattern
                var newSys = AddVariable();
                PopVar(newSys);
                CompilePattern(newSys, e, failLab);
            }
        }

        //Tests if a given expression is a name reference of a placeholder (_).
        private bool IsSimplePattern(ElaExpression exp)
        {
            return exp.Type == ElaNodeType.NameReference || exp.Type == ElaNodeType.Placeholder;
        }

        //Adds a provided variable to the current scope or modifies an existing
        //variable with the same name (this is required because match variables unlike
        //regular variables in the same scope can shadow each other).
		private int AddMatchVariable(string varName, ElaExpression exp, out bool newV)
		{
            var sv = ScopeVar.Empty;
            newV = false;

            //Here we first check if such a name is already added and if this is the case simply fetch an
            //existing name.
            if (CurrentScope.Parent != null && CurrentScope.Parent.Locals.TryGetValue(varName, out sv)
                && (sv.Flags & ElaVariableFlags.Parameter) == ElaVariableFlags.Parameter)
                return 0 | sv.Address << 8;
            else
            {
                var res = CurrentScope.TryChangeVariable(varName);
                newV = true;

                if (res != -1)
                    return 0 | res << 8;

                return AddVariable(varName, exp, ElaVariableFlags.None, -1);
            }
		}
	}
}