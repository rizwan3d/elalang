using System;
using Ela.CodeModel;
using Ela.Runtime;
using System.Collections.Generic;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Main
		private void CompileMatch(ElaMatch exp, ElaExpression matchExp, LabelMap map, Hints hints)
		{
			var serv = -1;
			var tuple = default(ElaTupleLiteral);

			if (matchExp != null)
			{
				if (matchExp.Type == ElaNodeType.TupleLiteral)
					tuple = (ElaTupleLiteral)matchExp;
				
				if (tuple == null && serv == -1)
				{
					CompileExpression(matchExp, map, Hints.None);
					serv = AddVariable();
					cw.Emit(Op.Popvar, serv);
				}
			}
			else
			{
				serv = AddVariable();
				cw.Emit(Op.Popvar, serv);
			}

			var len = exp.Entries.Count;
			var end = cw.DefineLabel();
			var next = Label.Empty;
			var nextGuard = Label.Empty;
			var newHints = (hints & Hints.Tail) == Hints.Tail ? Hints.Tail : Hints.None;
            var oldWhere = default(ElaExpression);
			var prevEntry = default(ElaMatchEntry);
			var processed = new FastList<ElaPattern>();

			var plen = -1;

			for (var i = 0; i < len; i++)
			{
				var e = exp.Entries[i];

				if (e.Pattern != null && e.Guard == null)
				{
					foreach (var o in processed)
						if (!e.Pattern.CanFollow(o))
						{
							AddWarning(ElaCompilerWarning.MatchEntryNotReachable, e.Pattern, e.Pattern, o);
							AddHint(ElaCompilerHint.MatchEntryNotReachable, e.Pattern);
						}
				}

				var last = i == len - 1;

				if (e.Pattern != null)
				{
					var clen = e.Pattern.Type == ElaNodeType.PatternGroup ? ((ElaPatternGroup)e.Pattern).Patterns.Count : 1;

					if (plen > -1 && clen != plen)
						AddError(ElaCompilerError.PatternNotAllArgs, e, FormatNode(e.Pattern));

					plen = clen;
				}

				if (e.Pattern == null && e.Guard == null)
				{
					AddError(ElaCompilerError.MatchEntryEmptyNoGuard, e);
					continue;
				}
				else if (e.Guard != null && e.Guard.Type == ElaNodeType.OtherwiseGuard &&
					(e.Pattern != null || prevEntry == null || prevEntry.Guard == null || prevEntry.Guard.Type == ElaNodeType.OtherwiseGuard))
				{
					AddError(ElaCompilerError.ElseGuardNotValid, e);
					continue;
				}
				else if (e.Pattern == null)
				{
					//if (prevEntry != null && prevEntry.Pattern != null)
					//    CompilePattern(serv, tuple, prevEntry.Pattern, map, next, ElaVariableFlags.None, hints);
					
					if (!nextGuard.IsEmpty())
						cw.MarkLabel(nextGuard);

					var where = FindWhere(i, exp.Entries);

					if (where != null && where != oldWhere)
						CompileExpression(where, map, Hints.Left);

					oldWhere = where;

					if (e.Guard.Type != ElaNodeType.OtherwiseGuard)
					{
						nextGuard = cw.DefineLabel();
						CompileExpression(e.Guard, map, Hints.Scope);
						cw.Emit(Op.Brfalse, nextGuard);
					}
					else
						nextGuard = Label.Empty;

					CompileExpression(e.Expression, map, newHints);

					if (i == len - 1)
						EndScope();

					prevEntry = e;
				}
				else
				{
					if (i != 0)
						EndScope();

					if (!next.IsEmpty())
					{
						if (!nextGuard.IsEmpty())
						{
							cw.MarkLabel(nextGuard);
							cw.Emit(Op.Nop);
                            nextGuard = Label.Empty;
						}

						cw.MarkLabel(next);
						cw.Emit(Op.Nop);
					}

					StartScope(false);
					next = cw.DefineLabel();

					var mc = Errors.Count;
					CompilePattern(serv, tuple, e.Pattern, map, next, ElaVariableFlags.None, hints);

					if (Errors.Count == mc)
					{
						var where = FindWhere(i, exp.Entries);

						if (where != null && where != oldWhere)
							CompileExpression(where, map, Hints.Left);

						oldWhere = where;

						if (e.Guard != null)
						{
							nextGuard = cw.DefineLabel();
							CompileExpression(e.Guard, map, Hints.Scope);
							cw.Emit(Op.Brfalse, nextGuard);
						}

						CompileExpression(e.Expression, map, newHints);
					}

					prevEntry = e;

					if (i == len - 1)
						EndScope();
				}

				cw.Emit(Op.Br, end);

				if (e.Pattern != null && e.Guard == null)
					processed.Add(e.Pattern);
			}

			cw.MarkLabel(next);

			if (!nextGuard.IsEmpty())
				cw.MarkLabel(nextGuard);

			if ((hints & Hints.Throw) == Hints.Throw)
			{
				cw.Emit(Op.Pushvar, serv);
				cw.Emit(Op.Rethrow);
			}
			else
				cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);

			cw.MarkLabel(end);
			cw.Emit(Op.Nop);
		}


		private ElaExpression FindWhere(int i, List<ElaMatchEntry> entries)
		{
			if (entries[i].Where != null)
				return entries[i].Where;

			var idx = i + 1;

			while (idx < entries.Count)
			{
				var e = entries[idx];

				if (e.Guard == null || e.Pattern != null)
					return null;

				if (e.Where != null)
					return e.Where;

				idx++;
			}

			return null;
		}


		private void CompilePattern(int pushSys, ElaTupleLiteral tuple, ElaPattern patExp, LabelMap map, Label nextLab, ElaVariableFlags flags, Hints hints)
		{
			switch (patExp.Type)
			{
				case ElaNodeType.VariantPattern:
                    var vp = (ElaVariantPattern)patExp;
						
					if (tuple != null && vp.Tag != "Tuple#")
						MatchEntryAlwaysFail(patExp, nextLab);
					else
					{
                        if (tuple != null)
                        {
                            CompileExpression(tuple, map, Hints.None);
                            pushSys = AddVariable();
                            cw.Emit(Op.Popvar, pushSys);
                        }

						cw.Emit(Op.Pushvar, pushSys);
						
						if (!String.IsNullOrEmpty(vp.Tag))
							cw.Emit(Op.Skiptag, AddString(vp.Tag));
						else
							cw.Emit(Op.Skiphtag);

						cw.Emit(Op.Br, nextLab);

						if (vp.Pattern != null)
						{
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Untag);
							cw.Emit(Op.Popvar, pushSys);
							CompilePattern(pushSys, null, vp.Pattern, map, nextLab, flags, hints);
						}
					}
					break;
				case ElaNodeType.UnitPattern:
					{
						if (tuple != null)
							MatchEntryAlwaysFail(patExp, nextLab);
						else
							CheckType(pushSys, ElaTypeCode.Unit, nextLab);
					}
					break;
				case ElaNodeType.AsPattern:
					{
						var asPat = (ElaAsPattern)patExp;
						var addr = AddMatchVariable(asPat.Name, flags, asPat);

						if (tuple != null)
						{
							CompileExpression(tuple, map, Hints.None);
							pushSys = AddVariable();
							cw.Emit(Op.Popvar, pushSys);
						}

						cw.Emit(Op.Pushvar, pushSys);
						cw.Emit(Op.Popvar, addr);

						CompilePattern(pushSys, tuple, asPat.Pattern, map, nextLab, flags, hints);
					}
					break;
				case ElaNodeType.LiteralPattern:
					if (tuple != null)
						MatchEntryAlwaysFail(patExp, nextLab);
					else
					{
						cw.Emit(Op.Pushvar, pushSys);
						PushPrimitive(((ElaLiteralPattern)patExp).Value);
						cw.Emit(Op.Br_neq, nextLab);
					}
					break;
				case ElaNodeType.VariablePattern:
					{
						var vexp = (ElaVariablePattern)patExp;

						if (tuple != null)
						{
							CompileExpression(tuple, map, Hints.None);
							pushSys = AddVariable();
							cw.Emit(Op.Popvar, pushSys);
						}

						var addr = AddMatchVariable(vexp.Name, flags, vexp);

						if (pushSys != addr)
						{
							cw.Emit(Op.Pushvar, pushSys);
							cw.Emit(Op.Popvar, addr);
						}
					}
					break;
				case ElaNodeType.RecordPattern:
					CompileRecordPattern(pushSys, tuple, (ElaRecordPattern)patExp, map, nextLab, flags, hints);
					break;
				case ElaNodeType.PatternGroup:
				case ElaNodeType.TuplePattern:
					CompileSequencePattern(pushSys, tuple, (ElaTuplePattern)patExp, map, nextLab, flags, hints);
					break;
				case ElaNodeType.DefaultPattern:
					break;
				case ElaNodeType.HeadTailPattern:
					if (tuple != null)
						MatchEntryAlwaysFail(patExp, nextLab);
					else
						CompileHeadTailPattern(pushSys, (ElaHeadTailPattern)patExp, map, nextLab, flags, hints);
					break;
				case ElaNodeType.NilPattern:
					if (tuple != null)
						MatchEntryAlwaysFail(patExp, nextLab);
					else
					{
						Silent(pushSys, nextLab, hints, ElaPatterns.HeadTail);
						cw.Emit(Op.Pushvar, pushSys);
						cw.Emit(Op.Isnil);
						cw.Emit(Op.Brfalse, nextLab);
					}
					break;
			}
		}


		private void CompileHeadTailPattern(int pushSys, ElaHeadTailPattern pexp, LabelMap map, Label nextLab, ElaVariableFlags flags, Hints hints)
		{
			if (pexp.Patterns.Count == 2 && currentCounter < 250 &&
				(pexp.Patterns[0].Type == ElaNodeType.VariablePattern || pexp.Patterns[0].Type == ElaNodeType.DefaultPattern) &&
                (pexp.Patterns[1].Type == ElaNodeType.VariablePattern || pexp.Patterns[1].Type == ElaNodeType.DefaultPattern ||
                pexp.Patterns[1].Type == ElaNodeType.NilPattern))
			{
				Silent(pushSys, nextLab, hints, ElaPatterns.HeadTail);
				cw.Emit(Op.Pushvar, pushSys);

				var newHints = (hints & Hints.FunBody) == Hints.FunBody ? hints ^ Hints.FunBody : hints;
				var pe1 = pexp.Patterns[0];
				var pe2 = pexp.Patterns[1];
				var v1 = 0;
				
				if (pe1.Type == ElaNodeType.VariablePattern)
					v1 = AddMatchVariable(((ElaVariablePattern)pe1).Name, flags, pe1);
				else
					v1 = AddVariable();

				if (pe2.Type == ElaNodeType.VariablePattern || pe2.Type == ElaNodeType.DefaultPattern)
				{
					if (pe2.Type == ElaNodeType.VariablePattern)
						AddMatchVariable(((ElaVariablePattern)pe2).Name, flags, pe2); //v2
					else
						AddVariable(); //v2

					cw.Emit(Op.Skiptl, v1 >> 8);
				}
				else if (pe2.Type == ElaNodeType.NilPattern)
					cw.Emit(Op.Skiptn, v1 >> 8);

				cw.Emit(Op.Br, nextLab);
			}
			else
			{
				Silent(pushSys, nextLab, hints, ElaPatterns.HeadTail);

				cw.Emit(Op.Pushvar, pushSys);
				cw.Emit(Op.Brnil, nextLab);

				var els = pexp.Patterns;
				var len = els.Count;
				cw.Emit(Op.Pushvar, pushSys);

                if (len > 2)
                {
                    pushSys = AddVariable();
					cw.Emit(Op.Dup);
					cw.Emit(Op.Popvar, pushSys);
                }

				for (var i = 0; i < len; i++)
				{
					var e = els[i];
					var isEnd = i == len - 1;

					if (!isEnd)
					{
						if (i > 0)
						{
							cw.Emit(Op.Dup);
							cw.Emit(Op.Popvar, pushSys);
							cw.Emit(Op.Brnil, nextLab);
							cw.Emit(Op.Pushvar, pushSys);
						}

						cw.Emit(Op.Head);
					}

					if (e.Type == ElaNodeType.VariablePattern)
					{
						var nm = ((ElaVariablePattern)e).Name;
						var a = AddMatchVariable(nm, flags, e);
						cw.Emit(Op.Popvar, a);
					}
					else if (e.Type == ElaNodeType.DefaultPattern)
						cw.Emit(Op.Pop);
					else
					{
						var newSys = AddVariable();
						cw.Emit(Op.Popvar, newSys);
						CompilePattern(newSys, null, e, map, nextLab, flags, hints);
					}

					if (!isEnd)
					{
						cw.Emit(Op.Pushvar, pushSys);
						cw.Emit(Op.Tail);
					}
				}
			}
		}


		private void CompileRecordPattern(int pushSys, ElaTupleLiteral tuple, ElaRecordPattern rec, LabelMap map, Label nextLab, ElaVariableFlags flags, Hints hints)
		{
			if (tuple != null)
				MatchEntryAlwaysFail(rec, nextLab);
			else
			{
				Silent(pushSys, nextLab, hints, ElaPatterns.Record);

				for (var i = 0; i < rec.Fields.Count; i++)
				{
					var fld = rec.Fields[i];
					var addr = AddVariable();
					var str = AddString(fld.Name);

					if ((hints & Hints.Silent) == Hints.Silent)
					{
						//cw.Emit(Op.Pushvar, pushSys);
						//cw.Emit(Op.Hasfld, str);
						//cw.Emit(Op.Brfalse, nextLab);
					}

					cw.Emit(Op.Pushstr, str);
					cw.Emit(Op.Pushvar, pushSys);
					cw.Emit(Op.Pushelem);
					cw.Emit(Op.Popvar, addr);
					CompilePattern(addr, null, fld.Value, map, nextLab, flags, hints);
				}
			}

		}
		

		private void CompileSequencePattern(int pushSys, ElaTupleLiteral tuple, ElaTuplePattern seq, LabelMap map, Label nextLab, ElaVariableFlags flags, Hints hints)
		{
			var len = seq.Patterns.Count;

			if (pushSys != -1 && tuple == null)
				Silent(pushSys, nextLab, hints, ElaPatterns.Tuple);
			
			if (tuple != null)
			{
				if (tuple.Parameters.Count < len || tuple.Parameters.Count > len)
				{
					cw.Emit(Op.Br, nextLab);
					AddWarning(ElaCompilerWarning.MatchEntryAlwaysFail, seq, seq);
				}
				else
				{
					for (var i = 0; i < len; i++)
					{
						var p = tuple.Parameters[i];
						var newSys = -1;

						if (p.Type == ElaNodeType.VariableReference)
						{
							var v = (ElaVariableReference)p;
							var sv = GetVariable(v.VariableName, CurrentScope, 0, GetFlags.OnlyGet, v.Line, v.Column);
							newSys = sv.Address;
						}
						else
						{
							CompileExpression(tuple.Parameters[i], map, hints);
							newSys = AddVariable();
							cw.Emit(Op.Popvar, newSys);
						}

						CompilePattern(newSys, null, seq.Patterns[i], map, nextLab, flags, hints);
					}
				}
			}
			else
			{
				cw.Emit(Op.Pushvar, pushSys);
				cw.Emit(Op.Len);

				if (len == 0)
					cw.Emit(Op.PushI4_0);
				else
					cw.Emit(Op.PushI4, len);

				cw.Emit(Op.Br_neq, nextLab);

				for (var i = 0; i < len; i++)
				{
					var pat = seq.Patterns[i];
					cw.Emit(Op.Pushvar, pushSys);

					if (i == 0)
						cw.Emit(Op.PushI4_0);
					else
						cw.Emit(Op.PushI4, i);

					cw.Emit(Op.Pushelem);
					var newSys = AddVariable();
					cw.Emit(Op.Popvar, newSys);
					CompilePattern(newSys, null, pat, map, nextLab, flags, hints);
				}
			}
		}
		#endregion


		#region Helper
		private void CheckType(int pushSys, ElaTypeCode type, Label nextLab)
		{
			cw.Emit(Op.Pushvar, pushSys);
			cw.Emit(Op.Typeid);
			cw.Emit(Op.PushI4, (Int32)type);
			cw.Emit(Op.Br_neq, nextLab);
		}


		private void Silent(int pushSys, Label nextLab, Hints hints, ElaPatterns pats)
		{
			if ((hints & Hints.Silent) == Hints.Silent)
			{
				//cw.Emit(Op.Pushvar, pushSys);
				//cw.Emit(Op.Pat, (Int32)pats);
				//cw.Emit(Op.Brfalse, nextLab);
			}
		}


		private void MatchEntryAlwaysFail(ElaExpression exp, Label lab)
		{
			AddWarning(ElaCompilerWarning.MatchEntryAlwaysFail, exp, exp);
			cw.Emit(Op.Br, lab);
		}


		private int AddMatchVariable(string varName, ElaVariableFlags flags, ElaExpression exp)
		{
			if (IsRegistered(varName))
			{
				AddError(ElaCompilerError.RedefinitionNotAllowed, exp, varName);
				return -1;
			}

			return AddVariable(varName, exp, flags, -1);
		}
		#endregion
	}
}