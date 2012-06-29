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

            //Throw hint is to tell match compiler to generate a different code if 
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
                    CompileExpression(b.Right, map, hints);

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
                case ElaNodeType.NameReference:
                    {
                        //Irrefutable pattern, always binds expression to a name
                        var n = (ElaNameReference)exp;
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
                    break;
                case ElaNodeType.VariantLiteral:
                    {
                        var n = (ElaVariantLiteral)exp;
                        PushVar(sysVar);

                        //This op codes skips one offset if an expression
                        //on the top of the stack has a specified tag.
                        cw.Emit(Op.Skiptag, AddString(n.Tag));
                        cw.Emit(Op.Br, failLab); //We will skip this if tags are equal

                        //A variant literal can only have a tag or can have a tagged
                        //expression. In the first case we check a tag only, we don't
                        //unwrap a variant, so any value (e.g. Tag () or Tag 42) will do.
                        if (n.Expression != null)
                        {
                            PushVar(sysVar);
                            cw.Emit(Op.Untag); //Unwrap a variant

                            //Now we need to create a new system variable to hold
                            //a unwrapped value.
                            var sysVar2 = AddVariable();
                            PopVar(sysVar2);
                            CompilePattern(sysVar2, n.Expression, failLab);
                        }
                    }
                    break;
                case ElaNodeType.UnitLiteral:
                    {
                        //Unit pattern is redundant, it is essentially the same as checking
                        //the type of an expression which is what we do here.
                        cw.Emit(Op.PushI4_0);
                        PushVar(sysVar);
                        cw.Emit(Op.Type);
                        cw.Emit(Op.Pushelem);
                        cw.Emit(Op.PushI4, (Int32)ElaTypeCode.Unit);
                        cw.Emit(Op.Cneq);

                        //Types are not equal, proceed to fail.
                        cw.Emit(Op.Brtrue, failLab);
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
                case ElaNodeType.TypeCheck:
                    {
                        var n = (ElaTypeCheck)exp;

                        //This pattern can either check a specific type or a collection of traits.
                        //Parser guarantees that these cases cannot overlap.
                        if (!String.IsNullOrEmpty(n.TypeName))
                        {
                            cw.Emit(Op.PushI4_0);
                            PushVar(sysVar);
                            cw.Emit(Op.Type);
                            cw.Emit(Op.Pushelem);
                            EmitSpecName(n.TypePrefix, "$$" + n.TypeName, n, ElaCompilerError.UndefinedType);
                            cw.Emit(Op.Cneq);
                            cw.Emit(Op.Brtrue, failLab);
                        }
                        else
                        {
                            //Here we are checking all classes specified in a pattern. We have to loop
                            //through all classes and generate a check instruction (Traitch) for each.
                            for (var i = 0; i < n.Traits.Count; i++)
                            {
                                var t = n.Traits[i];
                                PushVar(sysVar);
                                EmitSpecName(t.Prefix, "$$$" + t.Name, n, ElaCompilerError.UnknownClass);
                                cw.Emit(Op.Traitch);
                                cw.Emit(Op.Brfalse, failLab);
                            }
                        }

                        //Process a pattern in type check expression
                        if (n.Expression != null)
                            CompilePattern(sysVar, n.Expression, failLab);
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
                                last = nn;
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
                var str = AddString(fld.FieldName);

                PushVar(sysVar);
                cw.Emit(Op.Pushfld, str);
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

                //Generate a 'short' op code for the first entry
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
            //If a function is not a simple name which is not suppored. If we don't have exactly two
            //parameters this is an error as well.
            if ((call.Target != null && call.Target.Type != ElaNodeType.NameReference) || call.Parameters.Count != 2)
            {
                AddError(ElaCompilerError.InvalidPattern, call.Target, FormatNode(call.Target));
                return;
            }
            else
            {
                //Target can be null if this entry was generated by a compiler from a list literal.
                if (call.Target != null)
                {
                    var nam = GetVariable(call.Target.GetName(), call.Target.Line, call.Target.Column);

                    //The function is a name however it is not a construction operator. Nothing else is
                    //supported at the moment so generate an error and quit.
                    if ((nam.VariableFlags & ElaVariableFlags.Builtin) != ElaVariableFlags.Builtin ||
                        ((ElaBuiltinKind)nam.Data) != ElaBuiltinKind.Cons)
                    {
                        AddError(ElaCompilerError.InvalidPattern, call.Target, FormatNode(call.Target));
                        return;
                    }
                }

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