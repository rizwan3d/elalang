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

			var rec = IsMutualRecursive(s);
			
			if (s.In != null)
				StartScope(false);
			
			if (s.Pattern == null)
			{
				var addr = -1;
				var addSym = false;
				
				if (s.InitExpression != null && s.InitExpression.Type == ElaNodeType.FunctionLiteral)
				{
					var fun = (ElaFunctionLiteral)s.InitExpression;
					addr = (hints & Hints.And) == Hints.And ?
						GetVariable(s.VariableName, s.Line, s.Column).Address : 
						AddVariable(s.VariableName, s, s.VariableFlags, -1);
				}
				else
				{
                    addr = -1;
                    addSym = true;
				}

				var po = cw.Offset;
				var and = s.And;

				while (and != null && (hints & Hints.And) != Hints.And && rec)
				{
					AddVariable(and.VariableName, and, and.VariableFlags, -1);
					and = and.And;
				}

				if (s.Where != null)
					CompileWhere(s.Where, map, Hints.Left);

				var ed = s.InitExpression != null ? CompileExpression(s.InitExpression, map, Hints.None) : default(ExprData);
				var fc = ed.Type == DataKind.FunCurry || ed.Type == DataKind.FunParams;

				if (ed.Type == DataKind.FunParams && addSym)
				{
					pdb.StartFunction(s.VariableName, po, ed.Data);
					pdb.EndFunction(-1, cw.Offset);
				}

				if (s.Where != null)
					EndScope();

				if (addr != -1 && fc)
					CurrentScope.ChangeVariable(s.VariableName, new ScopeVar(s.VariableFlags | ElaVariableFlags.Function, addr >> 8, ed.Data));
				else if (addr == -1)
					addr = AddVariable(s.VariableName, s, fc ? s.VariableFlags | ElaVariableFlags.Function : s.VariableFlags, ed.Data);

				AddLinePragma(s);
				cw.Emit(Op.Popvar, addr);
			}
			else
				CompileBindingPattern(s, map);

			var newHints = hints | (s.In != null ? Hints.Scope : Hints.None) | (rec ? Hints.And : Hints.None);

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


		#region Helper
		private bool IsMutualRecursive(ElaBinding s)
		{
			if (s.And != null)
			{
				if (s.InitExpression.Type != ElaNodeType.FunctionLiteral)
					return false;

				var and = s.And;

				do
				{
					if (and.InitExpression.Type != ElaNodeType.FunctionLiteral)
						return false;

					and = and.And;
				}
				while (and != null);
			}

			return true;
		}
		#endregion
	}
}