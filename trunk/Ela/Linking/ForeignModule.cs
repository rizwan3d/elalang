using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Compilation;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Linking
{
	public abstract class ForeignModule
	{
		#region Construction
		private FastList<ElaValue> locals;
        private Dictionary<String,Int32> pervasives;
		private Scope scope;

		protected ForeignModule()
		{
			locals = new FastList<ElaValue>();
			scope = new Scope(false, null);
		}
		#endregion


		#region Methods
		public abstract void Initialize();


		internal IntrinsicFrame Compile()
		{
			var frame = new IntrinsicFrame(locals.ToArray());
			frame.Layouts.Add(new MemoryLayout(locals.Count, 0, 0));
			frame.GlobalScope = scope;

            if (pervasives != null)
                foreach (var kv in pervasives)
                    frame.DeclaredPervasives.Add(kv.Key, 0 | kv.Value << 8);
            
			return frame;
		}


		protected void Add(string name, int val)
		{
			Add(name, new ElaValue(val));
		}


		protected void Add(string name, long val)
		{
			Add(name, new ElaValue(val));
		}


		protected void Add(string name, float val)
		{
			Add(name, new ElaValue(val));
		}


		protected void Add(string name, double val)
		{
			Add(name, new ElaValue(val));
		}


		protected void Add(string name, char val)
		{
			Add(name, new ElaValue(val));
		}


		protected void Add(string name, string val)
		{
			Add(name, new ElaValue(val));
		}


		protected void Add(string name, bool val)
		{
			Add(name, new ElaValue(val));
		}


		protected void Add(string name, ElaObject obj)
		{
			Add(name, new ElaValue(obj));
		}


        protected void Add<T1>(string name, ElaFun<T1> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1>(name, fun)));
		}
		

		protected void Add<T1,T2>(string name, ElaFun<T1,T2> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1,T2>(name, fun)));
		}


		protected void Add<T1,T2,T3>(string name, ElaFun<T1,T2,T3> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1,T2,T3>(name, fun)));
		}


		protected void Add<T1,T2,T3,T4>(string name, ElaFun<T1,T2,T3,T4> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1,T2,T3,T4>(name, fun)));
		}


		protected void Add<T1,T2,T3,T4,T5>(string name, ElaFun<T1,T2,T3,T4,T5> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1,T2,T3,T4,T5>(name, fun)));
		}


        protected void AddPervasive(string name, ElaObject value)
        {
            AddPervasive(name, new ElaValue(value));
        }


        protected void AddPervasive(string name, ElaValue value)
        {
            if (pervasives == null || !pervasives.ContainsKey(name))
            {
                var addr = locals.Count;
                scope.Locals.Add(name, new ScopeVar(ElaVariableFlags.None, addr, -1));
                locals.Add(value);

                if (pervasives == null)
                    pervasives = new Dictionary<String,Int32>();

                pervasives.Add(name, addr);
            }
        }


		private void Add(string name, ElaValue value)
		{
			scope.Locals.Add(name, new ScopeVar(ElaVariableFlags.None, locals.Count, -1));
			locals.Add(value);
		}
		#endregion
	}
}
