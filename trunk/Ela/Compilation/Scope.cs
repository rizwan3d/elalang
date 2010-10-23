using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
	public sealed class Scope
	{
		#region Construction
		internal Scope(bool fun, Scope parent)
		{
			Function = fun;
			Parent = parent;
			Locals = new Dictionary<String,ScopeVar>();
		}
		#endregion


		#region Methods
		public ScopeVar GetVariable(string name)
		{
			var var = default(ScopeVar);

			if (!Locals.TryGetValue(name, out var))
				var = ScopeVar.Empty;

			return var;
		}


		public Scope Clone()
		{
			var ret = new Scope(Function, Parent);
			ret.Locals = new Dictionary<String,ScopeVar>(Locals);
			return ret;
		}


		public IEnumerable<String> EnumerateNames()
		{
			foreach (var s in Locals.Keys)
				yield return s;
		}


		internal void ChangeVariable(string name, ScopeVar var)
		{
			Locals[name] = var;
		}
		#endregion


		#region Properties
		internal Scope Parent { get; private set; }

		internal Dictionary<String,ScopeVar> Locals { get; private set; }

		internal bool Function { get; private set; }
		#endregion
	}
}
