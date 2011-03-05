using System;
using System.Collections.Generic;

namespace Ela.Debug
{
	public sealed class DebugReader
	{
		#region Construction
		public DebugReader(DebugInfo symbols)
		{
			Symbols = symbols;
		}
		#endregion


		#region Methods
		public FunSym GetFunSymByHandle(int handle)
		{
			for (var i = 0; i < Symbols.Functions.Count; i++)
                if (Symbols.Functions[i].Handle == handle)
                    return Symbols.Functions[i];

            return null;
		}


        public FunSym FindFunSym(int offset)
		{
			var fun = default(FunSym);

			for (var i = 0; i < Symbols.Functions.Count; i++)
			{
				var f = Symbols.Functions[i];

				if (offset > f.StartOffset && offset < f.EndOffset)
					fun = f;
			}

			return fun;
		}


		public LineSym FindLineSym(int offset)
		{
			for (var i = 0; i < Symbols.Lines.Count; i++)
			{
				var l = Symbols.Lines[i];

				if (l.Offset == offset)
					return l;
			}

			return offset == 0 ? null : FindLineSym(offset - 1);
		}


		public ScopeSym FindScopeSym(int offset)
		{
			var scope = default(ScopeSym);

			for (var i = 0; i < Symbols.Scopes.Count; i++)
			{
				var s = Symbols.Scopes[i];

				if (offset > s.StartOffset && offset < s.EndOffset)
					scope = s;
			}

			return scope;
		}


		public VarSym FindVarSym(int address, int scopeIndex)
		{
			for (var i = 0; i < Symbols.Vars.Count; i++)
			{
				var v = Symbols.Vars[i];

				if (v.Address == address && v.Scope == scopeIndex)
					return v;
			}

			return default(VarSym);
		}


		public IEnumerable<VarSym> FindVarSyms(int offset, ScopeSym scope)
		{
			for (var i = 0; i < Symbols.Vars.Count; i++)
			{
				var v = Symbols.Vars[i];

				if ((scope == null && v.Scope == 0 || v.Scope == scope.Index) &&
					v.Offset <= offset)
					yield return v;
			}
		}


		public string PrintSymTables(SymTables tables)
		{
			var gen = new SymGenerator(Symbols);
			return gen.Generate(tables);
		}
		#endregion


		#region Properties
		internal DebugInfo Symbols { get; private set; }
		#endregion
	}
}
