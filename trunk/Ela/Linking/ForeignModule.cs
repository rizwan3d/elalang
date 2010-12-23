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


		private void Add(string name, ElaValue value)
		{
			scope.Locals.Add(name, new ScopeVar(ElaVariableFlags.Immutable, locals.Count, -1));
			locals.Add(value);
		}
		#endregion
	}
}
