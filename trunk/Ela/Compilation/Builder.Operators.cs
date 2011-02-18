using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Main
		private void CompileUnary(ElaUnary un, LabelMap map, Hints hints)
		{
			CompileExpression(un.Expression, map, Hints.None);
			AddLinePragma(un);

			if (un.Operator == ElaUnaryOperator.Negate)
				cw.Emit(Op.Neg);
			else if (un.Operator == ElaUnaryOperator.BitwiseNot)
				cw.Emit(Op.NotBw);

			if ((hints & Hints.Left) == Hints.Left)
				AddValueNotUsed(un);
		}


		private ExprData CompileBinary(ElaBinary bin, LabelMap map, Hints hints)
		{
			if (bin.Operator == ElaOperator.Sequence)
				return CompileSequence(bin, map, hints);

			var exprData = ExprData.Empty;
			var newHints =
				((hints & Hints.Scope) == Hints.Scope ? Hints.Scope : Hints.None) |
				((hints & Hints.Nested) == Hints.Nested ? Hints.Nested : Hints.None);

			if (bin.Operator != ElaOperator.CompBackward && bin.Operator != ElaOperator.CompForward && bin.Operator != ElaOperator.Assign)
			{
				if (bin.Left != null &&
					(bin.Left.Flags & ElaExpressionFlags.ReturnsUnit) == ElaExpressionFlags.ReturnsUnit)
					AddWarning(ElaCompilerWarning.UnitAlwaysFail, bin.Left);

				if (bin.Right != null &&
					(bin.Right.Flags & ElaExpressionFlags.ReturnsUnit) == ElaExpressionFlags.ReturnsUnit)
					AddWarning(ElaCompilerWarning.UnitAlwaysFail, bin.Right);
			}

			if (bin.Left != null &&
				(bin.Left.Flags & ElaExpressionFlags.BreaksExecution) == ElaExpressionFlags.BreaksExecution)
				AddError(ElaCompilerError.BreakExecutionNotAllowed, bin.Left);
			else if (bin.Right != null &&
				(bin.Right.Flags & ElaExpressionFlags.BreaksExecution) == ElaExpressionFlags.BreaksExecution)
				AddError(ElaCompilerError.BreakExecutionNotAllowed, bin.Right);
			else if (bin.Operator == ElaOperator.CompBackward ||
				bin.Operator == ElaOperator.CompForward)
			{
				exprData = CompileComposition(bin, map, newHints);

				if ((hints & Hints.Left) == Hints.Left)
					AddValueNotUsed(bin);
			}
			else if (bin.Operator == ElaOperator.Swap)
			{
				CompileSwap(bin, map, hints);
			}
			else if (bin.Operator == ElaOperator.Assign)
			{
				CompileAssign(bin, bin.Left, bin.Right, hints, map);
			}
			else
			{
				if (bin.Operator != ElaOperator.Custom)
				{
					CompileExpression(bin.Left, map, newHints);

					if (bin.Operator != ElaOperator.BooleanAnd && bin.Operator != ElaOperator.BooleanOr)
						CompileExpression(bin.Right, map, newHints);
				}
				else
				{
					if (bin.Right != null)
						CompileExpression(bin.Right, map, newHints );

					if (bin.Left != null)
						CompileExpression(bin.Left, map, newHints);
				}

				CompileBinaryMain(bin.Operator, bin, map, newHints);

				if ((hints & Hints.Left) == Hints.Left)
					AddValueNotUsed(bin);
			}

			return exprData;
		}


		private ExprData CompileSequence(ElaBinary bin, LabelMap map, Hints hints)
		{
			CompileExpression(bin.Left, map, Hints.None);
			cw.Emit(Op.Pop);
			return CompileExpression(bin.Right, map, hints);
		}


		private void CompileBinaryMain(ElaOperator op, ElaBinary bin, LabelMap map, Hints hints)
		{
			var exitLab = default(Label);
			var termLab = default(Label);

			switch (op)
			{
				case ElaOperator.BooleanAnd:
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
				case ElaOperator.BooleanOr:
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
				case ElaOperator.Custom:
					ReferencePervasive(bin.CustomOperator);
					var partial = bin.Right == null || bin.Left == null;

					if (partial)
					{
						if (bin.Left == null)
							cw.Emit(Op.Flip);

						cw.Emit(Op.Call);
					}
					else
					{
						cw.Emit(Op.Call);
						cw.Emit(Op.Call);
					}
					break;
				default:
					CompileSimpleBinary(bin.Operator);
					break;
			}
		}


		private void CompileSimpleBinary(ElaOperator op)
		{
			switch (op)
			{
				case ElaOperator.Concat:
					cw.Emit(Op.Concat);
					break;
				case ElaOperator.Add:
					cw.Emit(Op.Add);
					break;
				case ElaOperator.Divide:
					cw.Emit(Op.Div);
					break;
				case ElaOperator.Multiply:
					cw.Emit(Op.Mul);
					break;
				case ElaOperator.Power:
					cw.Emit(Op.Pow);
					break;
				case ElaOperator.Modulus:
					cw.Emit(Op.Rem);
					break;
				case ElaOperator.Subtract:
					cw.Emit(Op.Sub);
					break;
				case ElaOperator.ShiftRight:
					cw.Emit(Op.Shr);
					break;
				case ElaOperator.ShiftLeft:
					cw.Emit(Op.Shl);
					break;
				case ElaOperator.Greater:
					cw.Emit(Op.Cgt);
					break;
				case ElaOperator.Lesser:
					cw.Emit(Op.Clt);
					break;
				case ElaOperator.Equals:
					cw.Emit(Op.Ceq);
					break;
				case ElaOperator.NotEquals:
					cw.Emit(Op.Cneq);
					break;
				case ElaOperator.GreaterEqual:
					cw.Emit(Op.Cgteq);
					break;
				case ElaOperator.LesserEqual:
					cw.Emit(Op.Clteq);
					break;
				case ElaOperator.BitwiseAnd:
					cw.Emit(Op.AndBw);
					break;
				case ElaOperator.BitwiseOr:
					cw.Emit(Op.OrBw);
					break;
				case ElaOperator.BitwiseXor:
					cw.Emit(Op.Xor);
					break;
				case ElaOperator.ConsList:
					cw.Emit(Op.Cons);
					break;
			}
		}


		private void CompileSwap(ElaBinary bin, LabelMap map, Hints hints)
		{
			var err = false;

			if ((bin.Left.Flags & ElaExpressionFlags.Assignable) != ElaExpressionFlags.Assignable)
			{
				AddError(ElaCompilerError.UnableAssignExpression, bin.Left, FormatNode(bin.Left));
				err = true;
			}

			if ((bin.Right.Flags & ElaExpressionFlags.Assignable) != ElaExpressionFlags.Assignable)
			{
				AddError(ElaCompilerError.UnableAssignExpression, bin.Right, FormatNode(bin.Right));
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

			if ((hints & Hints.Left) != Hints.Left)
				cw.Emit(Op.Pushunit);
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

				if (ind.Index.Type == ElaNodeType.VariableReference)
				{
					var vr = (ElaVariableReference)ind.Index;
					var svar = GetVariable(vr.VariableName);

					if (svar.IsEmpty())
						AddError(ElaCompilerError.UndefinedVariable, vr, vr.VariableName);

					AddLinePragma(ind);
					cw.Emit(Op.Pushelemi, svar.Address);
				}
				else
				{
					CompileExpression(ind.Index, map, Hints.None);

					if (ind.Index.Type != ElaNodeType.Primitive)
					{
						idx = AddVariable();
						cw.Emit(Op.Dup);
						cw.Emit(Op.Popvar, idx);
					}

					AddLinePragma(ind);
					cw.Emit(Op.Pushelem);
				}
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

					if (ind.Index.Type == ElaNodeType.VariableReference)
					{
						var vr = (ElaVariableReference)ind.Index;
						var svar = GetVariable(vr.VariableName);

						if (svar.IsEmpty())
							AddError(ElaCompilerError.UndefinedVariable, vr, vr.VariableName);

						AddLinePragma(ind);
						cw.Emit(Op.Popelemi, svar.Address);
					}
					else
					{
						if (idx == -1)
						{
							CompileExpression(ind.Index, map, Hints.None);
							cw.Emit(Op.Popelem);
						}
						else
							cw.Emit(Op.Popelemi, idx);
					}
				}
			}
			else
				CompileExpression(exp, map, Hints.Left | Hints.Assign);
		}


		private void CompileAssign(ElaExpression exp, ElaExpression left, ElaExpression right, Hints hints, LabelMap map)
		{
			CompileExpression(right, map, Hints.None);

			if ((left.Flags & ElaExpressionFlags.Assignable) != ElaExpressionFlags.Assignable)
				AddError(ElaCompilerError.UnableAssignExpression, left, FormatNode(left));
			else
				CompileExpression(left, map, Hints.Assign | Hints.Left);

			if ((hints & Hints.Left) != Hints.Left)
				cw.Emit(Op.Pushunit);
		}		
		#endregion


		#region Service
		private void ReferencePervasive(string name)
		{
			var addr = 0;

			if (frame.DeclaredPervasives.TryGetValue(name, out addr))
				cw.Emit(Op.Pushvar, ReferenceGlobal(addr));
			else
			{
				var hdl = 0;

				if (!frame.ReferencedPervasives.TryGetValue(name, out hdl))
				{
					hdl = frame.ReferencedPervasives.Count;
					frame.ReferencedPervasives.Add(name, hdl);
				}

				cw.Emit(Op.Pushperv, hdl);
			}
		}
		#endregion
	}
}