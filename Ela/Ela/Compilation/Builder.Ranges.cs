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
				if (range.Initial.Type != ElaNodeType.ListLiteral)
					AddError(ElaCompilerError.InfiniteRangeOnlyList, range, FormatNode(parent));

				var sv = AddVariable();
				CompileExpression(range.First, map, hints);
				cw.Emit(Op.Dup);
                PopVar(sv);
				CompileCycleFunction(range.Second, map);
                PushVar(sv);
				cw.Emit(Op.Gen);
			}
			else if (!TryOptimizeRange(range, map, hints))
				CompileStrictRange(range, map, hints);
		}


		private bool TryOptimizeRange(ElaRange range, LabelMap map, Hints hints)
		{
			if (range.First.Type != ElaNodeType.Primitive ||
				(range.Second != null && range.Second.Type != ElaNodeType.Primitive) ||
				range.Last.Type != ElaNodeType.Primitive)
				return false;

			var fst = (ElaPrimitive)range.First;
			var snd = range.Second != null ? (ElaPrimitive)range.Second : null;
			var lst = (ElaPrimitive)range.Last;

			if (fst.Value.LiteralType != ElaTypeCode.Integer ||
				(snd != null && snd.Value.LiteralType != ElaTypeCode.Integer) ||
				lst.Value.LiteralType != ElaTypeCode.Integer)
				return false;

			var fstVal = fst.Value.AsInteger();
			var sndVal = snd != null ? snd.Value.AsInteger() : fstVal + 1;
			var lstVal = lst.Value.AsInteger();
			var step = sndVal - fstVal;

			if (Math.Abs((fstVal - lstVal) / step) > 20)
				return false;

			CompileExpression(range.Initial, map, Hints.None);

			if (snd != null)
			{
				cw.Emit(Op.PushI4, fstVal);
				fstVal = sndVal;
				cw.Emit(Op.Gen);
			}

			for (; ; )
			{
				cw.Emit(Op.PushI4, fstVal);
				cw.Emit(Op.Gen);
				fstVal += step;

				if (step > 0)
				{
					if (fstVal > lstVal)
						break;
				}
				else if (fstVal < lstVal)
					break;
			}

			cw.Emit(Op.Genfin);
			return true;
		}


		private void CompileStrictRange(ElaRange rng, LabelMap map, Hints hints)
		{
			var start = AddVariable();
			CompileExpression(rng.First, map, Hints.None);
            PopVar(start);

			var last = AddVariable();
			CompileExpression(rng.Last, map, Hints.None);
            PopVar(last);

			var step = AddVariable();

			if (rng.Second != null)
			{
                PushVar(start);
				CompileExpression(rng.Second, map, Hints.None);
				cw.Emit(Op.Sub);
                PopVar(step);
			}
			else
			{
				cw.Emit(Op.PushI4, 1);
                PopVar(step);
			}

			var second = AddVariable();
			var trueLab = cw.DefineLabel();
			var endLab = cw.DefineLabel();
			PushVar(step);
			PushVar(start);
			cw.Emit(Op.Add);
            cw.Emit(Op.Dup);

            PopVar(second);

            PushVar(start);
            cw.Emit(Op.Cgt);
            cw.Emit(Op.Brtrue, trueLab);

            CompileExpression(rng.Initial, map, Hints.None);
            CompileStrictRangeCycle(start, second, step, last, hints, false);

            cw.Emit(Op.Br, endLab);
            cw.MarkLabel(trueLab);
            
            CompileExpression(rng.Initial, map, Hints.None);
            CompileStrictRangeCycle(start, second, step, last, hints, true);

			cw.MarkLabel(endLab);
			cw.Emit(Op.Genfin);
		}


		private void CompileStrictRangeCycle(int start, int second, int step, int last, Hints hints, bool brGt)
		{
            PushVar(start);
			cw.Emit(Op.Gen);

            PushVar(second);
			cw.Emit(Op.Dup);
            PopVar(start);

			cw.Emit(Op.Gen);

			var iterLab = cw.DefineLabel();
			var exitLab = cw.DefineLabel();
			cw.MarkLabel(iterLab);

            PushVar(step);
            PushVar(start);
			cw.Emit(Op.Add);
			cw.Emit(Op.Dup);
            PopVar(start);

            PushVar(last);

			if (brGt)
				cw.Emit(Op.Cgt);
			else
				cw.Emit(Op.Clt);

            cw.Emit(Op.Brtrue, exitLab);
            PushVar(start);
			cw.Emit(Op.Gen);

			cw.Emit(Op.Br, iterLab);
			cw.MarkLabel(exitLab);

		}


		private void CompileCycleFunction(ElaExpression sec, LabelMap map)
		{
			var start = AddVariable();
            PopVar(start);
			var step = AddVariable();
			var fun = AddVariable();

			if (sec != null)
			{
                PushVar(start);
				CompileExpression(sec, map, Hints.None);
				cw.Emit(Op.Sub);
                PopVar(step);
			}
			else
			{
				cw.Emit(Op.PushI4, 1);
                PopVar(step);
			}

			StartSection();
			cw.StartFrame(0);
			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);
			var address = cw.Offset;

            PushVar(1 | (fun >> 8) << 8);
			cw.Emit(Op.Newlazy);

            PushVar(1 | (step >> 8) << 8);
            PushVar(1 | (start >> 8) << 8);
			cw.Emit(Op.Add);
			cw.Emit(Op.Dup);
            PopVar(1 | (start >> 8) << 8);
			
			cw.Emit(Op.Gen);

			cw.Emit(Op.Ret);
			frame.Layouts.Add(new MemoryLayout(currentCounter, cw.FinishFrame(), address));
			EndSection();

			cw.MarkLabel(funSkipLabel);
			cw.Emit(Op.PushI4, 1);
			cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
			cw.Emit(Op.Dup);
            PopVar(fun);
			cw.Emit(Op.Newlazy);
		}
		#endregion
	}
}