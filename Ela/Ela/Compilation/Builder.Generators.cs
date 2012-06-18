using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Main
		private void CompileGenerator(ElaGenerator s, LabelMap map, Hints hints)
		{
			StartScope(false, s.Line, s.Column);
			var iter = cw.DefineLabel();
			var breakExit = cw.DefineLabel();
			var newMap = new LabelMap(map);

			var addr = -1;

			if (s.Pattern.Type == ElaNodeType.VariablePattern)
				addr = AddVariable(((ElaVariablePattern)s.Pattern).Name, s.Pattern, ElaVariableFlags.None, -1);
			else
				addr = AddVariable();

			var serv = AddVariable();
			CompileExpression(s.Target, map, Hints.None);
			cw.Emit(Op.Dup);
			cw.Emit(Op.Popvar, serv);
			cw.Emit(Op.Isnil);
			cw.Emit(Op.Brtrue, breakExit);

			cw.MarkLabel(iter);
			cw.Emit(Op.Pushvar, serv);

            cw.Emit(Op.Isnil);
            cw.Emit(Op.Brtrue, breakExit);
            cw.Emit(Op.Pushvar, serv);
            cw.Emit(Op.Head);
            cw.Emit(Op.Popvar, addr);
            cw.Emit(Op.Pushvar, serv);
            cw.Emit(Op.Tail);
            cw.Emit(Op.Popvar, 0 | ((addr >> 8) + 1) << 8);

			if (s.Pattern.Type != ElaNodeType.VariablePattern)
				CompilePattern(addr, null, s.Pattern, map, iter, ElaVariableFlags.None, hints);

			if (s.Guard != null)
			{
				if (s.Guard.Type == ElaNodeType.OtherwiseGuard)
					AddError(ElaCompilerError.ElseGuardNotValid, s.Guard);
				else
				{
					CompileExpression(s.Guard, map, Hints.None);
					cw.Emit(Op.Brfalse, iter);
				}
			}

			if (s.Body != null)
			{
				CompileExpression(s.Body, newMap, Hints.Scope);

				if (s.Body.Type != ElaNodeType.Generator)
					cw.Emit(Op.Gen);
			}

			cw.Emit(Op.Br, iter);
			cw.MarkLabel(breakExit);
			EndScope();

			cw.Emit(Op.Nop);
		}
		#endregion


		#region Lazy
		private void CompileLazyList(ElaGenerator s, LabelMap map, Hints hints)
		{
            var fun = CompileRecursiveFor(s, map, hints, -1, -1);
			CompileExpression(s.Target, map, Hints.None);
			cw.Emit(Op.Pushvar, fun);
			cw.Emit(Op.Call);
		}


		private int CompileRecursiveFor(ElaGenerator s, LabelMap map, Hints hints, int parent, int parentTail)
		{
			var funAddr = AddVariable();
			StartSection();
			StartScope(true, s.Line, s.Column);
			cw.StartFrame(1);
			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);
			var address = cw.Offset;

			var exitLab = cw.DefineLabel();
			var endLab = cw.DefineLabel();
			var iterLab = cw.DefineLabel();
			var head = AddVariable();
			var tail = AddVariable();

            var sys = AddVariable();
            cw.Emit(Op.Dup);
            cw.Emit(Op.Popvar, sys);
            cw.Emit(Op.Isnil);
            cw.Emit(Op.Brtrue, endLab);
            cw.Emit(Op.Pushvar, sys);
            cw.Emit(Op.Head);
            cw.Emit(Op.Popvar, head);
            cw.Emit(Op.Pushvar, sys);
            cw.Emit(Op.Tail);
            cw.Emit(Op.Popvar, tail);

			if (s.Pattern.Type == ElaNodeType.VariablePattern)
			{
				var addr = AddVariable(((ElaVariablePattern)s.Pattern).Name, s.Pattern, ElaVariableFlags.None, -1);
				cw.Emit(Op.Pushvar, head);
				cw.Emit(Op.Popvar, addr);
			}
			else
				CompilePattern(head, null, s.Pattern, map, iterLab, ElaVariableFlags.None, hints);

			if (s.Guard != null)
			{
				if (s.Guard.Type == ElaNodeType.OtherwiseGuard)
					AddError(ElaCompilerError.ElseGuardNotValid, s.Guard);
				else
				{
					CompileExpression(s.Guard, map, Hints.None);
					cw.Emit(Op.Brfalse, iterLab);
				}
			}

			if (s.Body.Type == ElaNodeType.Generator)
			{
				var f = (ElaGenerator)s.Body;
				var child = CompileRecursiveFor(f, map, hints, funAddr, tail);
				CompileExpression(f.Target, map, Hints.None);
                cw.Emit(Op.Pushvar, child);
				cw.Emit(Op.Call);
                cw.Emit(Op.Br, exitLab);//
			}
			else
			{
				cw.Emit(Op.Pushvar, tail);
				cw.Emit(Op.Pushvar, 1 | (funAddr >> 8) << 8);
				cw.Emit(Op.LazyCall);
				CompileExpression(s.Body, map, Hints.None);
				cw.Emit(Op.Gen);
				cw.Emit(Op.Br, exitLab);
			}

			cw.MarkLabel(iterLab);
			cw.Emit(Op.Pushvar, tail);
			cw.Emit(Op.Pushvar, 1 | (funAddr >> 8) << 8);
			cw.Emit(Op.Call);
            cw.Emit(Op.Br, exitLab);//

			cw.MarkLabel(endLab);

			if (parent == -1)
				cw.Emit(Op.Newlist);
			else
			{
				cw.Emit(Op.Pushvar, 1 | (parentTail >> 8) << 8);
				cw.Emit(Op.Pushvar, 2 | (parent >> 8) << 8);
				cw.Emit(Op.Call);
			}

			cw.MarkLabel(exitLab);
			cw.Emit(Op.Ret);
			frame.Layouts.Add(new MemoryLayout(currentCounter, cw.FinishFrame(), address));
			EndSection();
			EndScope();

			cw.MarkLabel(funSkipLabel);
			cw.Emit(Op.PushI4, 1);
			cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
			cw.Emit(Op.Popvar, funAddr);
			return funAddr;
		}
		#endregion
	}
}