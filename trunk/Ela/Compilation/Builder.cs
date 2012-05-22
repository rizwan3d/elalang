using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Debug;
using Ela.Linking;
using Ela.Runtime;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Construction
        private bool debug;
		private bool opt;
		private CodeWriter cw;
		private DebugWriter pdb;
		private CodeFrame frame;
		private CompilerOptions options;
		private FastStack<Int32> counters;
		private int currentCounter;
		private Dictionary<String,Int32> stringLookup;
		private Dictionary<ElaCompilerHint,ElaCompilerHint> shownHints;
		private Scope globalScope;
		private ExportVars exports;
		private ElaCompiler comp;
		private Dictionary<String,InlineFun> inlineFuns;
        private FastStack<NoInit> allowNoInits;

		internal Builder(CodeFrame frame, ElaCompiler comp, ExportVars exportVars, Scope globalScope)
		{
			this.frame = frame;
			this.options = comp.Options;
			this.exports = exportVars;
			this.comp = comp;
			this.cw = new CodeWriter(frame.Ops, frame.OpData);
			this.currentCounter = frame.Layouts.Count > 0 ? frame.Layouts[0].Size : 0;
			this.globalScope = globalScope;
			CurrentScope = globalScope;
			debug = options.GenerateDebugInfo;
			opt = options.Optimize;
			pdb = new DebugWriter();
			Errors = new FastList<ElaMessage>();
			counters = new FastStack<Int32>();
			stringLookup = new Dictionary<String, Int32>();
			Success = true;
			shownHints = new Dictionary<ElaCompilerHint,ElaCompilerHint>();
			inlineFuns = new Dictionary<String,InlineFun>();
			allowNoInits = new FastStack<NoInit>();
		}
		#endregion


		#region Main
		internal void CompileUnit(ElaExpression expr)
		{
            frame.Layouts.Add(new MemoryLayout(0, 0, 1));
			cw.StartFrame(0);
			var map = new LabelMap();

			if (!String.IsNullOrEmpty(options.Prelude) && cw.Offset == 0)
				IncludePrelude();

            IncludeArguments();
			CompileExpression(expr, map, Hints.Scope);
			cw.Emit(Op.Stop);
			cw.CompileOpList();

			frame.Layouts[0].Size = currentCounter;
			frame.Layouts[0].StackSize = cw.FinishFrame();
		}

        
		private void IncludePrelude()
		{
			var modRef = new ModuleReference(frame, options.Prelude) { NoPrelude = true };
			var alias = modRef.ModuleName;
			frame.AddReference(alias, modRef);
            frame.HandleMap.Add(-1);
			var modIndex = AddString(modRef.ToString());
			cw.Emit(Op.Runmod, modIndex);
			cw.Emit(Op.Newmod, modIndex);
			var addr = AddVariable(alias, null, ElaVariableFlags.Module|ElaVariableFlags.Private, modIndex);
			comp.OnModuleInclude(new ModuleEventArgs(modRef));

			if (addr != -1)
				cw.Emit(Op.Popvar, addr);
		}

        private void IncludeArguments()
        {
            var modRef = new ModuleReference(frame, ElaLinker.ARG_MODULE) { NoPrelude = true, LogicalHandle = frame.HandleMap.Count };
            frame.AddReference(ElaLinker.ARG_MODULE, modRef);
            frame.HandleMap.Add(-1);
            var modIndex = AddString(modRef.ToString());
            cw.Emit(Op.Runmod, modIndex);
            comp.OnModuleInclude(new ModuleEventArgs(modRef));			
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
						ScopeVar sv;

						if (globalScope.Locals.TryGetValue(v.Tag, out sv) && (sv.VariableFlags & ElaVariableFlags.Module) == ElaVariableFlags.Module)
						{
							AddLinePragma(v);
							cw.Emit(Op.Pushvar, counters.Count | sv.Address << 8);
						}
						else
							CompileVariant(v, null, map);

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(v);
					}
					break;
				case ElaNodeType.LazyLiteral:
					{
						var v = (ElaLazyLiteral)exp;

						if (!TryOptimizeLazy(v, map, hints))
						{
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
												
						CheckEmbeddedWarning(s.Expression);

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

                        CompileMatch(s, null, map, Hints.Throw, ScopeVar.Empty);

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
					}
					break;
				case ElaNodeType.ModuleInclude:
					{
						var s = (ElaModuleInclude)exp;

                        var modHandle = frame.References.Count;
                        var modRef = new ModuleReference(frame, s.Name, s.DllName, s.Path.ToArray(), s.Line, s.Column, s.RequireQuailified, modHandle);
                        frame.AddReference(s.Alias, modRef);
                        frame.HandleMap.Add(-1);
						AddLinePragma(s);
						var modIndex = AddString(modRef.ToString());
						cw.Emit(Op.Runmod, modIndex);
						cw.Emit(Op.Newmod, modIndex);
						var addr = AddVariable(s.Alias, s, ElaVariableFlags.Module|ElaVariableFlags.Private, modIndex);
						comp.OnModuleInclude(new ModuleEventArgs(modRef));

						if (addr != -1)
							cw.Emit(Op.Popvar, addr);

						if (s.HasImportList)
						{
							var il = s.ImportList;

							for (var i = 0; i < il.Count; i++)
							{
								var im = il[i];
								var a = AddVariable(im.LocalName, im, im.Private ? ElaVariableFlags.Private : ElaVariableFlags.None, -1);

								AddLinePragma(im);
								cw.Emit(Op.Pushvar, addr);
								cw.Emit(Op.Pushfld, AddString(im.Name));
								cw.Emit(Op.Popvar, a);
							}
						}
					}
					break;
				case ElaNodeType.Match:
					{
						var match = (ElaMatch)exp;
						CompileMatch(match, match.Expression, map, hints, ScopeVar.Empty);

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(exp);
					}
					break;
				case ElaNodeType.FunctionLiteral:
					{
						var f = (ElaFunctionLiteral)exp;
						CompileFunction(f, (hints & Hints.Extends) == Hints.Extends ? FunFlag.Extends : FunFlag.None);
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
						var scope = s.StartScope && (hints & Hints.Scope) != Hints.Scope;

						if (scope)
							StartScope(false, s.Line, s.Column);

						if (s.Expressions.Count > 0)
						{
							var len = s.Expressions.Count - 1;

							for (var i = 0; i < len; i++)
								CompileExpression(s.Expressions[i], map, Hints.Left);

							CompileExpression(s.Expressions[len], map, hints);
						}
						else if ((hints & Hints.Left) != Hints.Left)
							cw.Emit(Op.Pushunit);

						if (scope)
						{
							cw.Emit(Op.Nop);
							EndScope();
						}
					}
					break;
				case ElaNodeType.Condition:
					{
						var s = (ElaCondition)exp;
						CheckEmbeddedWarning(s.True);						
						CheckEmbeddedWarning(s.False);

						AddLinePragma(exp);
						CompileExpression(s.Condition, map, Hints.None | Hints.Scope);

						var falseLab = cw.DefineLabel();
						cw.Emit(Op.Brfalse, falseLab);
						var left = (hints & Hints.Left) == Hints.Left;
						var tailHints = (hints & Hints.Tail) == Hints.Tail ? Hints.Tail : (left ? Hints.Left : Hints.None);
						
						if (s.True != null)
                            CompileExpression(s.True, map, tailHints | Hints.Scope);

						if (s.False != null)
						{
							var skipLabel = cw.DefineLabel();
							cw.Emit(Op.Br, skipLabel);
							cw.MarkLabel(falseLab);
							CompileExpression(s.False, map, tailHints | Hints.Scope);
							cw.MarkLabel(skipLabel);
							cw.Emit(Op.Nop);
						}
						else
						{
                            AddError(ElaCompilerError.ElseMissing, s.True);
                            AddHint(ElaCompilerHint.AddElse, s.True);
						}
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
					{
						var ed = CompileBinary((ElaBinary)exp, map, hints);

						if (ed.Type != DataKind.None)
							exprData = ed;
					}
					break;
				case ElaNodeType.Primitive:
					{
						var p = (ElaPrimitive)exp;
						AddLinePragma(p);
						PushPrimitive(p.Value);

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(p);
					}
					break;
				case ElaNodeType.RecordLiteral:
					{
						var p = (ElaRecordLiteral)exp;
						AddLinePragma(p);
						cw.Emit(Op.Newrec, p.Fields.Count);
						ProcessFields(p.Fields, p, map, hints);
						
						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(p);
					}
					break;
				case ElaNodeType.FieldReference:
					{
						var p = (ElaFieldReference)exp;

						if (p.TargetObject.Type == ElaNodeType.VariantLiteral)
						{
							AddLinePragma(p.TargetObject);
                            EmitVar(GetVariable(p.TargetObject.GetName(), p.Line, p.Column));
						}
						else
							CompileExpression(p.TargetObject, map, Hints.None);

						AddLinePragma(p);

						cw.Emit(Op.Pushfld, AddString(p.FieldName));

						if ((hints & Hints.Left) == Hints.Left)
						    AddValueNotUsed(exp);
					}
					break;
				case ElaNodeType.ListLiteral:
					{
						var p = (ElaListLiteral)exp;
						var len = p.Values.Count;

						if (len > 0)
						{
							AddLinePragma(p);
							cw.Emit(Op.Newlist);

							for (var i = 0; i < len; i++)
							{
								CompileExpression(p.Values[p.Values.Count - i - 1], map, Hints.None);
								cw.Emit(Op.Gen);
							}
						}
						else
						{
							AddLinePragma(p);
							cw.Emit(Op.Newlist);
						}

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(p);
					}
					break;
				case ElaNodeType.Indexer:
					{
						var v = (ElaIndexer)exp;
						CompileExpression(v.TargetObject, map, Hints.None);

						CompileExpression(v.Index, map, Hints.None);
						AddLinePragma(v);
						cw.Emit(Op.Pushelem);

						if ((hints & Hints.Left) == Hints.Left)
						    AddValueNotUsed(exp);
					}
					break;
				case ElaNodeType.VariableReference:
					{
						var v = (ElaVariableReference)exp;
						AddLinePragma(v);
						var scopeVar = GetVariable(v.VariableName, v.Line, v.Column);

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

						AddLinePragma(v);
						CompilePattern(addr, null, v.Pattern, map, next, ElaVariableFlags.None, hints | Hints.Silent);
						cw.Emit(Op.PushI1_1);
						cw.Emit(Op.Br, exit);
						cw.MarkLabel(next);
						cw.Emit(Op.PushI1_0);
						cw.MarkLabel(exit);
						cw.Emit(Op.Nop);

						if ((hints & Hints.Scope) != Hints.Scope)
							EndScope();

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(v);
					}
					break;
				case ElaNodeType.UnitLiteral:
					if ((hints & Hints.Left) != Hints.Left)
						cw.Emit(Op.Pushunit);
					break;
				case ElaNodeType.TupleLiteral:
					{
						var v = (ElaTupleLiteral)exp;
						CompileTupleParameters(v, v.Parameters, map, hints);
					}
					break;
			}

			return exprData;
		}


		
		#endregion


		#region Complex
		private void PushPrimitive(ElaLiteralValue val)
		{
			switch (val.LiteralType)
			{
				case ElaTypeCode.String:
					var str = val.AsString();

					if (str.Length == 0)
						cw.Emit(Op.Pushstr_0);
					else
						cw.Emit(Op.Pushstr, AddString(str));
					break;
				case ElaTypeCode.Char:
					cw.Emit(Op.PushCh, val.AsInteger());
					break;
				case ElaTypeCode.Integer:
					var v = val.AsInteger();

					if (v == 0)
						cw.Emit(Op.PushI4_0);
					else
						cw.Emit(Op.PushI4, v);
					break;
				case ElaTypeCode.Long:
					cw.Emit(Op.PushI4, val.GetData().I4_1);
					cw.Emit(Op.PushI4, val.GetData().I4_2);
					cw.Emit(Op.NewI8);
					break;
				case ElaTypeCode.Single:
					cw.Emit(Op.PushR4, val.GetData().I4_1);
					break;
				case ElaTypeCode.Double:
					cw.Emit(Op.PushI4, val.GetData().I4_1);
					cw.Emit(Op.PushI4, val.GetData().I4_2);
					cw.Emit(Op.NewR8);
					break;
				case ElaTypeCode.Boolean:
					cw.Emit(val.AsBoolean() ? Op.PushI1_1 : Op.PushI1_0);
					break;
				default:
					cw.Emit(Op.Pushunit);
					break;
			}
		}
		

		private void CompileTupleParameters(ElaExpression exp, List<ElaExpression> pars, LabelMap map, Hints hints)
		{
			if (pars.Count == 2)
			{
				CompileExpression(pars[0], map, Hints.None);
				CompileExpression(pars[1], map, Hints.None);
				cw.Emit(Op.Newtup_2);
			}
			else
			{
				AddLinePragma(exp);
				cw.Emit(Op.Newtup, pars.Count);

				for (var i = 0; i < pars.Count; i++)
				{
					CompileExpression(pars[i], map, Hints.None);
					cw.Emit(Op.Gen);
				}
			}

			if ((hints & Hints.Left) == Hints.Left)
				AddValueNotUsed(exp);
		}

		
		private void ProcessFields(List<ElaFieldDeclaration> fields, ElaExpression exp, LabelMap map, Hints hints)
		{
			for (var i = 0; i < fields.Count; i++)
			{
				var f = fields[i];
				CompileExpression(f.FieldValue, map, Hints.None);
				cw.Emit(Op.Pushstr, AddString(f.FieldName));
				cw.Emit(Op.Reccons);
			}
		}


		private void CompileVariant(ElaVariantLiteral v, ElaExpression arg, LabelMap map)
		{
			if (arg == null)
				cw.Emit(Op.Pushunit);
			else
				CompileExpression(arg, map, Hints.None);

			AddLinePragma(v);
			cw.Emit(Op.Newvar, AddString(v.Tag));
		}


		private bool CompileInlineCall(ElaFunctionCall v, LabelMap map, Hints hints)
		{
			if (v.Target.Type != ElaNodeType.VariableReference)
				return false;

			var name = v.Target.GetName();
			var fun = default(InlineFun);

			if (!inlineFuns.TryGetValue(name, out fun) || fun.Scope != CurrentScope || 
                map.FunctionName == name)
				return false;

			var len = v.Parameters.Count;
			
			if (len != fun.Literal.ParameterCount)
				return false;

            var oc = CurrentScope;
			CurrentScope = new Scope(false, fun.Scope);
			
			CompileFunction(fun.Literal, FunFlag.Inline);

            CurrentScope = oc;
			return true;
		}


		private ExprData CompileFunctionCall(ElaFunctionCall v, LabelMap map, Hints hints)
		{
			var ed = ExprData.Empty;
						
			if (v.Target.Type == ElaNodeType.VariantLiteral)
			{
				if (v.Parameters.Count != 1)
					AddError(ElaCompilerError.InvalidVariant, v, v);

				CompileVariant((ElaVariantLiteral)v.Target, v.Parameters[0], map);
				return ed;
			}

			var tail = (hints & Hints.Tail) == Hints.Tail;
			var len = v.Parameters.Count;

			for (var i = 0; i < len; i++)
				CompileExpression(v.Parameters[len - i - 1], map, Hints.None);

			if (tail && map.FunctionName != null && map.FunctionName == v.GetName() &&
				map.FunctionParameters == len && map.FunctionScope == GetScope(map.FunctionName))
			{
				AddLinePragma(v);
				cw.Emit(Op.Br, map.FunStart);
				return ed;
			}
			
			if (opt && CompileInlineCall(v, map, hints))
				return ed;			

			if (v.Target.Type == ElaNodeType.VariableReference)
			{
				var bf = (ElaVariableReference)v.Target;
				var sv = GetVariable(bf.VariableName, bf.Line, bf.Column);

                if ((sv.Flags & ElaVariableFlags.Builtin) == ElaVariableFlags.Builtin)
				{
					var kind = (ElaBuiltinKind)sv.Data;
					var pars = Builtins.Params(kind);

					if (len != pars)
					{
						AddLinePragma(bf);
						CompileBuiltin(kind, v.Target, map);

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
					AddLinePragma(v.Target);
					EmitVar(sv);

					if ((sv.VariableFlags & ElaVariableFlags.Function) == ElaVariableFlags.Function)
						ed = new ExprData(DataKind.FunParams, sv.Data);
					else if ((sv.VariableFlags & ElaVariableFlags.ObjectLiteral) == ElaVariableFlags.ObjectLiteral)
						ed = new ExprData(DataKind.VarType, (Int32)ElaVariableFlags.ObjectLiteral);
				}
			}
			else
				ed = CompileExpression(v.Target, map, Hints.None);
			
			if (v.FlipParameters)
				cw.Emit(Op.Flip);

			if (ed.Type == DataKind.VarType)
				AddWarning(ElaCompilerWarning.FunctionInvalidType, v);

			AddLinePragma(v);

			for (var i = 0; i < len; i++)
			{
				if (i == v.Parameters.Count - 1 && tail && opt && !map.InlineFunction)
					cw.Emit(Op.Callt);
				else
					cw.Emit(Op.Call);
			}

			return ed;
		}
		#endregion
		

		#region Service
		#region Messages
		private void AddError(ElaCompilerError error, ElaExpression exp, params object[] args)
		{
			AddError(error, exp.Line, exp.Column, args);
		}


        private void AddError(ElaCompilerError error, int line, int col, params object[] args)
        {
            if (Errors.Count < 101)
            {
                Errors.Add(new ElaMessage(Strings.GetError(error, args), MessageType.Error,
                    (Int32)error, line, col));
                Success = false;
            }
        }


		private void AddWarning(ElaCompilerWarning warning, ElaExpression exp, params object[] args)
		{
			if (options.WarningsAsErrors)
				AddError((ElaCompilerError)warning, exp, args);
			else if (!options.NoWarnings)
				Errors.Add(new ElaMessage(Strings.GetWarning(warning, args), MessageType.Warning,
					(Int32)warning, exp.Line, exp.Column));
		}


		private void AddHint(ElaCompilerHint hint, ElaExpression exp, params object[] args)
		{
			if (options.ShowHints && !options.NoWarnings && !shownHints.ContainsKey(hint))
			{
				Errors.Add(new ElaMessage(Strings.GetHint(hint, args), MessageType.Hint,
					(Int32)hint, exp.Line, exp.Column));
				shownHints.Add(hint, hint);
			}
		}


		private void AddValueNotUsed(ElaExpression exp)
		{
			AddWarning(ElaCompilerWarning.ValueNotUsed, exp);
			AddHint(ElaCompilerHint.UseIgnoreToPop, exp);
			cw.Emit(Op.Pop);
		}


		private void CheckEmbeddedWarning(ElaExpression exp)
		{
			if (exp != null && exp.Type == ElaNodeType.Binding &&
				((ElaBinding)exp).In == null)
				AddWarning(ElaCompilerWarning.EmbeddedBinding, exp);
		}


		private string FormatNode(ElaExpression exp)
		{
			var str = exp.ToString();

			if (str.IndexOf('\n') != -1 || str.Length > 40)
				str = "\r\n    " + str + "\r\n";
			else if (str.Length > 0 && str[0] != '\'' && str[0] != '"')
				str = "'" + str + "'";

			return str.Length > 150 ? str.Substring(0, 150) : str;
		}
		#endregion


		#region Debug
		private void StartFun(string name, int pars)
		{
			cw.StartFrame(pars);
			pdb.StartFunction(name, cw.Offset, pars);
		}


		private int EndFun(int handle)
		{
			pdb.EndFunction(handle, cw.Offset);
			return cw.FinishFrame();
		}


		private void StartScope(bool fun, int line, int col)
		{
			CurrentScope = new Scope(fun, CurrentScope);

			if (debug)
				pdb.StartScope(cw.Offset, line, col);
		}


		private void EndScope()
		{
			CurrentScope = CurrentScope.Parent != null ? CurrentScope.Parent : null;

			if (debug)
				pdb.EndScope(cw.Offset, lastLine, lastColumn);
		}


		private void StartSection()
		{
			counters.Push(currentCounter);
			currentCounter = 0;
		}


		private void EndSection()
		{
			currentCounter = counters.Pop();
		}


        private int lastLine;
        private int lastColumn;
		private void AddLinePragma(ElaExpression exp)
		{
            lastLine = exp.Line;
            lastColumn = exp.Column;
			pdb.AddLineSym(cw.Offset, exp.Line, exp.Column);
		}


		private void AddVarPragma(string name, int address, int offset, ElaVariableFlags flags, int data)
		{
			if (debug)
				pdb.AddVarSym(name, address, offset, (Int32)flags, data);
		}
		#endregion


		#region Variables
		[Flags]
		private enum GetFlags
		{
			None = 0x00,
			OnlyGet = 0x02,
			NoError = 0x04
		}

        private void EmitVar(ScopeVar sv)
        {
            if ((sv.Flags & ElaVariableFlags.External) == ElaVariableFlags.External)
                cw.Emit(Op.Pushext, sv.Address);
            else
                cw.Emit(Op.Pushvar, sv.Address);
        }

		private int AddVariable()
		{
			var ret = 0 | currentCounter << 8;
			currentCounter++;
			return ret;
		}


		private int AddVariable(string name, ElaExpression exp, ElaVariableFlags flags, int data)
		{
            CurrentScope.Locals.Remove(name); //Hides an already declared variable

			CurrentScope.Locals.Add(name, new ScopeVar(flags, currentCounter, data));

			if (debug && exp != null)
			{
				cw.Emit(Op.Nop);
				AddVarPragma(name, currentCounter, cw.Offset, flags, data);
				AddLinePragma(exp);
			}

			return AddVariable();
		}
        
		private bool Validate(ScopeVar var)
		{
			if ((var.Flags & ElaVariableFlags.NoInit) != ElaVariableFlags.NoInit)
                return true;

            if (allowNoInits.Count == 0)
                return false;

            var d = allowNoInits.Peek();

            if (d.Allow && d.Code == var.Data)
                return true;

            foreach (var n in allowNoInits)
                if (n.Allow && n.Code == var.Data)
                    return true;

            return false;
		}


		private ScopeVar GetVariable(string name, int line, int col)
		{
			return GetVariable(name, CurrentScope, 0, GetFlags.None, line, col);
		}


		private ScopeVar GetVariable(string name, Scope startScope, int startShift, GetFlags getFlags, int line, int col)
		{
			var cur = startScope;
			var shift = startShift;

			do
			{
				var var = default(ScopeVar);

				if (cur.Locals.TryGetValue(name, out var))
				{
                    var.Address = shift | var.Address << 8;

                    if (!Validate(var))
                        AddError(ElaCompilerError.ReferNoInit, line, col, name);

					return var;
				}

				if (cur.Function)
					shift++;

				cur = cur.Parent;
			}
			while (cur != null);

			if ((getFlags & GetFlags.OnlyGet) == GetFlags.OnlyGet)
                return ScopeVar.Empty;

			var vk = default(ExportVarData);

			if (exports.FindName(name, out vk))
			{
                var flags = ElaVariableFlags.None;
                var data = -1;

                if (vk.Kind != ElaBuiltinKind.None)
                {
                    flags = ElaVariableFlags.Builtin;
                    data = (Int32)vk.Kind;
                }

                frame.LateBounds.Add(new LateBoundSymbol(name, vk.ModuleHandle | vk.Address << 8, data, line, col));
                return new ScopeVar(flags | ElaVariableFlags.External, vk.ModuleHandle | vk.Address << 8, data);
			}
			else
			{
                if (!options.IgnoreUndefined && (getFlags & GetFlags.NoError) != GetFlags.NoError)
				    AddError(ElaCompilerError.UndefinedName, line, col, name);

				return ScopeVar.Empty;
			}
		}


		private Scope GetScope(string name)
		{
			var cur = CurrentScope;

			do
			{
				if (cur.Locals.ContainsKey(name))
					return cur;

				cur = cur.Parent;
			}
			while (cur != null);

			return null;
		}


		private ScopeVar GetParentVariable(string name, int line, int col, out bool error)
		{
			var cur = CurrentScope;
			error = false;

			while (cur != null && !cur.Function)
				cur = cur.Parent;

			if (cur != null)
				cur = cur.Parent;
			else
				error = true;

			return cur == null ? ScopeVar.Empty : GetVariable(name, cur, 1, GetFlags.None, line, col);
		}
		#endregion


		#region String Table
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
		#endregion


		#region Misc
		private Hints Untail(Hints hints)
		{
			if ((hints & Hints.Tail) == Hints.Tail)
				return hints ^ Hints.Tail;
			else
				return hints;
		}
		#endregion
		#endregion


		#region Properties
		internal bool Success { get; private set; }

		internal FastList<ElaMessage> Errors { get; private set; }

		internal Scope CurrentScope { get; private set; }

		internal DebugInfo Symbols
		{
			get { return pdb != null ? pdb.Symbols : null; }
		}
		#endregion
	}
}