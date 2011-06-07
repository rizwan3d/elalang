using System;
using Ela.CodeModel;
using Ela.Runtime;

namespace Ela.Compilation
{
	internal sealed partial class Builder
	{
		#region Main
		private void CompileDeclaration(ElaBinding s, LabelMap map, Hints hints)
		{
			if (s.InitExpression == null)
				AddError(ElaCompilerError.VariableDeclarationInitMissing, s);

            var data = -1;
			var flags = s.VariableFlags;

			if (s.InitExpression != null && s.InitExpression.Type == ElaNodeType.Builtin)
			{
				data = (Int32)((ElaBuiltin)s.InitExpression).Kind;
				flags |= ElaVariableFlags.Builtin;

                if (String.IsNullOrEmpty(s.VariableName))
                    AddError(ElaCompilerError.InvalidBuiltinBinding, s);
			}
			
			if (s.In != null)
				StartScope(false);

            if ((s.VariableFlags & ElaVariableFlags.Private) == ElaVariableFlags.Private &&
                CurrentScope != globalScope)
                AddError(ElaCompilerError.PrivateOnlyGlobal, s);

            var inline = (s.VariableFlags & ElaVariableFlags.Inline) == ElaVariableFlags.Inline;
			
			if (s.Pattern == null)
			{
				var addr = -1;
				var addSym = false;
				
				if (s.InitExpression != null && s.InitExpression.Type == ElaNodeType.FunctionLiteral)
				{
					var fun = (ElaFunctionLiteral)s.InitExpression;
					addr = (hints & Hints.And) == Hints.And ?
						GetVariable(s.VariableName, CurrentScope, 0, GetFlags.SkipValidation|GetFlags.OnlyGet, s.Line, s.Column).Address :
						AddVariable(s.VariableName, s, flags, data);

					if (inline)
					{
						inlineFuns.Remove(fun.Name);
						inlineFuns.Add(fun.Name, new InlineFun(fun, CurrentScope));
					}
				}
				else
				{
					addr = GetVariable(s.VariableName, CurrentScope, 0, GetFlags.SkipValidation | GetFlags.OnlyGet, s.Line, s.Column).Address;
                    addSym = true;
				}

				var po = cw.Offset;
				var and = s.And;
                var noInitCode = allowNoInits.Count;

				while (and != null && (hints & Hints.And) != Hints.And)
				{
                    AddVariable(and.VariableName, and, and.VariableFlags | ElaVariableFlags.NoInit, noInitCode);					
                    and = and.And;
				}

				if (s.Where != null)
					CompileWhere(s.Where, map, Hints.Left);

                allowNoInits.Push(new NoInit(noInitCode, !addSym));
				var ed = s.InitExpression != null ? CompileExpression(s.InitExpression, map, Hints.None) : default(ExprData);
				var fc = ed.Type == DataKind.FunCurry || ed.Type == DataKind.FunParams;
				allowNoInits.Pop();

				if (ed.Type == DataKind.FunParams && addSym)
				{
					pdb.StartFunction(s.VariableName, po, ed.Data);
					pdb.EndFunction(-1, cw.Offset);
				}

				if (s.Where != null)
					EndScope();

				if (addr != -1)
				{
					if (fc)
						CurrentScope.ChangeVariable(s.VariableName, new ScopeVar(s.VariableFlags | ElaVariableFlags.Function, addr >> 8, ed.Data));
					else
						CurrentScope.ChangeVariable(s.VariableName, new ScopeVar(s.VariableFlags | flags, addr >> 8, data));
				}
				else if (addr == -1)
				{
					if (fc)
						flags |= ElaVariableFlags.Function;

					if (ed.Type == DataKind.Builtin)
						flags |= ElaVariableFlags.Builtin;

					addr = AddVariable(s.VariableName, s, flags, data != -1 ? data : ed.Data);
				}

				AddLinePragma(s);
				cw.Emit(Op.Popvar, addr);
			}
			else
				CompileBindingPattern(s, map);

			var newHints = hints | (s.In != null ? Hints.Scope : Hints.None) | Hints.And;

			if (s.And != null)
				CompileExpression(s.And, map, s.In != null ? (newHints | Hints.Left) : newHints);
			
			if (s.In != null)
				CompileIn(s, map, newHints);
			else if ((hints & Hints.Left) != Hints.Left && s.And == null)
				cw.Emit(Op.Pushunit);

			if (s.In != null)
				EndScope();
		}


		private void CompileWhere(ElaBinding s, LabelMap map, Hints hints)
		{
			StartScope(false);
			CompileExpression(s, map, hints);
		}


		private void CompileIn(ElaBinding s, LabelMap map, Hints hints)
		{
			if (s.In != null)
			{
				if ((hints & Hints.Scope) != Hints.Scope)
					StartScope(false);

				var newHints = (hints & Hints.And) == Hints.And ?
					hints ^ Hints.And : hints;
				CompileExpression(s.In, map, newHints | Hints.Scope);

				if ((hints & Hints.Scope) != Hints.Scope)
					EndScope();
			}
		}


		private void CompileBindingPattern(ElaBinding s, LabelMap map)
		{
			if (s.Pattern.Type == ElaNodeType.DefaultPattern)
			{
				if (s.Where != null)
					CompileWhere(s.Where, map, Hints.Left);

				CompileExpression(s.InitExpression, map, Hints.Nested);

				if (s.Where != null)
					EndScope();

				cw.Emit(Op.Pop);
			}
			else
			{
				var next = cw.DefineLabel();
				var exit = cw.DefineLabel();
				var addr = -1;
				var tuple = default(ElaTupleLiteral);

				if (s.InitExpression.Type == ElaNodeType.TupleLiteral && s.Pattern.Type == ElaNodeType.TuplePattern && s.Where == null)
					tuple = (ElaTupleLiteral)s.InitExpression;
				else
				{
					if (s.Where != null)
						CompileWhere(s.Where, map, Hints.Left);

					CompileExpression(s.InitExpression, map, Hints.None);

					if (s.Where != null)
						EndScope();

					addr = AddVariable();
					cw.Emit(Op.Popvar, addr);
				}

				CompilePattern(addr, tuple, s.Pattern, map, next, s.VariableFlags, Hints.Silent);
				cw.Emit(Op.Br, exit);
				cw.MarkLabel(next);
				cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
				cw.MarkLabel(exit);
				cw.Emit(Op.Nop);
			}
		}
		#endregion
	}
}