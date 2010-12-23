using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Main
		private void CompileFor(ElaFor s, LabelMap map, Hints hints)
		{
			if (s.Pattern.Type != ElaNodeType.VariablePattern)
				AddError(ElaCompilerError.MatchNotSupportedInFor, s.Pattern, FormatNode(s));
			else
			{
				CheckEmbeddedWarning(s.Body);

				var comp = (hints & Hints.CompArray) == Hints.CompArray ||
					(hints & Hints.CompList) == Hints.CompList;

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
						var newHints = Hints.Scope |
							((hints & Hints.CompArray) == Hints.CompArray ? Hints.CompArray :
							(hints & Hints.CompList) == Hints.CompList ? Hints.CompList :
							Hints.Left);
						CompileExpression(s.Body, newMap, newHints);

						if (s.Body.Type != ElaNodeType.For &&
							s.Body.Type != ElaNodeType.While)
						{
							if ((hints & Hints.CompList) == Hints.CompList)
								cw.Emit(Op.Consr);
							else if ((hints & Hints.CompArray) == Hints.CompArray)
								cw.Emit(Op.Arrcons);
						}
					}

					cw.Emit(Op.Br, iter);
					cw.MarkLabel(exit);
					EndScope();

					if ((hints & Hints.Left) != Hints.Left && !comp)
						cw.Emit(Op.Pushunit);
					else
						cw.Emit(Op.Nop);
				}
			}
		}


		private void CompileWhile(ElaWhile s, LabelMap map, Hints hints)
		{
			CheckEmbeddedWarning(s.Body);

			var comp = (hints & Hints.CompArray) == Hints.CompArray ||
				(hints & Hints.CompList) == Hints.CompList;

			StartScope(false);
			var iter = cw.DefineLabel();
			var exit = cw.DefineLabel();
			var cond = cw.DefineLabel();
			var newMap = new LabelMap(map)
			{
				BlockStart = iter,
				BlockEnd = exit
			};

			var optimize = opt && s.Condition.Type == ElaNodeType.Primitive &&
				((ElaPrimitive)s.Condition).Value.AsBoolean();

			if (optimize)
				cw.Emit(Op.Br, cond);

			cw.MarkLabel(iter);

			if (s.Body != null)
			{
				var newHints = Hints.Scope |
					((hints & Hints.CompArray) == Hints.CompArray ? Hints.CompArray :
					(hints & Hints.CompList) == Hints.CompList ? Hints.CompList :
					Hints.Left);
				CompileExpression(s.Body, newMap, newHints);

				if (s.Body.Type != ElaNodeType.For &&
					s.Body.Type != ElaNodeType.While)
				{
					if ((hints & Hints.CompList) == Hints.CompList)
						cw.Emit(Op.Consr);
					else if ((hints & Hints.CompArray) == Hints.CompArray)
						cw.Emit(Op.Arrcons);
				}
			}
			else
				cw.Emit(Op.Nop);

			if (!optimize)
			{
				cw.MarkLabel(cond);
				CompileExpression(s.Condition, newMap, Hints.None);

				cw.Emit(Op.Brtrue, iter);
			}
			else
				cw.Emit(Op.Br, iter);

			cw.MarkLabel(exit);
			EndScope();

			if ((hints & Hints.Left) != Hints.Left && !comp)
				cw.Emit(Op.Pushunit);
			else
				cw.Emit(Op.Nop);
		}


		private void CompileForeach(ElaFor s, LabelMap map, Hints hints)
		{
			if (s.InitExpression != null)
				AddError(ElaCompilerError.ForeachNoInitialization, s.InitExpression);
			else
			{
				CheckEmbeddedWarning(s.Body);

				var comp = (hints & Hints.CompArray) == Hints.CompArray ||
					(hints & Hints.CompList) == Hints.CompList;

				StartScope(false);
				var iter = cw.DefineLabel();
				var breakExit = cw.DefineLabel();
				var newMap = new LabelMap(map)
				{
					BlockStart = iter,
					BlockEnd = breakExit
				};

				var addr = -1;

				if (s.Pattern.Type == ElaNodeType.VariablePattern)
					addr = AddVariable(((ElaVariablePattern)s.Pattern).Name, s.Pattern, s.VariableFlags, -1);
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
				cw.Emit(Op.Skiptl, addr >> 8);
				cw.Emit(Op.Br, breakExit);

				if (s.Pattern.Type != ElaNodeType.VariablePattern)
					CompilePattern(addr, null, s.Pattern, map, iter, s.VariableFlags, hints);

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
					var newHints = Hints.Scope |
						((hints & Hints.CompArray) == Hints.CompArray ? Hints.CompArray :
						(hints & Hints.CompList) == Hints.CompList ? Hints.CompList :
						Hints.Left);
					CompileExpression(s.Body, newMap, newHints);

					if (s.Body.Type != ElaNodeType.For &&
						s.Body.Type != ElaNodeType.While)
					{
						if ((hints & Hints.CompList) == Hints.CompList)
							cw.Emit(Op.Consr);
						else if ((hints & Hints.CompArray) == Hints.CompArray)
							cw.Emit(Op.Arrcons);
					}
				}

				cw.Emit(Op.Br, iter);
				cw.MarkLabel(breakExit);
				EndScope();

				if ((hints & Hints.Left) != Hints.Left && !comp)
					cw.Emit(Op.Pushunit);
				else
					cw.Emit(Op.Nop);
			}
		}
		#endregion


		#region Lazy
		private void CompileLazyList(ElaFor s, LabelMap map, Hints hints)
		{
			var f = s.Body;
			var sa = -1;

			while (f.Type == ElaNodeType.For)
			{
				var fc = (ElaFor)f;
				var addr = AddVariable();
				
				if (sa == -1)
					sa = addr >> 8;

				f = fc.Body;
			}

			var sai = sa;
			f = s.Body;

			while (f.Type == ElaNodeType.For)
			{
				var fc = (ElaFor)f;
				CompileExpression(fc.Target, map, Hints.None);
				cw.Emit(Op.Popvar, 0 | sai << 8);
				sai++;
				f = fc.Body;
			}

			var fun = CompileRecursiveFor(s, map, hints, -1, -1, 0, sa);
			CompileExpression(s.Target, map, Hints.None);
			cw.Emit(Op.Pushvar, fun);
			cw.Emit(Op.Call);
		}


		private int CompileRecursiveFor(ElaFor s, LabelMap map, Hints hints, int parent, int parentTail, int nest, int sa)
		{
			var funAddr = AddVariable();
			StartSection();
			StartScope(true);
			cw.StartFrame(1);
			var funSkipLabel = cw.DefineLabel();
			cw.Emit(Op.Br, funSkipLabel);
			var address = cw.Offset;

			var exitLab = cw.DefineLabel();
			var endLab = cw.DefineLabel();
			var iterLab = cw.DefineLabel();
			var head = AddVariable();
			var tail = AddVariable();
			cw.Emit(Op.Skiptl, head >> 8);
			cw.Emit(Op.Br, endLab);

			if (s.Pattern.Type == ElaNodeType.VariablePattern)
			{
				var addr = AddVariable(((ElaVariablePattern)s.Pattern).Name, s.Pattern, s.VariableFlags, -1);
				cw.Emit(Op.Pushvar, head);
				cw.Emit(Op.Popvar, addr);
			}
			else
				CompilePattern(head, null, s.Pattern, map, iterLab, ElaVariableFlags.Immutable, hints);

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

			if (s.Body.Type == ElaNodeType.For)
			{
				var f = (ElaFor)s.Body;
				var child = CompileRecursiveFor(f, map, hints, funAddr, tail, nest + 1, sa);
				cw.Emit(Op.Pushvar, nest + 1 | (sa + nest) << 8);
				cw.Emit(Op.Pushvar, child);
				cw.Emit(Op.Callt);
			}
			else
			{
				CompileExpression(s.Body, map, Hints.None);
				cw.Emit(Op.Pushvar, tail);
				cw.Emit(Op.Pushvar, 1 | (funAddr >> 8) << 8);
				cw.Emit(Op.LazyCall);
				cw.Emit(Op.Cons);
				cw.Emit(Op.Br, exitLab);
			}

			cw.MarkLabel(iterLab);
			cw.Emit(Op.Pushvar, tail);
			cw.Emit(Op.Pushvar, 1 | (funAddr >> 8) << 8);
			cw.Emit(Op.Callt);

			cw.MarkLabel(endLab);

			if (parent == -1)
				cw.Emit(Op.Newlist);
			else
			{
				cw.Emit(Op.Pushvar, 1 | (parentTail >> 8) << 8);
				cw.Emit(Op.Pushvar, 2 | (parent >> 8) << 8);
				cw.Emit(Op.Callt);
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