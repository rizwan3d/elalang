using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Debug;
using Ela.Linking;
using Ela.Runtime;

namespace Ela.Compilation
{
	internal sealed partial class CompilerHelper
	{
		#region Construction
		private bool debug;
		private bool opt;
		private CodeWriter cw;
		private DebugWriter pdb;
		private CodeFrame frame;
		private CompilerOptions options;
		private Queue<Int32> currParams;
		private FastStack<Int32> counters;
		private int currentCounter;
		private Dictionary<String,Int32> stringLookup;
		private Dictionary<ElaCompilerHint,ElaCompilerHint> shownHints;

		internal CompilerHelper(CodeFrame frame, CompilerOptions options, Scope globalScope)
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
			currParams = new Queue<Int32>();
			shownHints = new Dictionary<ElaCompilerHint,ElaCompilerHint>();
		}
		#endregion


		#region Main
		internal int CompileUnit(ElaCodeUnit unit)
		{
			var map = new LabelMap(Label.Empty);
			var len = unit.Expressions.Count;

			for (var i = 0; i < len; i++)
			{
				var s = unit.Expressions[i];
				CompileExpression(unit.Expressions[i], map, i == len - 1 ? Hints.None : Hints.Left);
			}

			cw.Emit(Op.Term);
			cw.CompileOpList();
			return currentCounter;
		}


