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


		internal Builder(CodeFrame frame, CompilerOptions options, Scope globalScope)
		{
			this.frame = frame;
			this.options = options;
			this.cw = new CodeWriter(frame.Ops, frame.OpData);
			this.currentCounter = frame.Layouts.Count > 0 ? frame.Layouts[0].Size : 0;
			CurrentScope = globalScope;
			debug = options.GenerateDebugInfo;
			opt = options.Optimize;
			pdb = new DebugWriter();
			Errors = new FastList<ElaMessage>();
			counters = new FastStack<Int32>();
			stringLookup = new Dictionary<String, Int32>();
			Success = true;
			shownHints = new Dictionary<ElaCompilerHint,ElaCompilerHint>();
		}
		#endregion


		#region Main
		internal void CompileUnit(ElaExpression expr)
		{
			frame.Layouts.Add(new MemoryLayout(0, 0, 1));

			cw.StartFrame(0);
			var map = new LabelMap();
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
				case ElaNodeType.Range:
					{
						var r = (ElaRange)exp;
						CompileRange(exp, r, map, hints);
					}
					break;
				case ElaNodeType.VariantLiteral:
					{
						var v = (ElaVariantLiteral)exp;

						if (v.Expression != null)
							CompileExpression(v.Expression, map, Hints.None);
						else
							cw.Emit(Op.Pushunit);
						
						AddLinePragma(v);
						cw.Emit(Op.Newvar, AddString(v.Tag));
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
				case ElaNodeType.CustomOperator:
					{
						var s = (ElaCustomOperator)exp;
						ReferencePervasive(s.Operator);
					}
					break;
				case ElaNodeType.BuiltinFunction:
					{
						var s = (ElaBuiltinFunction)exp;
						AddLinePragma(s);
						CompileBuiltin(s, map);

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(s);
					}
					break;
				case ElaNodeType.BaseReference:
					AddError(ElaCompilerError.BaseNotAllowed, exp);
					break;
				case ElaNodeType.Try:
					{
						var s = (ElaTry)exp;
												
						CheckEmbeddedWarning(s.Expression);

						var catchLab = cw.DefineLabel();
						var exitLab = cw.DefineLabel();

						AddLinePragma(s);
						cw.Emit(Op.Start, catchLab);
						
						StartScope(false);
						var untail = (hints & Hints.Tail) == Hints.Tail ? hints ^ Hints.Tail : hints;
						CompileExpression(s.Expression, map, untail | Hints.Scope);
						EndScope();

						cw.Emit(Op.Leave);
						cw.Emit(Op.Br, exitLab);
						cw.MarkLabel(catchLab);
						cw.Emit(Op.Leave);

						StartScope(false);

						CompileMatch(s, null, map, Hints.Throw);
						EndScope();

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

						var modRef = new ModuleReference(s.Name, s.DllName, s.Path.ToArray(), s.Line, s.Column);
						frame.AddReference(s.Alias, modRef);
						AddLinePragma(s);
						var modIndex = AddString(modRef.ToString());
						cw.Emit(Op.Runmod, modIndex);
						cw.Emit(Op.Newmod, modIndex);
						var addr = AddVariable(s.Alias, s, ElaVariableFlags.Module, modIndex);

						if (addr != -1)
						{
							cw.Emit(Op.Popvar, addr);

							if (s.HasImports)
							{
								for (var i = 0; i < s.Imports.Count; i++)
								{
									var im = s.Imports[i];
									AddLinePragma(im);
									cw.Emit(Op.Pushvar, addr);
									cw.Emit(Op.Pushfld, AddString(im.ExternalName));
									var varAddr = AddVariable(im.LocalName, im, ElaVariableFlags.None, -1);
									cw.Emit(Op.Popvar, varAddr);
								}
							}
						}
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
					if ((hints & Hints.Left) == Hints.Left)
						AddValueNotUsed(exp);
					else
					{
						var f = (ElaFunctionLiteral)exp;
						CompileFunction(f, FunFlag.None);
						exprData = new ExprData(DataKind.FunParams, f.ParameterCount);
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
							StartScope(false);

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
						StartScope(false);
						CompileExpression(s.Condition, map, Hints.None | Hints.Scope);

						var falseLab = cw.DefineLabel();
						cw.Emit(Op.Brfalse, falseLab);
						var left = (hints & Hints.Left) == Hints.Left;
						var tailHints = (hints & Hints.Tail) == Hints.Tail ? Hints.Tail : (left ? Hints.Left : Hints.None);
						var newHints = ((s.False == null || left) ? Hints.Left : Hints.None) | tailHints;

						if (s.True != null)
							CompileExpression(s.True, map, newHints | Hints.Scope);

						EndScope();

						if (s.False != null)
						{
							StartScope(false);
							var skipLabel = cw.DefineLabel();
							cw.Emit(Op.Br, skipLabel);
							cw.MarkLabel(falseLab);
							CompileExpression(s.False, map, tailHints | Hints.Scope);
							cw.MarkLabel(skipLabel);
							EndScope();
							cw.Emit(Op.Nop);
						}
						else
						{
							cw.MarkLabel(falseLab);
							
							if ((hints & Hints.Left) != Hints.Left)
								cw.Emit(Op.Pushunit);
							else
								cw.Emit(Op.Nop);
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
				case ElaNodeType.Unary:
					CompileUnary((ElaUnary)exp, map, hints);
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

						if (p.TargetObject.Type == ElaNodeType.BaseReference)
						{
							var error = false;
							var var = GetParentVariable(p.FieldName, out error);

							if (error)
								AddError(ElaCompilerError.BaseNotAllowed, exp);
							else if (var.IsEmpty())
								AddError(ElaCompilerError.BaseVariableNotFound, p, p.FieldName);
							else
							{
								AddLinePragma(p);

								if ((hints & Hints.Left) == Hints.Left && (hints & Hints.Assign) == Hints.Assign)
								{
									AddError(ElaCompilerError.AssignImmutableVariable, p, p.FieldName);
									AddHint(ElaCompilerHint.UseReferenceCell, p);
								}
								else
									cw.Emit(Op.Pushvar, var.Address);
							}
						}
						else
						{
							CompileExpression(p.TargetObject, map, Hints.None);
							AddLinePragma(p);

							if ((hints & Hints.Left) == Hints.Left &&
								(hints & Hints.Assign) == Hints.Assign)
								cw.Emit(Op.Popfld, AddString(p.FieldName));
							else
							{
								cw.Emit(Op.Pushfld, AddString(p.FieldName));

								if ((hints & Hints.Left) == Hints.Left)
									AddValueNotUsed(exp);
							}
						}

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

						if ((hints & Hints.Left) == Hints.Left &&
							(hints & Hints.Assign) == Hints.Assign)
						{

							if (v.Index.Type == ElaNodeType.VariableReference)
							{
								var vr = (ElaVariableReference)v.Index;
								var svar = GetVariable(vr.VariableName);

								if (svar.IsEmpty())
									AddError(ElaCompilerError.UndefinedVariable, vr, vr.VariableName);

								AddLinePragma(v);
								cw.Emit(Op.Popelemi, svar.Address);
							}
							else
							{
								CompileExpression(v.Index, map, Hints.None);
								AddLinePragma(v);
								cw.Emit(Op.Popelem);
							}
						}
						else
						{
							if (v.Index.Type == ElaNodeType.VariableReference)
							{
								var vr = (ElaVariableReference)v.Index;
								var svar = GetVariable(vr.VariableName);

								if (svar.IsEmpty())
									AddError(ElaCompilerError.UndefinedVariable, vr, vr.VariableName);

								AddLinePragma(v);
								cw.Emit(Op.Pushelemi, svar.Address);
							}
							else
							{
								CompileExpression(v.Index, map, Hints.None);
								AddLinePragma(v);
								cw.Emit(Op.Pushelem);
							}

							if ((hints & Hints.Left) == Hints.Left)
								AddValueNotUsed(exp);
						}
					}
					break;
				case ElaNodeType.Argument:
					{
						var v = (ElaArgument)exp;
						AddLinePragma(v);
						cw.Emit(Op.Pusharg, AddString(v.ArgumentName));

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(v);
					}
					break;
				case ElaNodeType.VariableReference:
					{
						var v = (ElaVariableReference)exp;
						AddLinePragma(v);
						var scopeVar = default(ScopeVar);
						
						if ((scopeVar = GetVariable(v.VariableName)).IsEmpty())
							AddError(ElaCompilerError.UndefinedVariable, v, v.VariableName);
						else
						{
							if ((hints & Hints.Left) == Hints.Left && (hints & Hints.Assign) == Hints.Assign)
							{
								AddError(ElaCompilerError.AssignImmutableVariable, exp, v.VariableName);
								AddHint(ElaCompilerHint.UseReferenceCell, exp);
							}
							else
							{
								cw.Emit(Op.Pushvar, scopeVar.Address);

								if ((hints & Hints.Left) == Hints.Left)
									AddValueNotUsed(v);
							}
						}

						if ((scopeVar.VariableFlags & ElaVariableFlags.Function) == ElaVariableFlags.Function)
							exprData = new ExprData(DataKind.FunParams, scopeVar.Data);
						else if ((scopeVar.VariableFlags & ElaVariableFlags.ObjectLiteral) == ElaVariableFlags.ObjectLiteral)
							exprData = new ExprData(DataKind.VarType, (Int32)ElaVariableFlags.ObjectLiteral);
					}
					break;
				case ElaNodeType.Placeholder:
					{
						if ((hints & Hints.Left) == Hints.Left)
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
					}
					break;
				case ElaNodeType.Cast:
					{
						var v = (ElaCast)exp;
						CompileExpression(v.Expression, map, Hints.None);
						AddLinePragma(v);
						AddConv(v.CastAffinity, v);

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(v);
					}
					break;
				case ElaNodeType.Is:
					{
						var v = (ElaIs)exp;

						if ((hints & Hints.Scope) != Hints.Scope)
							StartScope(false);

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
					else if ((hints & Hints.Assign) == Hints.Assign)
						AddError(ElaCompilerError.UnableAssignExpression, exp, FormatNode(exp));
					break;
				case ElaNodeType.TupleLiteral:
					{
						var v = (ElaTupleLiteral)exp;

						if ((hints & Hints.Assign) == Hints.Assign)
						{
							for (var i = 0; i < v.Parameters.Count; i++)
							{
								var p = v.Parameters[i];
								cw.Emit(Op.Dup);

								if (i == 0)
									cw.Emit(Op.PushI4_0);
								else
									cw.Emit(Op.PushI4, i);

								cw.Emit(Op.Pushelem);

								if ((p.Flags & ElaExpressionFlags.Assignable) != ElaExpressionFlags.Assignable)
									AddError(ElaCompilerError.UnableAssignExpression, p, FormatNode(exp));
								else
									CompileExpression(p, map, Hints.None);
							}

							cw.Emit(Op.Pop);
						}
						else
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


		private void AddConv(ElaTypeCode aff, ElaExpression exp)
		{
			switch (aff)
			{
				case ElaTypeCode.Integer:
				case ElaTypeCode.Long:
				case ElaTypeCode.Single:
				case ElaTypeCode.Double:
				case ElaTypeCode.Boolean:
				case ElaTypeCode.String:
				case ElaTypeCode.Char:
					cw.Emit(Op.Conv, (Int32)aff);
					break;
				default:
					AddError(ElaCompilerError.CastNotSupported, exp, TypeCodeFormat.GetShortForm(aff));
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

				if (f.Mutable)
					cw.Emit(Op.PushI4, 1);
				else
					cw.Emit(Op.PushI4_0); 
				
				CompileExpression(f.FieldValue, map, Hints.None);
				cw.Emit(Op.Pushstr, AddString(f.FieldName));
				cw.Emit(Op.Reccons);
			}
		}
		

		private void CompileFunctionCall(ElaFunctionCall v, LabelMap map, Hints hints)
		{
			var tail = (hints & Hints.Tail) == Hints.Tail;
			var len = v.Parameters.Count;

			for (var i = 0; i < len; i++)
				CompileExpression(v.Parameters[len - i - 1], map, Hints.None);

			if (tail && map.FunctionName != null && map.FunctionName == v.GetName() &&
				map.FunctionParameters == len && map.FunctionScope == GetScope(map.FunctionName))
			{
				AddLinePragma(v);
				cw.Emit(Op.Br, map.FunStart);
				return;
			}

			if (v.Target.Type == ElaNodeType.BuiltinFunction)
			{
				var bf = (ElaBuiltinFunction)v.Target;

				if (len != bf.ParameterCount)
				{
					AddLinePragma(bf);
					CompileBuiltin(bf, map);

					for (var i = 0; i < len; i++)
						cw.Emit(Op.Call);
				}
				else
					CompileBuiltinInline(bf, map, hints);

				if ((hints & Hints.Left) == Hints.Left)
				{
					if ((bf.Flags & ElaExpressionFlags.ReturnsUnit) != ElaExpressionFlags.ReturnsUnit)
						AddValueNotUsed(v.Target);
					else
						cw.Emit(Op.Pop);
				}

				return;
			}

			var ed = CompileExpression(v.Target, map, Hints.None);

			if (v.FlipParameters)
				cw.Emit(Op.Flip);

			if (ed.Type == DataKind.VarType)
				AddWarning(ElaCompilerWarning.FunctionInvalidType, v);

			AddLinePragma(v);

			for (var i = 0; i < len; i++)
			{
				if (i == v.Parameters.Count - 1 && tail && opt)
					cw.Emit(Op.Callt);
				else
					cw.Emit(Op.Call);
			}

			if ((hints & Hints.Left) == Hints.Left)
				cw.Emit(Op.Pop);
		}
		#endregion
		

		#region Service
		#region Messages
		private void AddError(ElaCompilerError error, ElaExpression exp, params object[] args)
		{
			Errors.Add(new ElaMessage(Strings.GetError(error, args), MessageType.Error,
				(Int32)error, exp.Line, exp.Column));
			Success = false;
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
			else if (str.Length > 0 && str[0] != '\'')
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


		private void StartScope(bool fun)
		{
			CurrentScope = new Scope(fun, CurrentScope);

			if (debug)
				pdb.StartScope(cw.Offset);
		}


		private void EndScope()
		{
			CurrentScope = CurrentScope.Parent != null ? CurrentScope.Parent : null;

			if (debug)
				pdb.EndScope(cw.Offset);
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


		private void AddLinePragma(ElaExpression exp)
		{
			pdb.AddLineSym(cw.Offset, exp.Line, exp.Column);
		}


		private void AddVarPragma(string name, int address, int offset)
		{
			if (debug)
				pdb.AddVarSym(name, address, offset);
		}
		#endregion


		#region Variables
		private void AddOperator(ElaBinding s, LabelMap map, Hints hints)
		{
			var addr = 0;

			if ((addr = AddPervasive(s.VariableName)) == -1)
				AddError(ElaCompilerError.OperatorAlreadyDeclared, s, s.VariableName);

			if (CurrentScope.Parent != null)
				AddError(ElaCompilerError.OperatorOnlyInGlobal, s);

			CompileExpression(s.InitExpression, map, Hints.None);
			AddLinePragma(s);
			cw.Emit(Op.Popvar, addr);

			if ((hints & Hints.Left) != Hints.Left)
				cw.Emit(Op.Pushunit);
		}


		private Scope FindGlobalScope()
		{
			var sc = CurrentScope;

			while (sc.Parent != null)
				sc = sc.Parent;

			return sc;
		}


		private int AddVariable()
		{
			var ret = 0 | currentCounter << 8;
			currentCounter++;
			return ret;
		}


		private int AddPervasive(string name)
		{
			if (!frame.DeclaredPervasives.ContainsKey(name))
			{
				var addr = AddVariable();
				frame.DeclaredPervasives.Add(name, addr);
				return addr;
			}
			else
				return -1;
		}


		private int AddVariable(string name, ElaExpression exp, ElaVariableFlags flags, int data)
		{
			if (exp != null && IsRegistered(name))
			{
				AddError(ElaCompilerError.VariableAlreadyDeclared, exp, name);
				return -1;
			}

			if (Builtins.Kind(name) != ElaBuiltinFunctionKind.None)
			{
				AddError(ElaCompilerError.NameReserved, exp, name);
				return -1;
			}

			CurrentScope.Locals.Add(name, new ScopeVar(flags, currentCounter, data));
			
			if (exp != null)
				AddVarPragma(name, currentCounter, cw.Offset);

			return AddVariable();
		}


		private int ReferenceGlobal(int addr)
		{
			return counters.Count | (addr >> 8) << 8;
		}


		private bool IsRegistered(string name)
		{
			return CurrentScope.Locals.ContainsKey(name);
		}


		private ScopeVar GetLocalVariable(string name)
		{
			var var = default(ScopeVar);

			if (CurrentScope.Locals.TryGetValue(name, out var))
			{
				var.Address = 0 | var.Address << 8;
				return var;
			}
			else
				return ScopeVar.Empty;
		}


		private ScopeVar GetLocalOrBaseVariable(string name, out bool parent)
		{
			var var = default(ScopeVar);
			parent = false;

			if (CurrentScope.Locals.TryGetValue(name, out var))
			{
				var.Address = 0 | var.Address << 8;
				return var;
			}
			else if (CurrentScope.Parent.Locals.TryGetValue(name, out var))
			{
				parent = true;
				var.Address = 0 | var.Address << 8;
				return var;
			}
			else
				return ScopeVar.Empty;
		}


		private ScopeVar GetVariable(string name)
		{
			return GetVariable(name, CurrentScope, 0);
		}


		private ScopeVar GetVariable(string name, Scope startScope, int startShift)
		{
			var cur = startScope;
			var shift = startShift;

			do
			{
				var var = default(ScopeVar);

				if (cur.Locals.TryGetValue(name, out var))
				{
					var.Address = shift | var.Address << 8;
					return var;
				}

				if (cur.Function)
					shift++;

				cur = cur.Parent;
			}
			while (cur != null);

			return ScopeVar.Empty;
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


		private ScopeVar GetParentVariable(string name, out bool error)
		{
			var cur = CurrentScope;
			error = false;

			while (cur != null && !cur.Function)
				cur = cur.Parent;

			if (cur != null)
				cur = cur.Parent;
			else
				error = true;

			return cur == null ? ScopeVar.Empty : GetVariable(name, cur, 1);
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