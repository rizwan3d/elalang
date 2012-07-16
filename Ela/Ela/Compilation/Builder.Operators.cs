using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part is responsible for compilation of Ela operators.
    //All entities treated as operators are special forms (not functions).
    //Ela has only three of them - logical AND, logical OR and sequencing ($).
    //Plus conditional if-then-else operator.
	internal sealed partial class Builder
	{
        //Compile conditional if-then-else operator
        private void CompileConditionalOperator(ElaCondition s, LabelMap map, Hints hints)
        {
            AddLinePragma(s);
            CompileExpression(s.Condition, map, Hints.Scope);
            var falseLab = cw.DefineLabel();
            cw.Emit(Op.Brfalse, falseLab);

            //Both the True and False parts may be the tail expressions
            //Also this whole operator can be used as a statement. Or can be compiled
            //in a situation when some of the referenced names are not initialized (Lazy)
            var left = (hints & Hints.Left) == Hints.Left ? Hints.Left : Hints.None;
            var tail = (hints & Hints.Tail) == Hints.Tail ? Hints.Tail : Hints.None;
            var lazy = (hints & Hints.Lazy) == Hints.Lazy ? Hints.Lazy : Hints.None;
            
            if (s.True != null)
                CompileExpression(s.True, map, left | lazy | tail | Hints.Scope);

            if (s.False != null)
            {
                var skipLabel = cw.DefineLabel();
                cw.Emit(Op.Br, skipLabel);
                cw.MarkLabel(falseLab);
                CompileExpression(s.False, map, left | lazy | tail | Hints.Scope);
                cw.MarkLabel(skipLabel);
                cw.Emit(Op.Nop);
            }
            else
            {
                AddError(ElaCompilerError.ElseMissing, s.True);
                AddHint(ElaCompilerHint.AddElse, s.True);
            }
        }

        //Compile one of three binary operators
		private void CompileBinary(ElaBinary bin, LabelMap map, Hints hints)
		{
		    var op = bin.Operator;
			var exitLab = default(Label);
			var termLab = default(Label);
            var ut = hints;

            if ((ut & Hints.Tail) == Hints.Tail)
                ut ^= Hints.Tail;

            if ((ut & Hints.Left) == Hints.Left)
                ut ^= Hints.Left;

			switch (op)
			{
                //Logical AND is executed in a lazy manner
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
                //Logical OR is executed in a lazy manner
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
                //Sequence operators forces left expression, pops it and yields a value
                //of a right expression. Evaliation is done in a strict order.
                case ElaOperator.Sequence:
					CompileExpression(bin.Left, map, Hints.None);
					cw.Emit(Op.Force);
					cw.Emit(Op.Pop);
                    var ut2 = hints;

                    if ((ut2 & Hints.Left) == Hints.Left)
                        ut2 ^= Hints.Left;

					CompileExpression(bin.Right, map, ut2);
					break;
			}

            if ((hints & Hints.Left) == Hints.Left)
                AddValueNotUsed(bin);
		}
	}
}