using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Main
		private void CompileRange(ElaExpression parent, ElaRange range, LabelMap map, Hints hints)
		{
			AddLinePragma(range);

			if (range.Last == null)
			{
				if ((hints & Hints.CompList) != Hints.CompList)
					AddError(ElaCompilerError.InfiniteRangeOnlyList, range, FormatNode(parent));

				CompileExpression(range.First, map, hints);
				cw.Emit(Op.Dup);
				CompileCycleFunction(range.Second, map);
				cw.Emit(Op.Cons);
			}
			else if (!TryOptimizeRange(range, hints))
				CompileStrictRange(range, map, hints);
		}


		private bool TryOptimizeRange(ElaRange range, Hints hints)
		{
			if (range.First.Type != ElaNodeType.Primitive ||
				(range.Second != null && range.Second.Type != ElaNodeType.Primitive) ||
				range.Last.Type != ElaNodeType.Primitive)
				return false;

			var fst = (ElaPrimitive)range.First;
			var snd = range.Second != null ? (ElaPrimitive)range.Second : null;
			var lst = (ElaPrimitive)range.Last;

			if (fst.Value.LiteralType != ObjectType.Integer ||
				(snd != null && snd.Value.LiteralType != ObjectType.Integer) ||
				lst.Value.LiteralType != ObjectType.Integer)
				return false;

			var fstVal = fst.Value.AsInteger();
			var sndVal = snd != null ? snd.Value.AsInteger() : fstVal + 1;
			var lstVal = lst.Value.AsInteger();
			var step = sndVal - fstVal;

			if (Math.Abs((fstVal - lstVal) / step) > 20)
				return false;

			if ((hints & Hints.CompList) == Hints.CompList)
				cw.Emit(Op.Newlist);
			else
				cw.Emit(Op.Newarr);

			if (snd != null)
			{
				cw.Emit(Op.PushI4, fstVal);
				fstVal = sndVal;

				if ((hints & Hints.CompList) == Hints.CompList)
					cw.Emit(Op.Consr);
				else
					cw.Emit(Op.Arrcons);
			}

			for (; ; )
			{
				cw.Emit(Op.PushI4, fstVal);

				if ((hints & Hints.CompList) == Hints.CompList)
					cw.Emit(Op.Consr);
				else
					cw.Emit(Op.Arrcons);

				fstVal += step;

				if (step > 0)
				{
					if (fstVal > lstVal)
						break;
				}
				else if (fstVal < lstVal)
					break;
			}

			if ((hints & Hints.CompList) == Hints.CompList)
				cw.Emit(Op.Lrev);

			return true;
		}


		private void CompileStrictRange(ElaRange rng, LabelMap map, Hints hints)
		{
			var start = AddVariable();
			CompileExpression(rng.First, map, Hints.None);
			cw.Emit(Op.Popvar, start);

			var last = AddVariable();
			CompileExpression(rng.Last, map, Hints.None);
			cw.Emit(Op.Popvar, last);

			var step = AddVariable();

			if (rng.Second != null)
			{
				CompileExpression(rng.Second, map, Hints.None);
				cw.Emit(Op.Pushvar, start);
				cw.Emit(Op.Sub);
				cw.Emit(Op.Popvar, step);
			}
			else
			{
				cw.Emit(Op.PushI4, 1);
				cw.Emit(Op.Popvar, step);
			}

			var second = AddVariable();
			var trueLab = cw.DefineLabel();
			var endLab = cw.DefineLabel();
			cw.Emit(Op.Pushvar, start);
			cw.Emit(Op.Pushvar, step);
			cw.Emit(Op.Add);
			cw.Emit(Op.Dup);
			cw.Emit(Op.Popvar, second);
			cw.Emit(Op.Pushvar, start);
			cw.Emit(Op.Br_gt, trueLab);
			CompileStrictRangeCycle(start, second, step, last, hints, false);
			cw.Emit(Op.Br, endLab);
			cw.MarkLabel(trueLab);
			CompileStrictRangeCycle(start, second, step, last, hints, true);
			cw.MarkLabel(endLab);

			if ((hints & Hints.CompList) == Hints.CompList)
				cw.Emit(Op.Lrev);
			else
				cw.Emit(Op.Nop);
		}


		private void CompileStrictRangeCycle(int start, int second, int step, int last, Hints hints, bool brGt)
		{
			if ((hints & Hints.CompList) == Hints.CompList)
				cw.Emit(Op.Newlist);
			else
				cw.Emit(Op.Newarr);

			cw.Emit(Op.Pushvar, start);

			if ((hints & Hints.CompList) == Hints.CompList)
				cw.Emit(Op.Consr);
			else
				cw.Emit(Op.Arrcons);

			cw.Emit(Op.Pushvar, second);
			cw.Emit(Op.Dup);
			cw.Emit(Op.Popvar, start);

			if ((hints & Hints.CompList) == Hints.CompList)
				cw.Emit(Op.Consr);
			else
				cw.Emit(Op.Arrcons);

			var iterLab = cw.DefineLabel();
			var exitLab = cw.DefineLabel();
			cw.MarkLabel(iterLab);

			cw.Emit(Op.Pushvar, start);
			cw.Emit(Op.Pushvar, step);
			cw.Emit(Op.Add);
			cw.Emit(Op.Dup);
			cw.Emit(Op.Popvar, start);

			cw.Emit(Op.Pushvar, last);

			if (brGt)
				cw.Emit(Op.Br_gt, exitLab);
			else
				cw.Emit(Op.Br_lt, exitLab);

			cw.Emit(Op.Pushvar, start);

			if ((hints & Hints.CompList) == Hints.CompList)
				cw.Emit(Op.Consr);
			else
				cw.Emit(Op.Arrcons);

			cw.Emit(Op.Br, iterLab);
			cw.MarkLabel(exitLab);

		}


		private void CompileCycleFunction(ElaExpression sec, LabelMap map)
		{
			var start = AddVariable();
			cw.Emit(Op.Popvar, start);
			var step = AddVariable();
			var fun = AddVariable();

			if (sec != null)
			{
				CompileExpression(sec, map, Hints.None);
				cw.Emit(Op.Pushvar, start);
				cw.Emit(Op.Sub);
				cw.Emit(Op.Popvar, step);
			}
			else
			{
				cw.Emit(Op.PushI4, 1);
				cw.Emit(Op.Popvar, step);
			}

			StartSection();
			cw.StartFrame(0);
			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);
			var address = cw.Offset;

			cw.Emit(Op.Pushvar, 1 | (start >> 8) << 8);
			cw.Emit(Op.Pushvar, 1 | (step >> 8) << 8);
			cw.Emit(Op.Add);
			cw.Emit(Op.Dup);
			cw.Emit(Op.Popvar, 1 | (start >> 8) << 8);
			cw.Emit(Op.Pushvar, 1 | (fun >> 8) << 8);
			cw.Emit(Op.Newlazy);
			cw.Emit(Op.Cons);

			cw.Emit(Op.Ret);
			frame.Layouts.Add(new MemoryLayout(currentCounter, cw.FinishFrame(), address));
			EndSection();

			cw.MarkLabel(funSkipLabel);
			cw.Emit(Op.PushI4, 1);
			cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
			cw.Emit(Op.Dup);
			cw.Emit(Op.Popvar, fun);
			cw.Emit(Op.Newlazy);
		}
		#endregion
	}
}