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

			if ((hints & Hints.Left) == Hints.Left && bin.Operator != ElaOperator.Assign)
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
				case ElaOperator.Sequence:
					CompileExpression(bin.Left, map, Hints.None);
					cw.Emit(Op.Force);
					cw.Emit(Op.Pop);
					CompileExpression(bin.Right, map, hints);
					break;
			}
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