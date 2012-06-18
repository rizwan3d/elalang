using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Debug;
using Ela.Linking;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		private ElaCompiler comp; //Reference to compiler, to raise ModuleInclude event
                
        //Compilation options
        private bool debug; //Generate extended debug info
		private bool opt;   //Perform optimizations
        private CompilerOptions options;
		
        private CodeWriter cw; //Code emitter
		
        private CodeFrame frame; //Frame that is currently compiled
        private Scope globalScope; //Global scope for the current frame
        private Dictionary<String,Int32> stringLookup;  //String table		
        private FastList<InlineFun> inlineFuns; //Functions that can be inlined
               
        private ExportVars exports; //Exports for current module
		
        //globalScope is not empty (e.g. new Scope()) only if we are resuming in interactive mode
		internal Builder(CodeFrame frame, ElaCompiler comp, ExportVars exportVars, Scope globalScope)
		{
			this.frame = frame;
			this.options = comp.Options;
			this.exports = exportVars;
			this.comp = comp;
			this.cw = new CodeWriter(frame.Ops, frame.OpData);
			this.globalScope = globalScope;
            			
			CurrentScope = globalScope;
			debug = options.GenerateDebugInfo;
			opt = options.Optimize;

			stringLookup = new Dictionary<String, Int32>();
			inlineFuns = new FastList<InlineFun>();

            Success = true;			
		}

        //Entry point
		internal void CompileUnit(ElaExpression expr)
		{
            frame.Layouts.Add(new MemoryLayout(0, 0, 1)); //top level layout
			cw.StartFrame(0);
			
            //Only include prelude if is allowed and if we compiling frame from scratch (not resuming in interactive mode)
			if (!String.IsNullOrEmpty(options.Prelude) && cw.Offset == 0)
				IncludePrelude();

            //Always include arguments module
            IncludeArguments();
			            
            var map = new LabelMap();
            var block = (ElaBlock)expr;

            //Main compilation routines
            RunForwardDeclaration(block, map, Hints.Left);
            RunTypes(block, map, Hints.Left);
            RunInstances(block, map, Hints.Left);
            CompileExpression(expr, map, Hints.Scope);
            
			cw.Emit(Op.Stop);
			cw.CompileOpList();

			frame.Layouts[0].Size = currentCounter;
			frame.Layouts[0].StackSize = cw.FinishFrame();
		}

        private ExprData CompileExpression(ElaExpression exp, LabelMap map, Hints hints)
        {
            var exprData = ExprData.Empty;

            switch (exp.Type)
            {
                case ElaNodeType.Builtin:
                    {
                        var v = (ElaBuiltin)exp;
                        CompileBuiltin(v.Kind, v, map);

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(exp);
                    }
                    break;
                case ElaNodeType.Generator:
                    {
                        CompileGenerator((ElaGenerator)exp, map, hints);

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(exp);
                    }
                    break;
                case ElaNodeType.Range:
                    {
                        var r = (ElaRange)exp;
                        CompileRange(exp, r, map, hints);

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(r);
                    }
                    break;
                case ElaNodeType.VariantLiteral:
                    {
                        var v = (ElaVariantLiteral)exp;
                        CompileVariant(v, null, map, hints);
                    }
                    break;
                case ElaNodeType.LazyLiteral:
                    {
                        var v = (ElaLazyLiteral)exp;

                        //Try to optimize lazy section for a case
                        //when a function application is marked as lazy
                        if (!TryOptimizeLazy(v, map, hints))
                        {
                            //Regular lazy section compilation
                            //Create a closure around section
                            CompileFunction(v, FunFlag.Lazy);
                            AddLinePragma(exp);
                            cw.Emit(Op.Newlazy);
                        }

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(v);
                    }
                    break;
                case ElaNodeType.Try:
                    {
                        var s = (ElaTry)exp;

                        var catchLab = cw.DefineLabel();
                        var exitLab = cw.DefineLabel();

                        AddLinePragma(s);
                        cw.Emit(Op.Start, catchLab);

                        var untail = (hints & Hints.Tail) == Hints.Tail ? hints ^ Hints.Tail : hints;
                        CompileExpression(s.Expression, map, untail | Hints.Scope);

                        cw.Emit(Op.Leave);
                        cw.Emit(Op.Br, exitLab);
                        cw.MarkLabel(catchLab);
                        cw.Emit(Op.Leave);

                        CompileMatch(s, null, map, Hints.Throw);

                        cw.MarkLabel(exitLab);
                        cw.Emit(Op.Nop);

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(s);
                    }
                    break;
                case ElaNodeType.Comprehension:
                    {
                        var c = (ElaComprehension)exp;
                        AddLinePragma(c);

                        if (c.Lazy)
                            CompileLazyList(c.Generator, map, hints);
                        else
                        {
                            CompileExpression(c.Initial, map, Hints.None);
                            CompileGenerator(c.Generator, map, Hints.None);
                            AddLinePragma(c);
                            cw.Emit(Op.Genfin);
                        }

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(c);
                    }
                    break;
                case ElaNodeType.Match:
                    {
                        var match = (ElaMatch)exp;
                        CompileMatch(match, match.Expression, map, hints);

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(exp);
                    }
                    break;
                case ElaNodeType.FunctionLiteral:
                    {
                        var f = (ElaFunctionLiteral)exp;
                        CompileFunction(f, FunFlag.None);
                        exprData = new ExprData(DataKind.FunParams, f.ParameterCount);

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(exp);
                    }
                    break;
                case ElaNodeType.Binding:
                    CompileDeclaration((ElaBinding)exp, map, hints);
                    break;
                case ElaNodeType.Block:
                    {
                        var s = (ElaBlock)exp;
                        AddLinePragma(s);
                        var len = s.Expressions.Count - 1;

                        //Normally this should be an impossible situation
                        if (len < 0)
                            AddError(ElaCompilerError.InvalidProgram, s);

                        for (var i = 0; i < len; i++)
                            CompileExpression(s.Expressions[i], map, Hints.Left);

                        //The last expression yields a value (no Left hint therefore).
                        CompileExpression(s.Expressions[len], map, hints);
                    }
                    break;
                case ElaNodeType.Condition:
                    {
                        var s = (ElaCondition)exp;
                        CompileConditionalOperator(s, map, hints);
                    }
                    break;
                case ElaNodeType.Raise:
                    {
                        var s = (ElaRaise)exp;

                        if (s.Expression != null)
                            CompileExpression(s.Expression, map, Hints.None);
                        else
                            cw.Emit(Op.Pushstr_0);

                        cw.Emit(Op.Pushstr, AddString(s.ErrorCode));
                        AddLinePragma(s);
                        cw.Emit(Op.Throw);
                    }
                    break;
                case ElaNodeType.Binary:
                    CompileBinary((ElaBinary)exp, map, hints);
                    break;
                case ElaNodeType.Primitive:
                    {
                        var p = (ElaPrimitive)exp;
                        exprData = CompilePrimitive(p, map, hints);
                    }
                    break;
                case ElaNodeType.RecordLiteral:
                    {
                        var p = (ElaRecordLiteral)exp;
                        exprData = CompileRecord(p, map, hints);
                    }
                    break;
                case ElaNodeType.FieldReference:
                    {
                        var p = (ElaFieldReference)exp;

                        //Here we check if a field reference is actually an external name
                        //prefixed by a module alias. This call is not neccessary (modules
                        //are first class) but is used as optimization.
                        if (!TryOptimizeFieldReference(p))
                        {
                            CompileExpression(p.TargetObject, map, Hints.None);
                            AddLinePragma(p);
                            cw.Emit(Op.Pushfld, AddString(p.FieldName));
                        }

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(exp);
                    }
                    break;
                case ElaNodeType.ListLiteral:
                    {
                        var p = (ElaListLiteral)exp;
                        exprData = CompileList(p, map, hints);
                    }
                    break;
                case ElaNodeType.VariableReference:
                    {
                        var v = (ElaVariableReference)exp;
                        AddLinePragma(v);
                        var scopeVar = GetVariable(v.VariableName, v.Line, v.Column);

                        if ((hints & Hints.Lazy) == Hints.Lazy && (scopeVar.Flags & ElaVariableFlags.NoInit) == ElaVariableFlags.NoInit &&
                            CurrentScope.LocalOrParent(v.VariableName))
                        {
                            //Disabled (explicit better than implicit)                        
                            //EmitAsLazy(scopeVar);
                            //AddLinePragma(exp);
                            //cw.Emit(Op.Newlazy);
                            AddWarning(ElaCompilerWarning.BottomValue, exp, v.VariableName);
                            AddHint(ElaCompilerHint.UseThunk, exp, v.VariableName);
                        }

                        EmitVar(scopeVar);

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(v);

                        if ((scopeVar.VariableFlags & ElaVariableFlags.Function) == ElaVariableFlags.Function)
                            exprData = new ExprData(DataKind.FunParams, scopeVar.Data);
                        else if ((scopeVar.VariableFlags & ElaVariableFlags.ObjectLiteral) == ElaVariableFlags.ObjectLiteral)
                            exprData = new ExprData(DataKind.VarType, (Int32)ElaVariableFlags.ObjectLiteral);
                        else if ((scopeVar.VariableFlags & ElaVariableFlags.Builtin) == ElaVariableFlags.Builtin)
                            exprData = new ExprData(DataKind.Builtin, scopeVar.Data);
                    }
                    break;
                case ElaNodeType.Placeholder:
                    {
                        if ((hints & Hints.Left) != Hints.Left)
                            AddError(ElaCompilerError.PlaceholderNotValid, exp);
                        else
                        {
                            AddLinePragma(exp);
                            cw.Emit(Op.Pop);
                        }
                    }
                    break;
                case ElaNodeType.FunctionCall:
                    {
                        var v = (ElaFunctionCall)exp;
                        CompileFunctionCall(v, map, hints);

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(v);
                    }
                    break;
                case ElaNodeType.Is:
                    {
                        var v = (ElaIs)exp;

                        if ((hints & Hints.Scope) != Hints.Scope)
                            StartScope(false, v.Line, v.Column);

                        CompileExpression(v.Expression, map, Hints.None | Hints.Scope);
                        var addr = AddVariable();
                        cw.Emit(Op.Popvar, addr);
                        var next = cw.DefineLabel();
                        var exit = cw.DefineLabel();
                        var err = cw.DefineLabel();

                        AddLinePragma(v);
                        cw.Emit(Op.Start, err);
                        CompilePattern(addr, null, v.Pattern, map, next, ElaVariableFlags.None, hints);
                        cw.Emit(Op.PushI1_1);
                        cw.Emit(Op.Br, exit);
                        cw.MarkLabel(err);

                        cw.Emit(Op.Pop);
                        cw.MarkLabel(next);
                        cw.Emit(Op.PushI1_0);
                        cw.MarkLabel(exit);
                        cw.Emit(Op.Leave);

                        if ((hints & Hints.Scope) != Hints.Scope)
                            EndScope();

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(v);
                    }
                    break;
                case ElaNodeType.UnitLiteral:
                    if ((hints & Hints.Left) != Hints.Left)
                        cw.Emit(Op.Pushunit);

                    exprData = new ExprData(DataKind.VarType, -1);
                    break;
                case ElaNodeType.TupleLiteral:
                    {
                        var v = (ElaTupleLiteral)exp;
                        exprData = CompileTuple(v, map, hints);
                    }
                    break;
            }

            return exprData;
        }
		
		private int AddString(string val)
		{
			var index = 0;

			if (!stringLookup.TryGetValue(val, out index))
			{
				frame.Strings.Add(val);
				index = frame.Strings.Count - 1;
				stringLookup.Add(val, index);
			}

			return index;
		}

		internal bool Success { get; private set; }
		
		internal Scope CurrentScope { get; private set; }
	}
}