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

            ResumeIndexer();
            Success = true;			
		}

        //Entry point
		internal void CompileUnit(ElaProgram prog)
		{
            frame.Layouts.Add(new MemoryLayout(0, 0, 1)); //top level layout
			cw.StartFrame(0);
			
            //Only include prelude if is allowed and if we compiling frame from scratch (not resuming in interactive mode)
			if (!String.IsNullOrEmpty(options.Prelude) && cw.Offset == 0)
				IncludePrelude();

            //Always include arguments module
            IncludeArguments();
			            
            var map = new LabelMap();

            //Main compilation routine
            CompileProgram(prog, map);
            
            //Every Ela module should end with a Stop op code
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
                case ElaNodeType.As:
                case ElaNodeType.TypeCheck:
                case ElaNodeType.Equation:
                    AddError(ElaCompilerError.InvalidExpression, exp, FormatNode(exp));
                    break;
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
                        CompileVariant(v, v.Expression, map, hints);
                    }
                    break;
                case ElaNodeType.LazyLiteral:
                    {
                        var v = (ElaLazyLiteral)exp;
                        CompileLazy(v, map, hints);
                    }
                    break;
                case ElaNodeType.LetBinding:
                    {
                        var v = (ElaLetBinding)exp;
                        StartScope(false, v.Line, v.Column);
                        WalkDeclarations(v.Equations, map, Hints.None);
                        CompileExpression(v.Expression, map, hints);
                        EndScope();
                    }
                    break;
                case ElaNodeType.Try:
                    {
                        var s = (ElaTry)exp;
                        CompileTryExpression(s, map, hints);
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
                            cw.Emit(Op.Newlist);
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
                        var n = (ElaMatch)exp;
                        CompileMatchExpression(n, map, hints);
                    }
                    break;
                case ElaNodeType.Lambda:
                    {
                        var f = (ElaLambda)exp;
                        var pc = CompileLambda(f);
                        exprData = new ExprData(DataKind.FunParams, pc);

                        if ((hints & Hints.Left) == Hints.Left)
                            AddValueNotUsed(exp);
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
                case ElaNodeType.NameReference:
                    {
                        var v = (ElaNameReference)exp;
                        AddLinePragma(v);
                        var scopeVar = GetVariable(v.Name, v.Line, v.Column);

                        PushVar(scopeVar);

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
                case ElaNodeType.Juxtaposition:
                    {
                        var v = (ElaJuxtaposition)exp;
                        CompileFunctionCall(v, map, hints);

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