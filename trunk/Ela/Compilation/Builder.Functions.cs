using System;
using Ela.CodeModel;
using Ela.Runtime;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Main
		private void CompileFunction(ElaFunctionLiteral dec, FunFlag flag)
		{
			var pars = dec.ParameterCount;
			StartFun(dec.Name, pars);

			var map = new LabelMap(cw.DefineLabel());
			var funSkipLabel = cw.DefineLabel();
			var startLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);

			map.FunStart = startLabel;
			map.FunctionName = dec.Name;
			map.FunctionParameters = pars;
			map.FunctionScope = CurrentScope;
			cw.MarkLabel(startLabel);

			StartScope(true);
			StartSection();
			AddLinePragma(dec);

			var address = cw.Offset;

			if (flag != FunFlag.Lazy)
				CompileParameters(dec, map);

			var expr = dec.Body.Entries.Count > 1 ? null : dec.Body.Entries[0].Expression;

			if (dec.Body.Entries.Count > 1)
				CompileMatch(dec.Body, dec.Body.Expression, map, Hints.Tail | Hints.Scope | Hints.FunBody);
			else
			{
				if (dec.Body.Entries[0].Where != null)
					CompileExpression(dec.Body.Entries[0].Where, map, Hints.Left);

				CompileExpression(dec.Body.Entries[0].Expression, map, Hints.Tail | Hints.Scope);
			}

			var funHandle = frame.Layouts.Count;
			var ss = EndFun(funHandle);
			frame.Layouts.Add(new MemoryLayout(currentCounter, ss, address));
			EndScope();
			EndSection();

			cw.MarkLabel(map.Exit);
			cw.Emit(Op.Ret);
			cw.MarkLabel(funSkipLabel);

			AddLinePragma(dec);

			cw.Emit(Op.PushI4, pars);
			cw.Emit(Op.Newfun, funHandle);
		}


		private void CompileParameters(ElaFunctionLiteral fun, LabelMap map)
		{
			if (fun.Body.Entries.Count == 1)
			{
				var fe = fun.Body.Entries[0];
				var seq = fe.Pattern.Type == ElaNodeType.PatternGroup ? (ElaPatternGroup)fe.Pattern : null;
				var parCount = seq != null ? seq.Patterns.Count : 1;
				var tupPat = default(ElaTuplePattern);

				if (parCount == 1 && fe.Pattern.Type == ElaNodeType.TuplePattern && fe.Guard == null &&
					(tupPat = (ElaTuplePattern)fe.Pattern).IsSimple())
				{
					cw.Emit(Op.Tupex, tupPat.Patterns.Count);

					for (var i = 0; i < tupPat.Patterns.Count; i++)
					{
						var p = tupPat.Patterns[i];

						if (p.Type == ElaNodeType.DefaultPattern)
							AddVariable();
						else
							AddVariable(((ElaVariablePattern)p).Name, p,
								ElaVariableFlags.Immutable | ElaVariableFlags.Parameter, -1);
					}
				}
				else
				{
					var ng = fe.Guard == null;

					for (var i = 0; i < parCount; i++)
					{
						var e = parCount > 1 ? seq.Patterns[i] : fe.Pattern;

						if (ng && e.Type == ElaNodeType.VariablePattern)
							AddParameter((ElaVariablePattern)e);
						else if (ng && e.Type == ElaNodeType.DefaultPattern)
							cw.Emit(Op.Pop);
						else
						{
							var addr = AddVariable();
							cw.Emit(Op.Popvar, addr);
							var nextLab = cw.DefineLabel();
							var skipLab = cw.DefineLabel();
							CompilePattern(addr, null, e, map, nextLab, ElaVariableFlags.Immutable, Hints.FunBody);
							cw.Emit(Op.Br, skipLab);
							cw.MarkLabel(nextLab);
							cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
							cw.MarkLabel(skipLab);
							cw.Emit(Op.Nop);
						}
					}

					if (fe.Guard != null)
					{
						var next = cw.DefineLabel();
						CompileExpression(fe.Guard, map, Hints.Scope);
						cw.Emit(Op.Brtrue, next);
						cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
						cw.MarkLabel(next);
						cw.Emit(Op.Nop);
					}
				}
			}
			else
			{
				var t = (ElaTupleLiteral)fun.Body.Expression;

				for (var i = 0; i < t.Parameters.Count; i++)
				{
					var v = (ElaVariableReference)t.Parameters[i];
					var addr = AddVariable(v.VariableName, v,
						v.VariableName[0] == '$' ?
						(ElaVariableFlags.SpecialName | ElaVariableFlags.Parameter) :
						(ElaVariableFlags.Immutable | ElaVariableFlags.Parameter), -1);
					cw.Emit(Op.Popvar, addr);
				}
			}
		}
		#endregion


		#region Builtins
		private void CompileBuiltin(ElaBuiltinFunction fun, LabelMap map)
		{
			StartSection();
			cw.StartFrame(fun.ParameterCount);
			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);
			var address = cw.Offset;
			var pars = CompileBuiltinInline(fun, map, Hints.None);

			cw.Emit(Op.Ret);
			frame.Layouts.Add(new MemoryLayout(currentCounter, cw.FinishFrame(), address));
			EndSection();

			cw.MarkLabel(funSkipLabel);
			cw.Emit(Op.PushI4, pars);
			cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
		}


		private int CompileBuiltinInline(ElaBuiltinFunction fun, LabelMap map, Hints hints)
		{
			var pars = 1;

			switch (fun.Kind)
			{
				case ElaBuiltinFunctionKind.Negate:
					cw.Emit(Op.Neg);
					break;
				case ElaBuiltinFunctionKind.Succ:
					cw.Emit(Op.Succ);
					break;
				case ElaBuiltinFunctionKind.Pred:
					cw.Emit(Op.Pred);
					break;
				case ElaBuiltinFunctionKind.Max:
					cw.Emit(Op.Max);
					break;
				case ElaBuiltinFunctionKind.Min:
					cw.Emit(Op.Min);
					break;
				case ElaBuiltinFunctionKind.Type:
					cw.Emit(Op.Type);
					break;
				case ElaBuiltinFunctionKind.Length:
					cw.Emit(Op.Len);
					break;
				case ElaBuiltinFunctionKind.Force:
					cw.Emit(Op.Force);
					break;
				case ElaBuiltinFunctionKind.Typeid:
					cw.Emit(Op.Typeid);
					break;
				case ElaBuiltinFunctionKind.Cout:
					cw.Emit(Op.Cout);
					cw.Emit(Op.Pushunit);
					break;
				case ElaBuiltinFunctionKind.Not:
					cw.Emit(Op.Not);
					break;
				case ElaBuiltinFunctionKind.Flip:
					cw.Emit(Op.Flip);
					break;
				case ElaBuiltinFunctionKind.Nil:
					cw.Emit(Op.Nil);
					break;
				case ElaBuiltinFunctionKind.Show:
					cw.Emit(Op.Pushstr_0);
					cw.Emit(Op.Show);
					break;
				case ElaBuiltinFunctionKind.Showf:
					pars = 2;
					cw.Emit(Op.Show);
					break;
                case ElaBuiltinFunctionKind.Ref:
                    pars = 2;
                    cw.Emit(Op.Ceqref);
                    break;
				case ElaBuiltinFunctionKind.Operator:
					pars = 2;

					if (fun.Operator == ElaOperator.CompBackward)
					{
						cw.Emit(Op.Swap);
						CompileComposition(null, map, hints);
					}
					else if (fun.Operator == ElaOperator.CompForward)
					{
						CompileComposition(null, map, hints);
					}
					else
					{
						cw.Emit(Op.Swap);
						CompileSimpleBinary(fun.Operator);
					}
					break;
				case ElaBuiltinFunctionKind.Bitnot:
					cw.Emit(Op.NotBw);
					break;

			}

			return pars;
		}
		#endregion


		#region Lazy
		private bool TryOptimizeLazy(ElaLazyLiteral lazy, LabelMap map, Hints hints)
		{
			var body = default(ElaExpression);

			if ((body = lazy.Body.Entries[0].Expression).Type != ElaNodeType.FunctionCall)
				return false;

			var funCall = (ElaFunctionCall)body;

			if (funCall.Target.Type != ElaNodeType.VariableReference)
				return false;

			var varRef = (ElaVariableReference)funCall.Target;
			var scopeVar = GetVariable(varRef.VariableName);
			var len = funCall.Parameters.Count;

			if (scopeVar.IsEmpty() || (scopeVar.VariableFlags & ElaVariableFlags.Function) != ElaVariableFlags.Function ||
				(scopeVar.Data != funCall.Parameters.Count && map.FunctionName == null && map.FunctionName != funCall.GetName() &&
				map.FunctionParameters != len && map.FunctionScope != GetScope(map.FunctionName)))
				return false;

			for (var i = 0; i < len; i++)
				CompileExpression(funCall.Parameters[len - i - 1], map, Hints.None);

			var sl = len - 1;
			AddLinePragma(varRef);
			cw.Emit(Op.Pushvar, scopeVar.Address);

			for (var i = 0; i < sl; i++)
				cw.Emit(Op.Call);

			AddLinePragma(lazy);
			cw.Emit(Op.LazyCall, len);
			return true;
		}
		#endregion


		#region Composition
		private ExprData CompileComposition(ElaBinary bin, LabelMap map, Hints hints)
		{
			var exprData = ExprData.Empty;
			var firstData = bin != null ? CompileExpression(bin.Left, map, hints | Hints.Nested) : ExprData.Empty;
			var first = AddVariable();
			var funPars = (hints & Hints.Nested) == Hints.Nested ? 1 :
				firstData.Type == DataKind.FunParams || firstData.Type == DataKind.FunCurry ? firstData.Data : -1;
			cw.Emit(Op.Popvar, first);

			var secondData = bin != null ? CompileExpression(bin.Right, map, hints | Hints.Nested) : ExprData.Empty;
			var second = AddVariable();
			cw.Emit(Op.Popvar, second);

			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);

			StartSection();
			var address = cw.Offset;

			cw.Emit(Op.Pushvar, 1 | (first >> 8) << 8);

			if (funPars > 1)
				AddWarning(ElaCompilerWarning.FunctionImplicitPartial, bin);

			cw.Emit(Op.Call);

			cw.Emit(Op.Pushvar, 1 | (second >> 8) << 8);
			cw.Emit(Op.Call);
			cw.Emit(Op.Ret);

			if ((secondData.Type == DataKind.FunParams || secondData.Type == DataKind.FunCurry) &&
				secondData.Data > 1)
				AddWarning(ElaCompilerWarning.FunctionImplicitPartial, bin);

			frame.Layouts.Add(new MemoryLayout(currentCounter, 3, address));
			EndSection();
			cw.MarkLabel(funSkipLabel);

			if (funPars > -1)
			{
				cw.Emit(Op.PushI4, funPars);

				if (bin != null)
					AddLinePragma(bin);

				cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
			}
			else
			{
				cw.Emit(Op.PushI4, 1);

				if (bin != null)
					AddLinePragma(bin);

				cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
			}

			if (firstData.Type == DataKind.FunCurry || firstData.Type == DataKind.FunParams)
				exprData = new ExprData(DataKind.FunParams, firstData.Data);

			return exprData;
		}
		#endregion


		#region Helper
		private void AddParameter(ElaVariablePattern exp)
		{
			var addr = AddVariable(exp.Name, exp, ElaVariableFlags.Immutable | ElaVariableFlags.Parameter, -1);
			cw.Emit(Op.Popvar, addr);
		}
		#endregion
	}
}