		private ExprData CompileExpression(ElaExpression exp, LabelMap map, Hints hints)
		{
			var exprData = ExprData.Empty;

			switch (exp.Type)
			{
				case ElaNodeType.Typeof:
					{
						var s = (ElaTypeof)exp;
						CompileExpression(s.Expression, map, Hints.None);
						AddLinePragma(s);
						cw.Emit(Op.Typeof);

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(s);
					}
					break;
				case ElaNodeType.BaseReference:
					AddError(ElaCompilerError.BaseNotAllowed, exp);
					break;
				case ElaNodeType.TryCatch:
					{
						var s = (ElaTryCatch)exp;
												
						if (s.Try.Type == ElaNodeType.VariableDeclaration)
							AddWarning(ElaCompilerWarning.EmbeddedBinding, s.Try);

						if (s.Catch.Type == ElaNodeType.VariableDeclaration)
							AddWarning(ElaCompilerWarning.EmbeddedBinding, s.Catch);
						
						var catchLab = cw.DefineLabel();
						var exitLab = cw.DefineLabel();

						AddLinePragma(s);
						cw.Emit(Op.Start, catchLab);
						
						StartScope(false);
						CompileExpression(s.Try, map, hints | Hints.Scope);
						EndScope();

						cw.Emit(Op.Leave);
						cw.Emit(Op.Br, exitLab);
						cw.MarkLabel(catchLab);
						cw.Emit(Op.Leave);

						StartScope(false);

						if (s.Variable != null)
						{
							var addr = AddVariable(s.Variable.VariableName, s.Variable, ElaVariableFlags.Immutable, -1);
							AddLinePragma(s.Variable);
							cw.Emit(Op.Popvar, addr);
						}

						CompileExpression(s.Catch, map, Hints.Throw | Hints.Scope);
						EndScope();

						cw.MarkLabel(exitLab);
						cw.Emit(Op.Nop);
					}
					break;
				case ElaNodeType.VariantLiteral:
					{
						var s = (ElaVariantLiteral)exp;
						var hdl = 0;
						var fn = s.Name + "@" + s.Length;

						if (!frame.Variants.TryGetValue(fn, out hdl))
						{
							hdl = CompileVariant(s);
							frame.Variants.Add(fn, hdl);
						}

						AddLinePragma(exp);
						cw.Emit(Op.PushI4, s.Length);
						cw.Emit(Op.Newfun, hdl);
					}
					break;
				case ElaNodeType.Ignore:
					{
						var s = (ElaIgnore)exp;
						CompileExpression(s.Expression, map, Hints.None);
						AddLinePragma(s);
						cw.Emit(Op.Pop);

						if ((hints & Hints.Left) != Hints.Left)
							cw.Emit(Op.Pushunit);
					}
					break;
				case ElaNodeType.AsyncLiteral:
					CompileFunction((ElaAsyncLiteral)exp, FunFlag.Async);
					AddLinePragma(exp);
					cw.Emit(Op.Calla);
					break;
				case ElaNodeType.LazyLiteral:
					CompileFunction((ElaLazyLiteral)exp, FunFlag.Async);
					AddLinePragma(exp);
					cw.Emit(Op.Newlazy);
					break;
				case ElaNodeType.While:
					{
						var s = (ElaWhile)exp;
						CompileWhile(s, map, hints);						
					}
					break;
				case ElaNodeType.For:
					{
						var s = (ElaFor)exp;

						if (s.ForType == ElaForType.Foreach)
							CompileForeach(s, map, hints);
						else
							CompileFor(s, map, hints);
					}
					break;
				case ElaNodeType.ModuleInclude:
					{
						var s = (ElaModuleInclude)exp;

						var modRef = new ModuleReference(s.Name, s.DllName, s.Folder, s.Line, s.Column);
						frame.AddReference(s.Alias, modRef);
						AddLinePragma(s);
						var modIndex = AddString(modRef.ToString());
						cw.Emit(Op.Runmod, modIndex);
						cw.Emit(Op.Newmod, modIndex);
						var addr = AddVariable(s.Alias, s, ElaVariableFlags.Immutable, -1);

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
									var varAddr = AddVariable(im.LocalName, im, ElaVariableFlags.Immutable, -1);
									cw.Emit(Op.Popvar, varAddr);
								}
							}
						}
					}
					break;
				case ElaNodeType.Yield:
					{
						if (map.GotoParam == -1)
							AddError(ElaCompilerError.YieldNotAllowed, exp);
						else
						{
							var y = (ElaYield)exp;
							CompileExpression(y.Expression, map, Hints.None);
							AddLinePragma(y);
							cw.Emit(Op.Yield, map.GotoParam);

							if ((hints & Hints.Left) != Hints.Left)
								cw.Emit(Op.Pushunit);
						}
					}
					break;
				case ElaNodeType.Match:
					CompileMatch((ElaMatch)exp, map, hints);

					if ((hints & Hints.Left) == Hints.Left)
						AddValueNotUsed(exp);
					break;
				case ElaNodeType.FunctionLiteral:
					if ((hints & Hints.Left) == Hints.Left)
						AddValueNotUsed(exp);
					else
					{
						var f = (ElaFunctionLiteral)exp;

						if ((f.Flags & ElaExpressionFlags.HasYield) == ElaExpressionFlags.HasYield)
							CompileGenerator(f);
						else
							CompileFunction(f, FunFlag.None);
					}
					break;
				case ElaNodeType.VariableDeclaration:
					{
						var s = (ElaVariableDeclaration)exp;

						if ((s.VariableFlags & ElaVariableFlags.Constructor) == ElaVariableFlags.Constructor)
						{
							if ((s.VariableFlags & ElaVariableFlags.Immutable) != ElaVariableFlags.Immutable)
								AddWarning(ElaCompilerWarning.ConstructorAsMutable, s);
							
							CompileFunction((ElaFunctionLiteral)s.InitExpression, FunFlag.Constructor);
						}
						else
						{
							if (s.InitExpression == null)
								AddError(ElaCompilerError.VariableDeclarationInitMissing, s);

							if (s.Pattern == null)
							{
								if (options.StrictMode && CurrentScope.Parent == null &&
									(s.VariableFlags & ElaVariableFlags.Immutable) != ElaVariableFlags.Immutable)
									AddError(ElaCompilerError.StrictModeMutableGlobal, exp, s.VariableName);

								var addr = 0;
								var funPars = -1;

								if (s.InitExpression.Type == ElaNodeType.FunctionLiteral)
								{
									var fun = (ElaFunctionLiteral)s.InitExpression;
									funPars = fun.ParameterCount;

									if (fun.FunctionType != ElaFunctionType.Standard)
									{
										AddOperator(s, map, hints);
										break;
									}
								}

								addr = AddVariable(s.VariableName, s, s.VariableFlags, -1);
								var ed = CompileExpression(s.InitExpression, map, Hints.None);
								
								if (ed.Type == DataKind.FunCurry || ed.Type == DataKind.FunParams)
									CurrentScope.ChangeVariable(s.VariableName, new ScopeVar(s.VariableFlags | ElaVariableFlags.Function,  addr, ed.Data));
								
								AddLinePragma(s);
								cw.Emit(Op.Popvar, addr);
							}
							else
								CompileDeclPattern(s, map);
						}

						if ((hints & Hints.Left) != Hints.Left)
							cw.Emit(Op.Pushunit);
					}
					break;
				case ElaNodeType.Block: //scope
					{
						var s = (ElaBlock)exp;
						AddLinePragma(s);

						if ((hints & Hints.Scope) != Hints.Scope)
							StartScope(false);

						cw.Emit(Op.Nop);

						if (s.Expressions.Count > 0)
						{
							var len = s.Expressions.Count - 1;

							for (var i = 0; i < len; i++)
								CompileExpression(s.Expressions[i], map, Hints.Left);

							CompileExpression(s.Expressions[len], map, hints);
						}
						else if ((hints & Hints.Left) != Hints.Left)
							cw.Emit(Op.Pushunit);

						cw.Emit(Op.Nop);

						if ((hints & Hints.Scope) != Hints.Scope)
							EndScope();
					}
					break;
				case ElaNodeType.Return:
					if (map.Exit.IsEmpty())
						AddError(ElaCompilerError.ReturnInGlobal, exp);
					else if (map.GotoParam != -1)
					{
						AddError(ElaCompilerError.ReturnInGenerator, exp);
						AddHint(ElaCompilerHint.UseYieldInGenerator, exp);
					}
					else
					{
						var s = (ElaReturn)exp;
						CompileExpression(s.Expression, map, Hints.None);
						AddLinePragma(s);
						cw.Emit(Op.Br, map.Exit);
					}
					break;
				case ElaNodeType.Break:
					if (map.BlockEnd.IsEmpty())
						AddError(ElaCompilerError.BreakNotAllowed, exp);
					else
					{
						AddLinePragma(exp);
						cw.Emit(Op.Br, map.BlockEnd);

						if ((hints & Hints.Left) != Hints.Left)
							cw.Emit(Op.Pushunit);
					}
					break;
				case ElaNodeType.Continue:
					if (map.BlockStart.IsEmpty())
						AddError(ElaCompilerError.ContinueNotAllowed, exp);
					else
					{
						AddLinePragma(exp);
						cw.Emit(Op.Br, map.BlockStart);

						if ((hints & Hints.Left) != Hints.Left)
							cw.Emit(Op.Pushunit);
					}
					break;
				case ElaNodeType.Condition: //scope
					{
						var s = (ElaCondition)exp;

						if (s.True != null && s.True.Type == ElaNodeType.VariableDeclaration)
							AddWarning(ElaCompilerWarning.EmbeddedBinding, s.True);
						
						if (s.False != null && s.False.Type == ElaNodeType.VariableDeclaration)
							AddWarning(ElaCompilerWarning.EmbeddedBinding, s.False);

						AddLinePragma(exp);
						StartScope(false);
						CompileExpression(s.Condition, map, Hints.None | Hints.Scope);

						if (s.Unless)
							cw.Emit(Op.Not);

						var falseLab = cw.DefineLabel();
						cw.Emit(Op.Brfalse, falseLab);
						var left = (hints & Hints.Left) == Hints.Left;
						var tailHints = (hints & Hints.Tail) == Hints.Tail ? Hints.Tail : (left ? Hints.Left : Hints.None);
						var newHints = ((s.False == null || left) ? Hints.Left : Hints.None) | tailHints;

						if (s.True != null)
							CompileExpression(s.True, map, newHints | Hints.Scope);

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
							cw.MarkLabel(falseLab);

							if ((hints & Hints.Left) != Hints.Left)
								cw.Emit(Op.Pushunit);
							else
								cw.Emit(Op.Nop);
						}

						EndScope();
					}
					break;
				case ElaNodeType.Cout:
					{
						var s = (ElaCout)exp;
						CompileExpression(s.Expression, map, Hints.None);
						AddLinePragma(s);
						cw.Emit(Op.Cout);

						if ((hints & Hints.Left) != Hints.Left)
							cw.Emit(Op.Pushunit);
					}
					break;
				case ElaNodeType.Throw:
					{
						var s = (ElaThrow)exp;
						CompileExpression(s.Expression, map, Hints.None);
						AddLinePragma(s);
						cw.Emit(Op.Throw);
					}
					break;
				case ElaNodeType.Assign:
					{
						var s = (ElaAssign)exp;
						CompileAssign(s, s.Left, s.Right, hints, map);
					}
					break;
				case ElaNodeType.Binary:
					{
						var bin = (ElaBinary)exp;
						var newHints = 
							((hints & Hints.Scope) == Hints.Scope ? Hints.Scope : Hints.None) |
							((hints & Hints.Nested) == Hints.Nested ? Hints.Nested : Hints.None) | 
							((hints & Hints.Tail) == Hints.Tail ? Hints.Tail : Hints.None);

						if ((bin.Left.Flags & ElaExpressionFlags.ReturnsUnit) == ElaExpressionFlags.ReturnsUnit)
							AddWarning(ElaCompilerWarning.UnitAlwaysFail, bin.Left);

						if ((bin.Right.Flags & ElaExpressionFlags.ReturnsUnit) == ElaExpressionFlags.ReturnsUnit)
							AddWarning(ElaCompilerWarning.UnitAlwaysFail, bin.Right);

						if ((bin.Left.Flags & ElaExpressionFlags.BreaksExecution) == ElaExpressionFlags.BreaksExecution)
							AddError(ElaCompilerError.BreakExecutionNotAllowed, bin.Left);
						else if ((bin.Right.Flags & ElaExpressionFlags.BreaksExecution) == ElaExpressionFlags.BreaksExecution)
							AddError(ElaCompilerError.BreakExecutionNotAllowed, bin.Right);
						else if (bin.Operator == ElaBinaryOperator.CompBackward ||
							bin.Operator == ElaBinaryOperator.CompForward)
						{
							exprData = CompileComposition(bin, map, newHints);

							if ((hints & Hints.Left) == Hints.Left)
								AddValueNotUsed(bin);
						}
						else if (bin.Operator == ElaBinaryOperator.Swap)
						{
							CompileSwap(bin, map);

							if ((hints & Hints.Left) != Hints.Left)
								cw.Emit(Op.Pushunit);
						}
						else
						{
							CompileExpression(bin.Left, map, newHints);

							if (bin.Operator != ElaBinaryOperator.BooleanAnd &&
								bin.Operator != ElaBinaryOperator.BooleanOr)
								CompileExpression(bin.Right, map, newHints);

							CompileBinary(bin, map, newHints);

							if ((hints & Hints.Left) == Hints.Left)
							{
								if (bin.Operator != ElaBinaryOperator.PipeBackward && bin.Operator != ElaBinaryOperator.PipeForward)
									AddValueNotUsed(bin);
								else
									cw.Emit(Op.Pop);
							}
						}
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
						ProcessFields(p.Fields, p, map, hints);
						AddLinePragma(p);
						cw.Emit(Op.Newrec, p.Fields.Count);

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

								if ((hints & Hints.Left) == Hints.Left &&
									(hints & Hints.Assign) == Hints.Assign)
								{
									if ((var.Flags & ElaVariableFlags.Immutable) == ElaVariableFlags.Immutable)
										AddError(ElaCompilerError.AssignImmutableVariable, p, p.FieldName);
									else
										cw.Emit(Op.Popvar, var.Address);
								}
								else
									cw.Emit(Op.Pushvar, var.Address);
							}
						}
						else
						{
							CompileExpression(p.TargetObject, map, Hints.None);
							AddLinePragma(p);

							if ((hints & Hints.Left) == Hints.Left)
								cw.Emit(Op.Popfld, AddString(p.FieldName));
							else
								cw.Emit(Op.Pushfld, AddString(p.FieldName));
						}

					}
					break;
				case ElaNodeType.ArrayLiteral:
					{
						var p = (ElaArrayLiteral)exp;

						if (p.Comprehension != null)
						{
							cw.Emit(Op.Pushptr);
							CompileExpression(p.Comprehension, map, Hints.Comp);
							AddLinePragma(p);
							cw.Emit(Op.Newarr, -1);
						}
						else
						{
							var len = p.Values.Count;

							for (var i = 0; i < len; i++)
								CompileExpression(p.Values[i], map, Hints.None);

							AddLinePragma(p);

							if (len == 0)
								cw.Emit(Op.Newarr_0);
							else
								cw.Emit(Op.Newarr, len);
						}

						if ((hints & Hints.Left) == Hints.Left)
							AddValueNotUsed(p);
					}
					break;
				case ElaNodeType.ListLiteral:
					{
						var p = (ElaListLiteral)exp;
						var len = p.Values.Count;

						if (p.Comprehension != null)
						{
							cw.Emit(Op.Pushptr);
							CompileExpression(p.Comprehension, map, Hints.Comp);
							AddLinePragma(p);
							cw.Emit(Op.Listgen, -1);
						}
						else if (len > 0)
						{
							for (var i = 0; i < len; i++)
								CompileExpression(p.Values[i], map, Hints.None);

							AddLinePragma(p);
							cw.Emit(Op.Listgen, len);
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

						if ((hints & Hints.Left) == Hints.Left)
						{
							CompileExpression(v.Index, map, Hints.None);
							AddLinePragma(v);
							cw.Emit(Op.Popelem);
						}
						else
						{
							CompileExpression(v.Index, map, Hints.None);
							AddLinePragma(v);
							cw.Emit(Op.Pushelem);
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
						var addr = 0;

						if ((scopeVar = GetVariable(v.VariableName)).IsEmpty())
							AddError(ElaCompilerError.UndefinedVariable, v, v.VariableName);
						else
						{
							addr = scopeVar.Address;

							if ((hints & Hints.Left) == Hints.Left &&
								(hints & Hints.Assign) == Hints.Assign)
							{
								if ((scopeVar.Flags & ElaVariableFlags.Immutable) == ElaVariableFlags.Immutable)
								{
									AddError(ElaCompilerError.AssignImmutableVariable, exp, v.VariableName);
									AddHint(ElaCompilerHint.UseVarInsteadOfLet, exp);
								}
								else
									cw.Emit(Op.Popvar, addr);
							}
							else
							{
								cw.Emit(Op.Pushvar, addr);

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
						if (currParams.Count == 0)
						{
							if ((hints & Hints.Left) == Hints.Left)
								cw.Emit(Op.Pop);
							else
								AddError(ElaCompilerError.PlaceholderNotValid, exp);
						}
						else
						{
							AddLinePragma(exp);
							cw.Emit(Op.Pushvar, currParams.Dequeue());

							if ((hints & Hints.Left) == Hints.Left)
								AddValueNotUsed(exp);
						}
					}
					break;
				case ElaNodeType.FunctionCall:
					{
						var v = (ElaFunctionCall)exp;
						var tail = (hints & Hints.Tail) == Hints.Tail && map.GotoParam == -1;
						var len = 1;

						if (v.ConvertParametersToTuple)
							CompileTupleParameters(v, v.Parameters, map, hints);
						else
						{
							len = v.Parameters.Count;

							for (var i = 0; i < len; i++)
								CompileExpression(v.Parameters[len - i - 1], map, Hints.None);
						}

						if (tail && v.Target.Type == ElaNodeType.VariableReference && 
							((ElaVariableReference)v.Target).VariableName == map.FunctionName)
						{
							AddLinePragma(v);
							cw.Emit(Op.Br, map.FunStart);
						}
						else
						{
							var ed = CompileExpression(v.Target, map, Hints.None);

							if (ed.Data > -1 && ed.Type == DataKind.FunParams)
							{
								if (ed.Data < len)
									AddWarning(ElaCompilerWarning.FunctionTooManyParams, exp, ed.Data, len);
								else if (ed.Data > 0 && len == 0)
									AddWarning(ElaCompilerWarning.FunctionZeroParams, exp, ed.Data);
								else if (ed.Data > len)
									exprData = new ExprData(DataKind.FunCurry, len);
							}
							else if (ed.Type == DataKind.VarType)
								AddWarning(ElaCompilerWarning.FunctionInvalidType, exp);

							AddLinePragma(v);
							cw.Emit(tail && opt ? Op.Callt : Op.Call, len);

							if ((hints & Hints.Left) == Hints.Left)
								cw.Emit(Op.Pop);
						}
					}
					break;
				case ElaNodeType.Cast:
					{
						var v = (ElaCast)exp;
						CompileExpression(v.Expression, map, Hints.None);
						AddLinePragma(v);
						AddConv(v.CastAffinity);

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
						CompilePattern(addr, null, v.Pattern, map, next, ElaVariableFlags.Immutable, -1);
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
				case ElaNodeType.Unary:
					{
						var v = (ElaUnary)exp;
						CompileExpression(v.Expression, map, Hints.None);
						AddLinePragma(v);
						CompileUnary(v, map, hints);
					}
					break;
				case ElaNodeType.TupleLiteral:
					{
						var v = (ElaTupleLiteral)exp;

						if ((v.Flags & ElaExpressionFlags.ReturnsUnit) == ElaExpressionFlags.ReturnsUnit)
						{
							if ((hints & Hints.Left) != Hints.Left)
								cw.Emit(Op.Pushunit);
							else if ((hints & Hints.Assign) ==	Hints.Assign)
								AddError(ElaCompilerError.UnableAssignExpression, v);
						}
						else
						{
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
										AddError(ElaCompilerError.UnableAssignExpression, p);
									else
										CompileExpression(p, map, Hints.None);
								}

								cw.Emit(Op.Pop);
							}
							else
								CompileTupleParameters(v, v.Parameters, map, hints);
						}
					}
					break;
			}

			return exprData;
		}


		private void CompileTupleParameters(ElaExpression exp, List<ElaExpression> pars, LabelMap map, Hints hints)
		{
			for (var i = 0; i < pars.Count; i++)
				CompileExpression(pars[i], map, Hints.None);

			AddLinePragma(exp);

			if (pars.Count == 2)
				cw.Emit(Op.Newtup_2);
			else
				cw.Emit(Op.Newtup, pars.Count);

			if ((hints & Hints.Left) == Hints.Left)
				AddValueNotUsed(exp);
		}


		private void CompileAssign(ElaExpression exp, ElaExpression left, ElaExpression right, Hints hints, LabelMap map)
		{
			CompileExpression(right, map, Hints.None);

			if ((hints & Hints.Left) != Hints.Left)
				cw.Emit(Op.Dup);

			if ((left.Flags & ElaExpressionFlags.Assignable) != ElaExpressionFlags.Assignable)
				AddError(ElaCompilerError.UnableAssignExpression, exp);
			else
				CompileExpression(left, map, Hints.Assign | Hints.Left);
		}
		#endregion


		#region Complex
		#region Function
		private bool IsTupleLiteral(ElaExpression exp)
		{
			return exp != null && (exp.Type == ElaNodeType.TupleLiteral ||exp.Type == ElaNodeType.RecordLiteral);
		}


		private void CompileGenerator(ElaFunctionLiteral dec)
		{
			StartScope(true);
			StartSection();
			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);
			var address = cw.Offset;

			if (dec.ParameterCount > 0)
				CompileParameters(dec);

			CompileFunction(dec, FunFlag.Generator);
			cw.Emit(Op.Newseq);

			cw.Emit(Op.Ret);
			frame.Layouts.Add(new MemoryLayout(currentCounter, address));
			EndScope();
			EndSection();
			cw.MarkLabel(funSkipLabel);
			cw.Emit(Op.Nop);
			
			if (dec.ParameterCount == 0)
				cw.Emit(Op.PushI4_0);
			else
				cw.Emit(Op.PushI4, dec.ParameterCount);

			cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
		}


		private void CompileFunction(ElaFunctionLiteral dec, FunFlag flag)
		{
			var pars = flag == FunFlag.Generator ? 0 : dec.ParameterCount;
			StartFun(dec.Name, frame.Layouts.Count, dec.ParameterCount);

			var map = new LabelMap(cw.DefineLabel());
			var funSkipLabel = cw.DefineLabel();
			var startLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);

			map.FunStart = startLabel;
			map.FunctionName = dec.Name;
			cw.MarkLabel(startLabel);
			cw.Emit(Op.Nop);

			StartScope(true);
			StartSection();
			AddLinePragma(dec);

			var address = cw.Offset;
			
			if (dec.ParameterSlots > 0)
			{
				for (var i = 0; i < dec.ParameterSlots; i++)
				{
					var a = AddVariable();
					currParams.Enqueue(a);
					cw.Emit(Op.Popvar, a);
				}
			}
			
			CompileParameters(dec);
			
			if (dec.FunctionType == ElaFunctionType.BinaryOperator && pars != 2)
				AddError(ElaCompilerError.OperatorBinaryTwoParams, dec);
			else if (dec.FunctionType == ElaFunctionType.UnaryOperator && pars != 1)
				AddError(ElaCompilerError.OperatorUnaryOneParam, dec);
			else if (dec.FunctionType == ElaFunctionType.Constructor &&
				!IsTupleLiteral(dec.Expression) && (dec.Expression.Type != ElaNodeType.Block ||
				!IsTupleLiteral(((ElaBlock)dec.Expression).LastExpression)))
				AddError(ElaCompilerError.InvalidConstructorBody, dec);
			else
			{
				if (flag == FunFlag.Generator)
				{
					map.GotoParam = AddVariable();
					cw.Emit(Op.Brdyn, map.GotoParam);
				}

				if (dec.Expression != null)
				{
					CompileExpression(dec.Expression, map, Hints.Tail | Hints.Scope);

					if (flag == FunFlag.Constructor)
						cw.Emit(Op.Settag, AddString(dec.Name));
				}
				else
					cw.Emit(Op.Pushunit);

				cw.MarkLabel(map.Exit);

				if (flag == FunFlag.Generator)
					cw.Emit(Op.Epilog, map.GotoParam);
				else
					cw.Emit(Op.Nop);
			}

			frame.Layouts.Add(new MemoryLayout(currentCounter, address));
			EndScope();
			EndSection();

			cw.Emit(Op.Ret);

			cw.MarkLabel(funSkipLabel);
			cw.Emit(Op.Nop);

			AddLinePragma(dec);

			if (flag == FunFlag.Generator)
				cw.Emit(Op.Newfuns, frame.Layouts.Count - 1);
			else if (flag == FunFlag.Constructor)
			{
				var fn = dec.Name + "@" + pars;
				frame.Variants.Remove(fn);
				frame.Variants.Add(fn, frame.Layouts.Count - 1);
			}
			else
			{
				if (pars == 0)
					cw.Emit(Op.PushI4_0);
				else
					cw.Emit(Op.PushI4, pars);

				cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
			}

			EndFun();
		}


		private void CompileParameters(ElaFunctionLiteral fun)
		{
			if (fun.Parameters != null)
			{
				if (fun.Parameters.Type == ElaNodeType.VariableReference)
					AddParameter(fun.Parameters);
				else if (fun.Parameters.Type == ElaNodeType.TupleLiteral)
				{
					var pars = ((ElaTupleLiteral)fun.Parameters).Parameters;

					if (pars != null)
					{
						for (var i = 0; i < pars.Count; i++)
							AddParameter(pars[i]);
					}
				}
				else
					AddError(ElaCompilerError.InvalidFunctionDeclaration, fun);
			}
		}


		private void AddParameter(ElaExpression exp)
		{
			if (exp.Type != ElaNodeType.VariableReference)
				AddError(ElaCompilerError.InvalidParameterDeclaration, exp);
			else
			{
				var addr = AddVariable(((ElaVariableReference)exp).VariableName, exp, ElaVariableFlags.Immutable, -1);
				cw.Emit(Op.Popvar, addr);
			}
		}
		#endregion


		#region Iteration
		private void CompileFor(ElaFor s, LabelMap map, Hints hints)
		{
			if (s.Pattern.Type != ElaNodeType.VariablePattern)
				AddError(ElaCompilerError.MatchNotSupportedInFor, s.Pattern);
			else
			{
				if (s.Body.Type == ElaNodeType.VariableDeclaration)
					AddWarning(ElaCompilerWarning.EmbeddedBinding, s.Body); 
				
				StartScope(false);
				var firstIter = cw.DefineLabel();
				var iter = cw.DefineLabel();
				var exit = cw.DefineLabel();
				var serv = AddVariable();
				var newMap = new LabelMap(map)
				{
					BlockStart = iter,
					BlockEnd = exit
				};

				AddLinePragma(s);
				cw.Emit(Op.Nop);
				var addr = AddVariable(((ElaVariablePattern)s.Pattern).Name, s, s.VariableFlags, -1);

				if (addr != -1)
				{
					if (s.InitExpression != null)
						CompileExpression(s.InitExpression, map, Hints.None);
					else
						cw.Emit(Op.PushI4_0);

					cw.Emit(Op.Popvar, addr);

					CompileExpression(s.Target, map, Hints.None);
					cw.Emit(Op.Popvar, serv);

					cw.Emit(Op.Pushvar, addr);
					cw.Emit(Op.Br, firstIter);

					cw.MarkLabel(iter);

					if (s.ForType == ElaForType.ForDownto)
						cw.Emit(Op.Decr, addr);
					else
						cw.Emit(Op.Incr, addr);

					cw.MarkLabel(firstIter);
					cw.Emit(Op.Pushvar, serv);
					
					if (s.ForType == ElaForType.ForDownto)
						cw.Emit(Op.Br_lt, exit);
					else
						cw.Emit(Op.Br_gt, exit);

					if (s.Pattern.Guard != null)
					{
						CompileExpression(s.Pattern.Guard, map, Hints.None);
						cw.Emit(Op.Brfalse, iter);
					}

					if (s.Body != null)
					{
						var newHints = Hints.Scope |
							((hints & Hints.Comp) == Hints.Comp ? Hints.Comp : Hints.Left);
						CompileExpression(s.Body, newMap, newHints);
					}

					cw.Emit(Op.Br, iter);
					cw.MarkLabel(exit);
					EndScope();
					cw.Emit(Op.Nop);

					if ((hints & Hints.Left) != Hints.Left &&
						(hints & Hints.Comp) != Hints.Comp)
						cw.Emit(Op.Pushunit);
				}
			}
		}


		private void CompileWhile(ElaWhile s, LabelMap map, Hints hints)
		{
			if (s.Body.Type == ElaNodeType.VariableDeclaration)
				AddWarning(ElaCompilerWarning.EmbeddedBinding, s.Body);
			
			StartScope(false);
			var iter = cw.DefineLabel();
			var exit = cw.DefineLabel();
			var cond = cw.DefineLabel();
			var newMap = new LabelMap(map)
			{
				BlockStart = iter,
				BlockEnd = exit
			};

			if (s.WhileType == ElaWhileType.While ||
				s.WhileType == ElaWhileType.Until)
				cw.Emit(Op.Br, cond);

			cw.MarkLabel(iter);

			if (s.Body != null)
			{
				var newHints = Hints.Scope |
					((hints & Hints.Comp) == Hints.Comp ? Hints.None : Hints.Left);
				CompileExpression(s.Body, newMap, newHints);
			}
			else
				cw.Emit(Op.Nop);

			if (!opt || s.WhileType != ElaWhileType.While ||
				s.Condition.Type != ElaNodeType.Primitive || !((ElaPrimitive)s.Condition).Value.AsBoolean())
			{
				cw.MarkLabel(cond);
				CompileExpression(s.Condition, newMap, Hints.None);

				if (s.WhileType == ElaWhileType.Until ||
					s.WhileType == ElaWhileType.DoUntil)
					cw.Emit(Op.Not);

				cw.Emit(Op.Brtrue, iter);
			}
			else
				cw.Emit(Op.Br, iter);

			cw.MarkLabel(exit);
			EndScope();
			cw.Emit(Op.Nop);

			if ((hints & Hints.Left) != Hints.Left &&
				(hints & Hints.Comp) != Hints.Comp)
				cw.Emit(Op.Pushunit);
		}


		private void CompileForeach(ElaFor s, LabelMap map, Hints hints)
		{
			if (s.InitExpression != null)
				AddError(ElaCompilerError.ForeachNoInitialization, s.InitExpression);
			else
			{
				if (s.Body.Type == ElaNodeType.VariableDeclaration)
					AddWarning(ElaCompilerWarning.EmbeddedBinding, s.Body);

				StartScope(false);
				var iter = cw.DefineLabel();
				var exit = cw.DefineLabel();
				var breakExit = cw.DefineLabel();
				var serv = AddVariable();
				var newMap = new LabelMap(map)
				{
					BlockStart = iter,
					BlockEnd = breakExit
				};

				var addr = -1;

				if (s.Pattern.Type == ElaNodeType.VariablePattern)
					addr = AddVariable(((ElaVariablePattern)s.Pattern).Name, s.Pattern, s.VariableFlags, -1);
				else if (s.Pattern.Type == ElaNodeType.BoolPattern &&
					((ElaBoolPattern)s.Pattern).Left != null)
					addr = AddVariable(((ElaBoolPattern)s.Pattern).Left, s.Pattern, s.VariableFlags, -1);
				else
					addr = AddVariable();

				CompileExpression(s.Target, map, Hints.None);
				cw.Emit(Op.Newseq);
				cw.Emit(Op.Popvar, serv);

				cw.MarkLabel(iter);
				cw.Emit(Op.Pushvar, serv);
				cw.Emit(Op.Pushseq);
				cw.Emit(Op.Brptr, exit);
				cw.Emit(Op.Popvar, addr);

				if (s.Pattern.Type != ElaNodeType.VariablePattern || s.Pattern.Guard != null)
					CompilePattern(addr, null, s.Pattern, map, iter, s.VariableFlags, -1);

				if (s.Body != null)
				{
					var newHints = Hints.Scope |
						((hints & Hints.Comp) == Hints.Comp ? Hints.None : Hints.Left);
					CompileExpression(s.Body, newMap, newHints);
				}

				cw.Emit(Op.Br, iter);
				cw.MarkLabel(exit);
				cw.Emit(Op.Pop);
				cw.MarkLabel(breakExit);
				cw.Emit(Op.Nop);
				EndScope();

				if ((hints & Hints.Left) != Hints.Left &&
					(hints & Hints.Comp) != Hints.Comp)
					cw.Emit(Op.Pushunit);
			}
		}
		#endregion


		#region Declare Variable
		private void CompileDeclPattern(ElaVariableDeclaration s, LabelMap map)
		{
			var next = cw.DefineLabel();
			var exit = cw.DefineLabel();
			var addr = AddVariable();
			var tuple = default(ElaTupleLiteral);

			if (s.InitExpression != null)
			{
				if (s.InitExpression.Type == ElaNodeType.TupleLiteral &&
					s.Pattern.Type == ElaNodeType.SeqPattern)
					tuple = (ElaTupleLiteral)s.InitExpression;
				else
				{
					CompileExpression(s.InitExpression, map, Hints.None);
					cw.Emit(Op.Popvar, addr);
				}
			}

			CompilePattern(addr, tuple, s.Pattern, map, next, s.VariableFlags, -1);
			cw.Emit(Op.Br, exit);
			cw.MarkLabel(next);
			cw.Emit(Op.Throw, (Int32)ElaRuntimeError.MatchFailed);
			cw.MarkLabel(exit);
			cw.Emit(Op.Nop);
		}
		#endregion


		#region Primitives
		private void PushPrimitive(ElaLiteralValue val)
		{
			switch (val.LiteralType)
			{
				case ObjectType.String:
					var str = val.AsString();

					if (str.Length == 0)
						cw.Emit(Op.Pushstr_0);
					else
						cw.Emit(Op.Pushstr, AddString(str));
					break;
				case ObjectType.Char:
					cw.Emit(Op.PushCh, val.AsInteger());
					break;
				case ObjectType.Integer:
					var v = val.AsInteger();

					if (v == 0)
						cw.Emit(Op.PushI4_0);
					else
						cw.Emit(Op.PushI4, v);
					break;
				case ObjectType.Long:
					cw.Emit(Op.PushI4, val.GetData().I4_1);
					cw.Emit(Op.PushI4, val.GetData().I4_2);
					cw.Emit(Op.NewI8);
					break;
				case ObjectType.Single:
					cw.Emit(Op.PushR4, val.GetData().I4_1);
					break;
				case ObjectType.Double:
					cw.Emit(Op.PushI4, val.GetData().I4_1);
					cw.Emit(Op.PushI4, val.GetData().I4_2);
					cw.Emit(Op.NewR8);
					break;
				case ObjectType.Boolean:
					cw.Emit(val.AsBoolean() ? Op.PushI1_1 : Op.PushI1_0);
					break;
				default:
					cw.Emit(Op.Pushunit);
					break;
			}
		}


		private void AddConv(ObjectType aff)
		{
			switch (aff)
			{
				case ObjectType.Integer:
					cw.Emit(Op.ConvI4);
					break;
				case ObjectType.Long:
					cw.Emit(Op.ConvI8);
					break;
				case ObjectType.Single:
					cw.Emit(Op.ConvR4);
					break;
				case ObjectType.Double:
					cw.Emit(Op.ConvR8);
					break;
				case ObjectType.Boolean:
					cw.Emit(Op.ConvI1);
					break;
				case ObjectType.String:
					cw.Emit(Op.ConvStr);
					break;
				case ObjectType.Char:
					cw.Emit(Op.ConvCh);
					break;
				case ObjectType.Sequence:
					cw.Emit(Op.ConvSeq);
					break;
			}
		}
		#endregion


		#region Composition
		private ExprData CompileComposition(ElaBinary exp, LabelMap map, Hints hints)
		{
			var exprData = ExprData.Empty;
			var firstData = CompileExpression(exp.Left, map, hints | Hints.Nested);
			var first = AddVariable();
			var funPars = (hints & Hints.Nested) == Hints.Nested ? 1  :
				firstData.Type == DataKind.FunParams || firstData.Type == DataKind.FunCurry ? firstData.Data : -1;
			cw.Emit(Op.Popvar, first);

			var secondData = CompileExpression(exp.Right, map, hints | Hints.Nested);
			var second = AddVariable();
			cw.Emit(Op.Popvar, second);

			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);

			StartSection();
			var address = cw.Offset;

			cw.Emit(Op.Pushvar, 1 | (first >> 8) << 8);

			if (funPars > -1)
				cw.Emit(Op.Call, funPars);
			else
				cw.Emit(Op.Calld);

			cw.Emit(Op.Pushvar, 1 | (second >> 8) << 8);
			cw.Emit(Op.Call, 1);
			cw.Emit(Op.Ret);

			if (secondData.Type == DataKind.FunParams || secondData.Type == DataKind.FunCurry)
			{
				if (secondData.Data < 1)
					AddWarning(ElaCompilerWarning.FunctionTooManyParams, exp, 1, secondData.Data);
				else if (secondData.Data > 1)
					AddWarning(ElaCompilerWarning.FunctionImplicitPartial, exp);
			}

			frame.Layouts.Add(new MemoryLayout(currentCounter, address));
			EndSection();
			cw.MarkLabel(funSkipLabel);
			cw.Emit(Op.Nop);
			
			if (funPars > -1)
			{
				cw.Emit(Op.PushI4, funPars);
				AddLinePragma(exp);
				cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
			}
			else
			{
				cw.Emit(Op.Pushvar, first);
				AddLinePragma(exp);
				cw.Emit(Op.Newfund, frame.Layouts.Count - 1);
			}

			if (firstData.Type == DataKind.FunCurry || firstData.Type == DataKind.FunParams)
				exprData = new ExprData(DataKind.FunParams, firstData.Data);

			return exprData;
		}
		#endregion


		#region Complex objects
		private void ProcessFields(List<ElaFieldDeclaration> fields, ElaExpression exp, LabelMap map, Hints hints)
		{
			for (var i = fields.Count - 1; i > -1; --i)
			{
				var f = fields[i];
				CompileExpression(f.FieldValue, map, Hints.None);
				cw.Emit(Op.Pushstr, AddString(f.FieldName));
			}
		}


		private int CompileVariant(ElaVariantLiteral exp)
		{
			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);

			StartScope(true);
			StartSection();
			var address = cw.Offset;
			var len = exp.Length;
			var addrs = new int[len];

			for (var i = 0; i < len; i++)
			{
				var addr = AddVariable();
				addrs[i] = addr;
				cw.Emit(Op.Popvar, addr);
			}

			for (var i = 0; i < len; i++)
				cw.Emit(Op.Pushvar, addrs[i]);

			if (len == 2)
				cw.Emit(Op.Newtup_2);
			else
				cw.Emit(Op.Newtup, len);

			cw.Emit(Op.Settag, AddString(exp.Name));
			frame.Layouts.Add(new MemoryLayout(currentCounter, address));
			cw.Emit(Op.Ret);

			EndSection();
			EndScope();
			cw.MarkLabel(funSkipLabel);
			cw.Emit(Op.Nop);
			return frame.Layouts.Count - 1;
		}
		#endregion


		#region Operations
		private void CompileUnary(ElaUnary v, LabelMap map, Hints hints)
		{
			var left = false;

			switch (v.Operator)
			{
				case ElaUnaryOperator.BooleanNot:
					cw.Emit(Op.Not);
					break;
				case ElaUnaryOperator.Negate:
					cw.Emit(Op.Neg);
					break;
				case ElaUnaryOperator.Positive:
					cw.Emit(Op.Pos);
					break;
				case ElaUnaryOperator.BitwiseNot:
					cw.Emit(Op.NotBw);
					break;
				case ElaUnaryOperator.ValueOf:
					cw.Emit(Op.Valueof);
					break;
				case ElaUnaryOperator.Increment:
					cw.Emit(Op.PushI4, 1);
					cw.Emit(Op.Add);

					if ((hints & Hints.Left) != Hints.Left)
						cw.Emit(Op.Dup);

					left = true;
					CompileExpression(v.Expression, map, Hints.Left | Hints.Assign);
					break;
				case ElaUnaryOperator.Decrement:
					cw.Emit(Op.PushI4, 1);
					cw.Emit(Op.Sub);

					if ((hints & Hints.Left) != Hints.Left)
						cw.Emit(Op.Dup);

					left = true;
					CompileExpression(v.Expression, map, Hints.Left | Hints.Assign);
					break;
				case ElaUnaryOperator.PostDecrement:
					if ((hints & Hints.Left) != Hints.Left)
						cw.Emit(Op.Dup);

					cw.Emit(Op.PushI4, 1);
					cw.Emit(Op.Sub);
					CompileExpression(v.Expression, map, Hints.Left | Hints.Assign);
					left = true;
					break;
				case ElaUnaryOperator.PostIncrement:
					if ((hints & Hints.Left) != Hints.Left)
						cw.Emit(Op.Dup);

					cw.Emit(Op.PushI4, 1);
					cw.Emit(Op.Add);
					CompileExpression(v.Expression, map, Hints.Left | Hints.Assign);
					left = true;
					break;
				case ElaUnaryOperator.Custom:
					cw.Emit(Op.Pushperv, AddString(v.CustomOperator));
					cw.Emit(Op.Call, 1);
					break;
			}

			if (!left && (hints & Hints.Left) == Hints.Left)
				AddValueNotUsed(v);
		}


		private void CompileBinary(ElaBinary bin, LabelMap map, Hints hints)
		{
			var exitLab = default(Label);
			var termLab = default(Label);

			switch (bin.Operator)
			{
				case ElaBinaryOperator.BooleanAnd:
					termLab = cw.DefineLabel();
					exitLab = cw.DefineLabel();
					cw.Emit(Op.Brfalse, termLab);
					CompileExpression(bin.Right, map, hints);
					cw.Emit(Op.Br, exitLab);
					cw.MarkLabel(termLab);
					cw.Emit(Op.PushI1_0);
					cw.MarkLabel(exitLab);
					cw.Emit(Op.Nop);
					break;
				case ElaBinaryOperator.BooleanOr:
					termLab = cw.DefineLabel();
					exitLab = cw.DefineLabel();
					cw.Emit(Op.Brtrue, termLab);
					CompileExpression(bin.Right, map, hints);
					cw.Emit(Op.Br, exitLab);
					cw.MarkLabel(termLab);
					cw.Emit(Op.PushI1_1);
					cw.MarkLabel(exitLab);
					cw.Emit(Op.Nop);
					break;
				case ElaBinaryOperator.Add:
					cw.Emit(Op.Add);
					break;
				case ElaBinaryOperator.Divide:
					cw.Emit(Op.Div);
					break;
				case ElaBinaryOperator.Multiply:
					cw.Emit(Op.Mul);
					break;
				case ElaBinaryOperator.Power:
					cw.Emit(Op.Pow);
					break;
				case ElaBinaryOperator.Modulus:
					cw.Emit(Op.Rem);
					break;
				case ElaBinaryOperator.Subtract:
					cw.Emit(Op.Sub);
					break;
				case ElaBinaryOperator.ShiftRight:
					cw.Emit(Op.Shr);
					break;
				case ElaBinaryOperator.ShiftLeft:
					cw.Emit(Op.Shl);
					break;
				case ElaBinaryOperator.Greater:
					cw.Emit(Op.Cgt);
					break;
				case ElaBinaryOperator.Lesser:
					cw.Emit(Op.Clt);
					break;
				case ElaBinaryOperator.Equals:
					cw.Emit(Op.Ceq);
					break;
				case ElaBinaryOperator.NotEquals:
					cw.Emit(Op.Cneq);
					break;
				case ElaBinaryOperator.GreaterEqual:
					cw.Emit(Op.Cgteq);
					break;
				case ElaBinaryOperator.LesserEqual:
					cw.Emit(Op.Clteq);
					break;
				case ElaBinaryOperator.BitwiseAnd:
					cw.Emit(Op.AndBw);
					break;
				case ElaBinaryOperator.BitwiseOr:
					cw.Emit(Op.OrBw);
					break;
				case ElaBinaryOperator.BitwiseXor:
					cw.Emit(Op.Xor);
					break;
				case ElaBinaryOperator.ConsList:
					cw.Emit(Op.Listadd);
					break;
				case ElaBinaryOperator.PipeBackward:
				case ElaBinaryOperator.PipeForward:
					cw.Emit(Op.Call, 1);
					break;
				case ElaBinaryOperator.Custom:
					cw.Emit(Op.Pushperv, AddString(bin.CustomOperator));
					cw.Emit(Op.Call, 2);
					break;
			}
		}


		private void CompileSwap(ElaBinary bin, LabelMap map)
		{
			var err = false;

			if ((bin.Left.Flags & ElaExpressionFlags.Assignable) != ElaExpressionFlags.Assignable)
			{
				AddError(ElaCompilerError.UnableAssignExpression, bin.Left);
				err = true;
			}

			if ((bin.Right.Flags & ElaExpressionFlags.Assignable) != ElaExpressionFlags.Assignable)
			{
				AddError(ElaCompilerError.UnableAssignExpression, bin.Right);
				err = true;
			}

			if (!err)
			{
				var objLeft = 0;
				var idxLeft = 0;
				var objRight = 0;
				var idxRight = 0;
				CompilePushWithCache(bin.Left, map, out objLeft, out idxLeft);
				CompilePushWithCache(bin.Right, map, out objRight, out idxRight);
				CompilePopWithCache(bin.Left, map, objLeft, idxLeft);
				CompilePopWithCache(bin.Right, map, objRight, idxRight);
			}
		}


		private void CompilePushWithCache(ElaExpression exp, LabelMap map, out int obj, out int idx)
		{
			obj = -1;
			idx = -1;
			var fr = default(ElaFieldReference);

			if (exp.Type == ElaNodeType.FieldReference &&
				(fr = (ElaFieldReference)exp).TargetObject.Type != ElaNodeType.BaseReference &&
				fr.TargetObject.Type != ElaNodeType.VariableReference)
			{
				CompileExpression(fr.TargetObject, map, Hints.None);
				obj = AddVariable();
				cw.Emit(Op.Dup);
				cw.Emit(Op.Popvar, obj);
				AddLinePragma(fr);
				cw.Emit(Op.Pushfld, AddString(fr.FieldName));
			}
			else if (exp.Type == ElaNodeType.Indexer)
			{
				var ind = (ElaIndexer)exp;
				CompileExpression(ind.TargetObject, map, Hints.None);

				if (ind.TargetObject.Type != ElaNodeType.VariableReference)
				{
					obj = AddVariable();
					cw.Emit(Op.Dup);
					cw.Emit(Op.Popvar, obj);
				}

				CompileExpression(ind.Index, map, Hints.None);

				if (ind.Index.Type != ElaNodeType.VariableReference && ind.Index.Type != ElaNodeType.Primitive)
				{
					idx = AddVariable();
					cw.Emit(Op.Dup);
					cw.Emit(Op.Popvar, idx);
				}

				AddLinePragma(ind);
				cw.Emit(Op.Pushelem);
			}
			else
				CompileExpression(exp, map, Hints.None);
		}


		private void CompilePopWithCache(ElaExpression exp, LabelMap map, int obj, int idx)
		{
			if (obj != -1 || idx != -1)
			{
				if (obj != -1)
					cw.Emit(Op.Pushvar, obj);

				if (exp.Type == ElaNodeType.FieldReference)
				{
					var fr = (ElaFieldReference)exp;

					if (obj == -1)
						CompileExpression(fr.TargetObject, map, Hints.None);

					cw.Emit(Op.Popfld, AddString(fr.FieldName));
				}
				else if (exp.Type == ElaNodeType.Indexer)
				{
					var ind = (ElaIndexer)exp;

					if (obj == -1)
						CompileExpression(ind.TargetObject, map, Hints.None);

					if (idx == -1)
						CompileExpression(ind.Index, map, Hints.None);
					else
						cw.Emit(Op.Pushvar, idx);

					cw.Emit(Op.Popelem);
				}
			}
			else
				CompileExpression(exp, map, Hints.Left | Hints.Assign);
		}
		#endregion


		#region Match
		private bool BuildPatternTree(ElaPattern pat, PatternTree tree)
		{
			switch (pat.Type)
			{
				case ElaNodeType.SeqPattern:
				case ElaNodeType.ArrayPattern:
				case ElaNodeType.ListPattern:
				case ElaNodeType.ConstructorPattern:
				case ElaNodeType.HeadTailPattern:
					return BuildSeqPatternTree((ElaSeqPattern)pat, tree);
				case ElaNodeType.AndPattern:
					return BuildPatternTree(((ElaAndPattern)pat).Left, tree);
				case ElaNodeType.OrPattern:
					return BuildPatternTree(((ElaOrPattern)pat).Left, tree);
				case ElaNodeType.AsPattern:
					return BuildPatternTree(((ElaAsPattern)pat).Pattern, tree);
				case ElaNodeType.BoolPattern:
					return BuildPatternTree(((ElaBoolPattern)pat).Right, tree);
				default:
					return BuildGenericPatternTree(pat, tree);
			}
		}


		private bool BuildGenericPatternTree(ElaPattern pat, PatternTree tree)
		{
			if (tree.Affinity == ElaPatternAffinity.Any)
				tree.Affinity = pat.Affinity;
			else if (tree.Affinity != pat.Affinity &&
				pat.Affinity != ElaPatternAffinity.Any)
				return false;

			return true;			
		}


		private bool BuildSeqPatternTree(ElaSeqPattern pat, PatternTree tree)
		{
			if (tree.Affinity == ElaPatternAffinity.Any)
				tree.Affinity = pat.Affinity;
			else if (tree.Affinity != pat.Affinity)
				return false;

			if (pat.HasChildren)
			{
				var c = tree;
				var oldc = default(PatternTree);

				for (var i = 0; i < pat.Patterns.Count; i++)
				{
					if (i == 0)
					{
						oldc = c;
						c = oldc.Child;

						if (c == null)
							c = oldc.Child = new PatternTree();
					}
					else
					{
						oldc = c;
						c = oldc.Next;

						if (c == null)
							c = oldc.Next = new PatternTree();
					}

					if (!BuildPatternTree(pat.Patterns[i], c))
						return false;
				}
			}

			return true;
		}


		private void CompileMatch(ElaMatch exp, LabelMap map, Hints hints)
		{
			var aff = new ElaPatternAffinity[0];
			var serv = AddVariable();
			var tuple = default(ElaTupleLiteral);
			var matchExp = exp.Expression;

			if (matchExp != null)
			{
				if (exp.Expression.Type == ElaNodeType.TupleLiteral)
				{
					tuple = (ElaTupleLiteral)matchExp;

					if (tuple.Parameters.Count == 1)
					{
						matchExp = tuple.Parameters[0];
						tuple = null;
					}
				}

				if (tuple == null)
				{
					CompileExpression(matchExp, map, Hints.None);
					cw.Emit(Op.Popvar, serv);
				}
			}

			var len = exp.Entries.Count;
			var end = cw.DefineLabel();
			var err = cw.DefineLabel();
			var next = Label.Empty;
			var newHints = (hints & Hints.Tail) == Hints.Tail ? Hints.Tail : Hints.None;
			var tree = new PatternTree();

			for (var i = 0; i < len; i++)
			{
				var e = exp.Entries[i];
				
				if (!BuildPatternTree(e.Pattern, tree))
				{
					AddError(ElaCompilerError.MatchEntryTypingFailed, e);
					AddHint(ElaCompilerHint.MatchEntryTypingFailed, e);
				}

				var last = i == len - 1;

				if (!next.IsEmpty())
				{
					cw.MarkLabel(next);
					cw.Emit(Op.Nop);
				}

				StartScope(false);
				next = cw.DefineLabel();

				if ((e.Pattern.Type == ElaNodeType.DefaultPattern || e.Pattern.Type == ElaNodeType.VariablePattern) &&
					e.Pattern.Guard == null && !last)
				{
					AddWarning(ElaCompilerWarning.MatchNextEntryIgnored, e);
					AddHint(ElaCompilerHint.MatchNextEntryIgnored, e);
				}

				CompilePattern(serv, tuple, e.Pattern, map, last ? err : next, ElaVariableFlags.Immutable, -1);

				cw.MarkLabel(next);
				CompileExpression(e.Expression, map, newHints);

				EndScope();
				cw.Emit(Op.Br, end);
			}

			cw.MarkLabel(err);

			if ((hints & Hints.Throw) == Hints.Throw)
			{
				cw.Emit(Op.Pushvar, serv);
				cw.Emit(Op.Throw);
			}
			else
				cw.Emit(Op.Throw, (Int32)ElaRuntimeError.MatchFailed);

			cw.MarkLabel(end);
			cw.Emit(Op.Nop);
		}


		private void MatchEntryAlwaysFail(ElaExpression exp, Label lab)
		{
			AddWarning(ElaCompilerWarning.MatchEntryAlwaysFail, exp);
			cw.Emit(Op.Br, lab);
		}


		private int CompilePattern(int pushSys, ElaTupleLiteral tuple, ElaPattern patExp, LabelMap map, Label nextLab, ElaVariableFlags flags, int decMap)
		{
			var addr = 0;
			var vsum = 0;

			switch (patExp.Type)
			{
				case ElaNodeType.VoidPattern:
					{
						if (tuple != null)
							MatchEntryAlwaysFail(patExp, nextLab);
						else
						{
							var tp = (ElaVoidPattern)patExp;
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Typeof);
							cw.Emit(Op.PushI4, (Int32)ObjectType.Unit);
							cw.Emit(Op.Br_neq, nextLab);
						}
					}
					break;
				case ElaNodeType.BoolPattern:
					{
						if (tuple != null)
							MatchEntryAlwaysFail(patExp, nextLab);
						else
						{
							var grd = (ElaBoolPattern)patExp;
							cw.Emit(Op.Pushvar, pushSys);

							if (grd.Right.Type == ElaNodeType.LiteralPattern)
								PushPrimitive(((ElaLiteralPattern)grd.Right).Value);
							else
							{
								var vn = ((ElaValueofPattern)grd.Right).VariableName;
								var var = GetVariable(vn);

								if (var.IsEmpty())
									AddError(ElaCompilerError.UndefinedVariable, grd.Right, vn);
								else
								{
									cw.Emit(Op.Pushvar, var.Address);
									cw.Emit(Op.Valueof);
								}
							}

							if (grd.Operator == ElaBinaryOperator.Equals)
								cw.Emit(Op.Ceq);
							else if (grd.Operator == ElaBinaryOperator.NotEquals)
								cw.Emit(Op.Cneq);
							else if (grd.Operator == ElaBinaryOperator.Greater)
								cw.Emit(Op.Cgt);
							else if (grd.Operator == ElaBinaryOperator.Lesser)
								cw.Emit(Op.Clt);
							else if (grd.Operator == ElaBinaryOperator.GreaterEqual)
								cw.Emit(Op.Cgteq);
							else if (grd.Operator == ElaBinaryOperator.LesserEqual)
								cw.Emit(Op.Clteq);

							cw.Emit(Op.Brfalse, nextLab);

							if (grd.Left != null)
							{
								var patVar = GetVariable(grd.Left);

								if (patVar.Address != pushSys)
								{
									addr = patVar.IsEmpty() ? AddVariable(grd.Left, grd, flags, -1) : patVar.Address;
									cw.Emit(Op.Pushvar, pushSys);
									cw.Emit(Op.Popvar, addr);
								}
							}
						}
					}
					break;
				case ElaNodeType.OrPattern:
					var orPat = (ElaOrPattern)patExp;
					var next = cw.DefineLabel();
					var oc = currentCounter - 1;
					var lv = CompilePattern(pushSys, tuple, orPat.Left, map, next, flags, -1);
					var mp = oc | (oc + (lv & Int16.MaxValue)) << 16;
					var end = cw.DefineLabel();
					cw.Emit(Op.Br, end);
					cw.MarkLabel(next);
					var rv = CompilePattern(pushSys, tuple, orPat.Right, map, nextLab, flags, mp);
					cw.MarkLabel(end);
					cw.Emit(Op.Nop);
					
					if ((lv & Int16.MaxValue) != (rv >> 16) || (rv & Int16.MaxValue) != 0)
						AddError(ElaCompilerError.MatchOrPatternVariables, orPat);
					break;
				case ElaNodeType.AndPattern:
					var andPat = (ElaAndPattern)patExp;
					CompilePattern(pushSys, tuple, andPat.Left, map, nextLab, flags, -1);
					CompilePattern(pushSys, tuple, andPat.Right, map, nextLab, flags, -1);
					break;
				case ElaNodeType.AsPattern:
					var asPat = (ElaAsPattern)patExp;
					var asVar = GetLocalVariable(asPat.Name);

					if (asVar.IsEmpty())
					{
						addr = AddVariable(asPat.Name, patExp, flags, -1);
						vsum = (vsum & Int16.MaxValue) + 1 | (vsum >> 16) << 16;
					}
					else
					{
						addr = asVar.Address;

						if (decMap > 0)
						{
							var lb = decMap & Int16.MaxValue;
							var ub = decMap >> 16;

							if ((addr >> 8) > lb && (addr >> 8) <= ub)
								vsum = (vsum & Int16.MaxValue) | (vsum >> 16) + 1 << 16;
						}
					}

					if (tuple != null)
					{
						CompileExpression(tuple, map, Hints.None);
						cw.Emit(Op.Popvar, pushSys);
						tuple = null;
					}

					var esum = CompilePattern(pushSys, tuple, asPat.Pattern, map, nextLab, flags, -1);
					vsum = (vsum & Int16.MaxValue) + (esum & Int16.MaxValue) |
						(vsum >> 16) + (esum >> 16) << 16;

					cw.Emit(Op.Pushvar, pushSys);
					cw.Emit(Op.Popvar, addr);
					break;
				case ElaNodeType.LiteralPattern:
					if (tuple != null)
						MatchEntryAlwaysFail(patExp, nextLab);
					else
					{
						cw.Emit(Op.Pushvar, pushSys);
						PushPrimitive(((ElaLiteralPattern)patExp).Value);
						cw.Emit(Op.Br_neq, nextLab);
					}
					break;
				case ElaNodeType.ValueofPattern:
					if (tuple != null)
						MatchEntryAlwaysFail(patExp, nextLab);
					else
					{
						var pt = (ElaValueofPattern)patExp;
						var var = GetVariable(pt.VariableName);

						if (var.IsEmpty())
							AddError(ElaCompilerError.UndefinedVariable, patExp, pt.VariableName);
						else
						{
							cw.Emit(Op.Pushvar, var.Address);
							cw.Emit(Op.Valueof);
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Br_neq, nextLab);
						}
					}
					break;
				case ElaNodeType.IsPattern:
					{
						var tp = (ElaIsPattern)patExp;

						if (tuple != null && tp.TypeAffinity != ObjectType.Tuple)
							MatchEntryAlwaysFail(patExp, nextLab);
						else if (tuple == null)
						{
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Typeof);
							cw.Emit(Op.PushI4, (Int32)tp.TypeAffinity);
							cw.Emit(Op.Br_neq, nextLab);

							if (tp.VariableName != null)
							{
								var patVar = GetLocalVariable(tp.VariableName);
								addr = !patVar.IsEmpty() ? patVar.Address :
									AddVariable(tp.VariableName, tp, flags, -1);
								cw.Emit(Op.Pushvar, pushSys);
								cw.Emit(Op.Popvar, addr);
							}
						}
					}
					break;
				case ElaNodeType.VariablePattern:
					{
						var vexp = (ElaVariablePattern)patExp;
						var patVar = GetLocalVariable(vexp.Name);

						if (tuple != null)
						{
							CompileExpression(tuple, map, Hints.None);
							cw.Emit(Op.Popvar, pushSys);
							tuple = null;
						}

						if (patVar.IsEmpty())
						{
							addr = AddVariable(vexp.Name, vexp, flags, -1);
							vsum = (vsum & Int16.MaxValue) + 1 | (vsum >> 16) << 16;
						}
						else
						{
							addr = patVar.Address;

							if (decMap > 0)
							{
								var lb = decMap & Int16.MaxValue;
								var ub = decMap >> 16;

								if ((addr >> 8) > lb && (addr >> 8) <= ub)
									vsum = (vsum & Int16.MaxValue) | (vsum >> 16) + 1 << 16;
							}
						}

						if (pushSys != addr)
						{
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Popvar, addr);
						}
					}
					break;
				case ElaNodeType.FieldPattern:
					if (tuple != null)
						MatchEntryAlwaysFail(patExp, nextLab);
					else
					{
						var fld = (ElaFieldPattern)patExp;
						addr = AddVariable();
						var str = AddString(fld.Name);
						cw.Emit(Op.Pushvar, pushSys);
						cw.Emit(Op.Hasfld, str);
						cw.Emit(Op.Brfalse, nextLab);

						if (fld.Value != null)
						{
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Pushfld, str);
							cw.Emit(Op.Popvar, addr);
							CompilePattern(addr, null, fld.Value, map, nextLab, flags, -1);
						}
					}
					break;
				case ElaNodeType.ArrayPattern:
				case ElaNodeType.SeqPattern:
					CompileSequencePattern(pushSys, tuple, (ElaSeqPattern)patExp, map, nextLab, flags);
					break;
				case ElaNodeType.DefaultPattern:
					break;
				case ElaNodeType.ConstructorPattern:
					if (tuple != null)
						MatchEntryAlwaysFail(patExp, nextLab);
					else
					{
						var cons = (ElaConstructorPattern)patExp;

						if (cons.Name != null)
						{
							cw.Emit(Op.Pushstr, AddString(cons.Name));
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Ctag);
							cw.Emit(Op.Brfalse, nextLab);
						}
						else
						{
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Htag);
							cw.Emit(Op.Brfalse, nextLab);
						}

						if (cons.HasChildren)
						{
							var newSys = pushSys;
							CompileSequencePattern(pushSys, tuple, cons, map, nextLab, flags);
						}
						else
						{
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Pushfld, AddString("length"));
							cw.Emit(Op.PushI4_0);
							cw.Emit(Op.Br_neq, nextLab);
						}
					}
					break;
				case ElaNodeType.HeadTailPattern:
					if (tuple != null)
						MatchEntryAlwaysFail(patExp, nextLab);
					else
					{
						var pexp = (ElaHeadTailPattern)patExp;
						cw.Emit(Op.Pushvar, pushSys);
						cw.Emit(Op.Len);
						cw.Emit(Op.PushI4_0);
						cw.Emit(Op.Br_eq, nextLab);

						var els = pexp.Patterns;
						var len = els.Count;
						cw.Emit(Op.Pushvar, pushSys);

						for (var i = 0; i < len; i++)
						{
							var e = els[i];
							var isEnd = i == len - 1;

							if (!isEnd && e.Type == ElaNodeType.ListPattern && !((ElaListPattern)e).HasChildren)
								AddError(ElaCompilerError.MatchHeadTailPatternNil, e);
							else
							{
								if (!isEnd)
								{
									cw.Emit(Op.Dup);
									cw.Emit(Op.Listelem);
								}

								if (e.Type == ElaNodeType.VariablePattern)
								{
									var nm = ((ElaVariablePattern)e).Name;
									var patVar = GetLocalVariable(nm);
									var a = patVar.IsEmpty() ? AddVariable(nm, e, flags, -1) : patVar.Address;
									cw.Emit(Op.Popvar, a);
								}
								else if (e.Type == ElaNodeType.DefaultPattern)
									cw.Emit(Op.Pop);
								else
								{
									var newSys = AddVariable();
									cw.Emit(Op.Popvar, newSys);
									CompilePattern(newSys, null, e, map, nextLab, flags, -1);
								}

								if (!isEnd)
									cw.Emit(Op.Listtail);
							}
						}
					}
					break;
				case ElaNodeType.ListPattern:
					if (tuple != null)
						MatchEntryAlwaysFail(patExp, nextLab);
					else
					{
						var pat = (ElaListPattern)patExp;

						if (!pat.HasChildren)
						{
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Len);
							cw.Emit(Op.PushI4_0);
							cw.Emit(Op.Br_neq, nextLab);
						}
						else
						{
							var len = pat.Patterns.Count;

							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Len);
						
							if (len == 0)
								cw.Emit(Op.PushI4_0);
							else
								cw.Emit(Op.PushI4, len);

							cw.Emit(Op.Br_neq, nextLab);
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Dup);
							var mutSys = AddVariable();
							cw.Emit(Op.Popvar, mutSys);

							for (var i = 0; i < len; i++)
							{
								var p = pat.Patterns[i];

								if (i > 0)
								{
									cw.Emit(Op.Pushvar, mutSys);
									cw.Emit(Op.Listtail);
									cw.Emit(Op.Dup);
									cw.Emit(Op.Popvar, mutSys);
								}

								cw.Emit(Op.Listelem);
								var newSys = AddVariable();
								cw.Emit(Op.Popvar, newSys);
								CompilePattern(newSys, null, p, map, nextLab, flags, -1);
							}
						}
					}
					break;
			}

			if (patExp.Guard != null)
			{
				CompileExpression(patExp.Guard, map, Hints.None);
				cw.Emit(Op.Brfalse, nextLab);
			}

			return vsum;
		}



		private void CompileSequencePattern(int pushSys, ElaTupleLiteral tuple, ElaSeqPattern seq, LabelMap map, Label nextLab, ElaVariableFlags flags)
		{
			var len = seq.Patterns.Count;

			if (seq.Type == ElaNodeType.ArrayPattern)
			{
				cw.Emit(Op.Pushvar, pushSys);
				cw.Emit(Op.Typeof);
				cw.Emit(Op.PushI4, (Int32)ObjectType.Array);
				cw.Emit(Op.Br_neq, nextLab);
			}

			if (tuple != null)
			{
				if (tuple.Parameters.Count != len)
				{
					cw.Emit(Op.Br, nextLab);
					AddWarning(ElaCompilerWarning.MatchEntryAlwaysFail, seq);
				}
				else
				{
					for (var i = 0; i < len; i++)
					{
						CompileExpression(tuple.Parameters[i], map, Hints.None);
						var newSys = AddVariable();
						cw.Emit(Op.Popvar, newSys);
						CompilePattern(newSys, null, seq.Patterns[i], map, nextLab, flags, -1);
					}
				}
			}
			else
			{
				if (!seq.IsRecord())
				{
					cw.Emit(Op.Pushvar, pushSys);
					cw.Emit(Op.Len);

					if (len == 0)
						cw.Emit(Op.PushI4_0);
					else
						cw.Emit(Op.PushI4, len);

					cw.Emit(Op.Br_neq, nextLab);
				}

				for (var i = 0; i < len; i++)
				{
					var pat = seq.Patterns[i];

					if (pat.Type == ElaNodeType.FieldPattern)
						CompilePattern(pushSys, null, pat, map, nextLab, flags, -1);
					else
					{
						cw.Emit(Op.Pushvar, pushSys);

						if (i == 0)
							cw.Emit(Op.PushI4_0);
						else
							cw.Emit(Op.PushI4, i);

						cw.Emit(Op.Pushelem);
						var newSys = AddVariable();
						cw.Emit(Op.Popvar, newSys);
						CompilePattern(newSys, null, pat, map, nextLab, flags, -1);
					}
				}
			}
		}
		#endregion
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
		#endregion


		#region Debug
		private void StartFun(string name, int handle, int pars)
		{
			pdb.StartFunction(name, handle, cw.Offset, pars);
		}


		private void EndFun()
		{
			pdb.EndFunction(cw.Offset);
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
		private void AddOperator(ElaVariableDeclaration s, LabelMap map, Hints hints)
		{
			var addr = 0;

			if ((addr = AddPervasive(s.VariableName)) == -1)
				AddError(ElaCompilerError.OperatorAlreadyDeclared, s, s.VariableName);

			if (CurrentScope.Parent != null)
				AddError(ElaCompilerError.OperatorOnlyInGlobal, s);

			if ((s.VariableFlags & ElaVariableFlags.Immutable) != ElaVariableFlags.Immutable)
				AddWarning(ElaCompilerWarning.OperatorAsMutable, s);

			CompileExpression(s.InitExpression, map, Hints.None);
			AddLinePragma(s);
			cw.Emit(Op.Popvar, addr);

			if ((hints & Hints.Left) != Hints.Left)
				cw.Emit(Op.Pushunit);
		}


		private int AddVariable()
		{
			var ret = 0 | currentCounter << 8;
			currentCounter++;
			return ret;
		}


		private int AddPervasive(string name)
		{
			if (!frame.Pervasives.ContainsKey(name))
			{
				var addr = AddVariable();
				frame.Pervasives.Add(name, addr);
				return addr;
			}
			else
				return -1;
		}


		private int AddVariable(string name, ElaExpression exp, ElaVariableFlags flags, int data)
		{
			if (IsRegistered(name))
			{
				AddError(ElaCompilerError.VariableAlreadyDeclared, exp, name);
				return -1;
			}

			CurrentScope.Locals.Add(name, new ScopeVar(flags, currentCounter, data));
			AddVarPragma(name, currentCounter, cw.Offset);
			return AddVariable();
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