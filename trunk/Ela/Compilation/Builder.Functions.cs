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

			if (flag != FunFlag.Inline)
				StartFun(dec.Name, pars);

			var funSkipLabel = Label.Empty;

			var map = new LabelMap();
			var startLabel = cw.DefineLabel();

			if (flag != FunFlag.Inline)
			{
				funSkipLabel = cw.DefineLabel();
				cw.Emit(Op.Br, funSkipLabel);
			}

			map.FunStart = startLabel;
			map.FunctionName = dec.Name;
			map.FunctionParameters = pars;
			map.FunctionScope = CurrentScope;
			map.InlineFunction = flag == FunFlag.Inline;

			if (flag != FunFlag.Inline)
				cw.MarkLabel(startLabel);

			StartScope(flag != FunFlag.Inline);

			if (flag != FunFlag.Inline)
				StartSection();
			
			AddLinePragma(dec);

			var address = cw.Offset;
			
			if (dec.IsTemplate)
			{
				for (var i = 0; i < dec.TemplateParameters.Count; i++)
					AddVariable(dec.TemplateParameters[i], dec, ElaVariableFlags.Template, i);
			}
			
			if (flag != FunFlag.Lazy)
				CompileParameters(dec, map);

			if (dec.Body.Entries.Count > 1)
			{
				var t = (ElaTupleLiteral)dec.Body.Expression;
				var ex = t.Parameters.Count == 1 ? t.Parameters[0] : t;
				CompileMatch(dec.Body, ex, map, Hints.Tail | Hints.Scope | Hints.FunBody);
			}
			else
			{
				if (dec.Body.Entries[0].Where != null)
					CompileExpression(dec.Body.Entries[0].Where, map, Hints.Left);

				CompileExpression(dec.Body.Entries[0].Expression, map, Hints.Tail | Hints.Scope);
			}

			if (flag != FunFlag.Inline)
			{
				var funHandle = frame.Layouts.Count;
				var ss = EndFun(funHandle);
				frame.Layouts.Add(new MemoryLayout(currentCounter, ss, address));
				EndScope();
				EndSection();

				cw.Emit(Op.Ret);
				cw.MarkLabel(funSkipLabel);

				AddLinePragma(dec);

				if (dec.IsTemplate)
				{
					cw.Emit(Op.PushI4, dec.TemplateParameters.Count);
					cw.Emit(Op.PushI4, pars);
					cw.Emit(Op.Newfunt, funHandle);
				}
				else
				{
					cw.Emit(Op.PushI4, pars);
					cw.Emit(Op.Newfun, funHandle);
				}
			}
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
							AddVariable(((ElaVariablePattern)p).Name, p, ElaVariableFlags.Parameter, -1);
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
							CompilePattern(addr, null, e, map, nextLab, ElaVariableFlags.None, Hints.FunBody);
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
						(ElaVariableFlags.SpecialName | ElaVariableFlags.Parameter) : ElaVariableFlags.Parameter, -1);
					cw.Emit(Op.Popvar, addr);
				}
			}
		}
		#endregion


		#region Builtins
        private void CompileBuiltin(ElaBuiltinKind kind, ElaExpression exp, LabelMap map)
		{
			StartSection();
			var pars = Builtins.Params(kind);
			cw.StartFrame(pars);
			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);
			var address = cw.Offset;
			CompileBuiltinInline(kind, exp, map, Hints.None);

			cw.Emit(Op.Ret);
			frame.Layouts.Add(new MemoryLayout(currentCounter, cw.FinishFrame(), address));
			EndSection();

			cw.MarkLabel(funSkipLabel);
			cw.Emit(Op.PushI4, pars);
			cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
		}


		private void CompileBuiltinInline(ElaBuiltinKind kind, ElaExpression exp, LabelMap map, Hints hints)
		{
			switch (kind)
			{
                case ElaBuiltinKind.Fst:
                    cw.Emit(Op.Elem, 2 | 0 << 8);
                    break;
                case ElaBuiltinKind.Snd:
                    cw.Emit(Op.Elem, 2 | 1 << 8);
                    break;
                case ElaBuiltinKind.Fst3:
                    cw.Emit(Op.Elem, 3 | 0 << 8);
                    break;
                case ElaBuiltinKind.Snd3:
                    cw.Emit(Op.Elem, 3 | 1 << 8);
                    break;
                case ElaBuiltinKind.Head:
                    cw.Emit(Op.Head);
                    break;
                case ElaBuiltinKind.Tail:
                    cw.Emit(Op.Tail);
                    break;
                case ElaBuiltinKind.IsNil:
                    cw.Emit(Op.Isnil);
                    break;
				case ElaBuiltinKind.Negate:
					cw.Emit(Op.Neg);
					break;
				case ElaBuiltinKind.Succ:
					cw.Emit(Op.Succ);
					break;
				case ElaBuiltinKind.Pred:
					cw.Emit(Op.Pred);
					break;
				case ElaBuiltinKind.Max:
					cw.Emit(Op.Max);
					break;
				case ElaBuiltinKind.Min:
					cw.Emit(Op.Min);
					break;
				case ElaBuiltinKind.Type:
					cw.Emit(Op.Type);
					break;
				case ElaBuiltinKind.Length:
					cw.Emit(Op.Len);
					break;
				case ElaBuiltinKind.Force:
					cw.Emit(Op.Force);
					break;
				case ElaBuiltinKind.Typeid:
					cw.Emit(Op.Typeid);
					break;
				case ElaBuiltinKind.Not:
					cw.Emit(Op.Not);
					break;
				case ElaBuiltinKind.Flip:
					cw.Emit(Op.Flip);
					break;
				case ElaBuiltinKind.Nil:
					cw.Emit(Op.Nil);
					break;
				case ElaBuiltinKind.Show:
					cw.Emit(Op.Pushstr_0);
					cw.Emit(Op.Show);
					break;
				case ElaBuiltinKind.Showf:
					cw.Emit(Op.Show);
					break;
                case ElaBuiltinKind.IsRef:
                    cw.Emit(Op.Ceqref);
                    break;
				case ElaBuiltinKind.CompBackward:
					cw.Emit(Op.Swap);
					CompileComposition(null, map, hints);
					break;
				case ElaBuiltinKind.CompForward:
					CompileComposition(null, map, hints);
					break;
				case ElaBuiltinKind.Concat:					
					cw.Emit(Op.Concat);
					break;
				case ElaBuiltinKind.Add:
					cw.Emit(Op.Add);
					break;
				case ElaBuiltinKind.Divide:
					cw.Emit(Op.Div);
					break;
				case ElaBuiltinKind.Multiply:
					cw.Emit(Op.Mul);
					break;
				case ElaBuiltinKind.Power:
					cw.Emit(Op.Pow);
					break;
				case ElaBuiltinKind.Remainder:
					cw.Emit(Op.Rem);
					break;
				case ElaBuiltinKind.Subtract:
					cw.Emit(Op.Sub);
					break;
				case ElaBuiltinKind.ShiftRight:
					cw.Emit(Op.Shr);
					break;
				case ElaBuiltinKind.ShiftLeft:
					cw.Emit(Op.Shl);
					break;
				case ElaBuiltinKind.Greater:
					cw.Emit(Op.Cgt);
					break;
				case ElaBuiltinKind.Lesser:
					cw.Emit(Op.Clt);
					break;
				case ElaBuiltinKind.Equals:
					cw.Emit(Op.Ceq);
					break;
				case ElaBuiltinKind.NotEquals:
					cw.Emit(Op.Cneq);
					break;
				case ElaBuiltinKind.GreaterEqual:
					cw.Emit(Op.Cgteq);
					break;
				case ElaBuiltinKind.LesserEqual:
					cw.Emit(Op.Clteq);
					break;
				case ElaBuiltinKind.BitwiseAnd:
					cw.Emit(Op.AndBw);
					break;
				case ElaBuiltinKind.BitwiseOr:
					cw.Emit(Op.OrBw);
					break;
				case ElaBuiltinKind.BitwiseXor:
					cw.Emit(Op.Xor);
					break;
				case ElaBuiltinKind.Cons:
					cw.Emit(Op.Cons);
					break;
				case ElaBuiltinKind.BitwiseNot:
					cw.Emit(Op.NotBw);
					break;

			}
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
			var scopeVar = GetVariable(varRef.VariableName, varRef.Line, varRef.Column);
			var len = funCall.Parameters.Count;

			if ((scopeVar.VariableFlags & ElaVariableFlags.Function) != ElaVariableFlags.Function ||
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
			var addr = AddVariable(exp.Name, exp, ElaVariableFlags.Parameter, -1);
			cw.Emit(Op.Popvar, addr);
		}
		#endregion
	}
}