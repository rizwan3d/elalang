using System;

namespace Ela.Runtime.ObjectModel
{
	internal abstract class DelegateFunction : ElaFunction
	{
		protected DelegateFunction(int args, string name) : base(args)
		{
			Name = name;
		}

		protected override string GetFunctionName()
		{
			return Name;
		}

		protected string Name { get; private set; }
	}


	internal sealed class DelegateFunction<T1> : DelegateFunction
	{
		private Func<T1> func;

		internal DelegateFunction(string name, Func<T1> func) : base(1, name)
		{
			this.func = func;
		}

		public override ElaValue Call(params ElaValue[] args)
		{
			args[0].AsUnit();
			return ElaValue.FromObject(func());
		}

		public override ElaFunction Clone()
		{
			return CloneFast(new DelegateFunction<T1>(Name, func));
		}
	}

	internal sealed class DelegateFunction<T1,T2> : DelegateFunction
	{
		private Func<T1,T2> func;

		internal DelegateFunction(string name, Func<T1,T2> func) : base(1, name)
		{
			this.func = func;
		}

		public override ElaValue Call(params ElaValue[] args)
		{
			return ElaValue.FromObject(func(args[0].Convert<T1>()));
		}

		public override ElaFunction Clone()
		{
			return CloneFast(new DelegateFunction<T1, T2>(Name, func));
		}
	}

	internal sealed class DelegateFunction<T1,T2,T3> : DelegateFunction
	{
		private Func<T1,T2,T3> func;

		internal DelegateFunction(string name, Func<T1,T2,T3> func) : base(2, name)
		{
			this.func = func;
		}

		public override ElaValue Call(params ElaValue[] args)
		{
			return ElaValue.FromObject(func(args[0].Convert<T1>(), args[1].Convert<T2>()));
		}

		public override ElaFunction Clone()
		{
			return CloneFast(new DelegateFunction<T1,T2,T3>(Name, func));
		}
	}

	internal sealed class DelegateFunction<T1,T2,T3,T4> : DelegateFunction
	{
		private Func<T1,T2,T3,T4> func;

		internal DelegateFunction(string name, Func<T1,T2,T3,T4> func) : base(3, name)
		{
			this.func = func;
		}

		public override ElaValue Call(params ElaValue[] args)
		{
			return ElaValue.FromObject(func(args[0].Convert<T1>(), args[1].Convert<T2>(), args[2].Convert<T3>()));
		}

		public override ElaFunction Clone()
		{
			return CloneFast(new DelegateFunction<T1,T2,T3,T4>(Name, func));
		}
	}

	internal sealed class DelegateFunction<T1,T2,T3,T4,T5> : DelegateFunction
	{
		private Func<T1,T2,T3,T4,T5> func;

		internal DelegateFunction(string name, Func<T1,T2,T3,T4,T5> func) : base(4, name)
		{
			this.func = func;
		}

		public override ElaValue Call(params ElaValue[] args)
		{
			return ElaValue.FromObject(func(args[0].Convert<T1>(), args[1].Convert<T2>(), args[2].Convert<T3>(), args[3].Convert<T4>()));
		}

		public override ElaFunction Clone()
		{
			return CloneFast(new DelegateFunction<T1,T2,T3,T4,T5>(Name, func));
		}
	}
}
