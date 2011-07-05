using System;
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
		private FastList<TernaryOverload> ternaryOverloads;
		private FastList<BinaryOverload> binaryOverloads;
		private FastList<UnaryOverload> unaryOverloads;
        private Scope scope;

		protected ForeignModule()
		{
			locals = new FastList<ElaValue>();
			ternaryOverloads = new FastList<TernaryOverload>();
			binaryOverloads = new FastList<BinaryOverload>();
			unaryOverloads = new FastList<UnaryOverload>();
			scope = new Scope(false, null);			
		}
		#endregion


		#region Methods
		public abstract void Initialize();


		public virtual void RegisterTypes(TypeRegistrator registrator)
		{

		}


		public virtual void Close()
		{

		}


		internal IntrinsicFrame Compile(ExportVars exports)
		{
			var frame = new IntrinsicFrame(locals.ToArray(), binaryOverloads, unaryOverloads, ternaryOverloads, this);
			frame.Layouts.Add(new MemoryLayout(locals.Count, 0, 0));
			frame.GlobalScope = scope;

            foreach (var sc in scope.Locals)
                exports.AddName(sc.Key);

			return frame;
		}


        internal bool IsRegistered(string name)
        {
            return scope.Locals.ContainsKey(name);
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


		protected void AddOverload(ElaTernaryFunction kind, DispatchTernaryFun fun, TypeId arg1, TypeId arg2)
		{
			ternaryOverloads.Add(new TernaryOverload(fun, arg1, arg2, kind));
		}


		protected void AddOverload(ElaBinaryFunction kind, DispatchBinaryFun fun, TypeId arg1, TypeId arg2)
		{
			binaryOverloads.Add(new BinaryOverload(fun, arg1, arg2, kind));
		}


		protected void AddOverload(ElaUnaryFunction kind, DispatchUnaryFun fun, TypeId arg)
		{
			unaryOverloads.Add(new UnaryOverload(fun, arg, kind));
		}


		protected void Add(string name, ElaValue value)
		{
			scope.Locals.Add(name, new ScopeVar(ElaVariableFlags.None, locals.Count, -1));
			locals.Add(value);
		}
		#endregion
    }
}
