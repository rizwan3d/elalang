using System;
using Ela.Compilation;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using Ela.CodeModel;

namespace Ela.Linking
{
	public abstract class ForeignModule
	{
		#region Construction
		private FastList<ElaValue> locals;
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


		protected void Add<T1>(string name, Func<T1> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1>(name, fun)));
		}
		

		protected void Add<T1,T2>(string name, Func<T1,T2> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1,T2>(name, fun)));
		}


		protected void Add<T1,T2,T3>(string name, Func<T1,T2,T3> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1,T2,T3>(name, fun)));
		}


		protected void Add<T1,T2,T3,T4>(string name, Func<T1,T2,T3,T4> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1,T2,T3,T4>(name, fun)));
		}


		protected void Add<T1,T2,T3,T4,T5>(string name, Func<T1,T2,T3,T4,T5> fun)
		{
			Add(name, new ElaValue(new DelegateFunction<T1,T2,T3,T4,T5>(name, fun)));
		}


		private void Add(string name, ElaValue value)
		{
			scope.Locals.Add(name, new ScopeVar(ElaVariableFlags.Immutable, locals.Count, -1));
			locals.Add(value);
		}
		#endregion
	}
}
