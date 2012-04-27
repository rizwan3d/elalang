﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Debug
{
	internal sealed class DebugWriter
	{
		#region Construction
		private FastStack<ScopeSym> scopes;
		private FastStack<FunSym> funs;
		
		internal DebugWriter()
		{
			Symbols = new DebugInfo();
			scopes = new FastStack<ScopeSym>();
			funs = new FastStack<FunSym>();
			var glob = new ScopeSym(0, 0, 0) { EndOffset = Int32.MaxValue };
			scopes.Push(glob);
			Symbols.Scopes.Add(glob);
		}
		#endregion


		#region Methods
		internal void StartFunction(string name, int offset, int pars)
		{
			funs.Push(new FunSym(name, offset, pars));
		}


		internal void EndFunction(int handle, int offset)
		{
			var f = funs.Pop();
			f.Handle = handle;
			f.EndOffset = offset;
			Symbols.Functions.Add(f);
		}


		internal void StartScope(int offset)
		{
            var index = Symbols.Scopes.Count + 1;
			scopes.Push(new ScopeSym(index, scopes.Peek().Index, offset));
		}


		internal void EndScope(int offset)
		{
			var s = scopes.Pop();
			s.EndOffset = offset;
			Symbols.Scopes.Add(s);
		}


		internal void AddVarSym(string name, int address, int offset)
		{
			Symbols.Vars.Add(new VarSym(name, address, offset, scopes.Peek().Index));
		}


		internal void AddLineSym(int offset, int line, int col)
		{
			Symbols.Lines.Add(new LineSym(offset, line, col));
		}
		#endregion


		#region Properties
		internal DebugInfo Symbols { get; private set; }		
		#endregion
	}
}
