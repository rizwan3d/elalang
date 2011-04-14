using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Main
		private ExprData CompileBinary(ElaBinary bin, LabelMap map, Hints hints)
		{
			CompileBinaryMain(bin.Operator, bin, map, hints);

			if ((hints & Hints.Left) == Hints.Left &&
				bin.Operator != ElaOperator.Assign && bin.Operator != ElaOperator.Swap)
				AddValueNotUsed(bin);

			return ExprData.Empty;
		}


		private void CompileBinaryMain(ElaOperator op, ElaBinary bin, LabelMap map, Hints hints)
		{
			var exitLab = default(Label);
			var termLab = default(Label);
			var ut = Untail(hints);

			switch (op)
			{
				case ElaOperator.BooleanAnd:
					CompileExpression(bin.Left, map, ut);
					termLab = cw.DefineLabel();
					exitLab = cw.DefineLabel();
					cw.Emit(Op.Brfalse, termLab);
					CompileExpression(bin.Right, map, ut);
					cw.Emit(Op.Br, exitLab);
					cw.MarkLabel(termLab);
					cw.Emit(Op.PushI1_0);
					cw.MarkLabel(exitLab);
					cw.Emit(Op.Nop);
					break;
				case ElaOperator.BooleanOr:
					CompileExpression(bin.Left, map, ut);
					termLab = cw.DefineLabel();
					exitLab = cw.DefineLabel();
					cw.Emit(Op.Brtrue, termLab);
					CompileExpression(bin.Right, map, ut);
					cw.Emit(Op.Br, exitLab);
					cw.MarkLabel(termLab);
					cw.Emit(Op.PushI1_1);
					cw.MarkLabel(exitLab);
					cw.Emit(Op.Nop);
					break;
				case ElaOperator.Assign:
					CompileAssign(bin, bin.Left, bin.Right, ut, map);
					break;
				case ElaOperator.Swap:
					CompileSwap(bin, map, ut);
					break;
				case ElaOperator.Sequence:
					CompileExpression(bin.Left, map, Hints.None);
					cw.Emit(Op.Force);
					cw.Emit(Op.Pop);
					CompileExpression(bin.Right, map, hints);
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

			if (exp.Type == ElaNodeType.FieldReference && (fr = (ElaFieldReference)exp).TargetObject.Type != ElaNodeType.VariableReference)
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
                    var svar = GetVariable(vr.VariableName, vr.Line, vr.Column);
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
                        var svar = GetVariable(vr.VariableName, vr.Line, vr.Column);
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
	}
}