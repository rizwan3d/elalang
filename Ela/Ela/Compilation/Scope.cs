﻿using System;
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
			foreach (var kv in Locals)
				if ((kv.Value.Flags & ElaVariableFlags.SpecialName) != ElaVariableFlags.SpecialName &&
					((kv.Value.Flags & ElaVariableFlags.External) != ElaVariableFlags.External))
					yield return kv.Key;
		}

        
        public IEnumerable<KeyValuePair<String,ScopeVar>> EnumerateVars()
        {
            foreach (var kv in Locals)
                if ((kv.Value.Flags & ElaVariableFlags.SpecialName) != ElaVariableFlags.SpecialName &&
                    ((kv.Value.Flags & ElaVariableFlags.External) != ElaVariableFlags.External))
                    yield return kv;
        }


		internal void ChangeVariable(string name, ScopeVar var)
		{
			Locals[name] = var;
		}

        internal int TryChangeVariable(string name, ElaVariableFlags flags)
        {
            var v = default(ScopeVar);

            if (Locals.TryGetValue(name, out v))
            {
                v = new ScopeVar(flags, v.Address, -1);
                Locals[name] = v;
                return v.Address;
            }

            return -1;
        }
		#endregion


		#region Properties
		internal Scope Parent { get; set; }

		internal Dictionary<String,ScopeVar> Locals { get; private set; }

		internal bool Function { get; private set; }
		#endregion
	}
}