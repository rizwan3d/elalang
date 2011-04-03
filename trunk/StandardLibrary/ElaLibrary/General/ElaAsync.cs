using System;
using ST = System.Threading;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
	public sealed class ElaAsync : ElaRefObject
	{
		#region Construction
		private const string TYPENAME = "async";
		internal readonly object SyncRoot = new Object();

		internal ElaAsync(AsyncModule mod, ElaFunction fun) : base(TYPENAME)
		{
			Initialize(mod, fun);
		}
		#endregion


		#region Methods
		protected override string GetTypeName()
		{
			return TYPENAME;
		}


		private void Initialize(AsyncModule mod, ElaFunction fun)
		{
			Thread = new ST.Thread(() => Return = fun.Call());
			mod.Threads.Add(Thread);
		}


		internal void Run()
		{
			Thread.Start();
		}
		#endregion


		#region Operations
		protected override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.ReferenceEquals(right));
		}


		protected override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(!left.ReferenceEquals(right));
		}


		protected override string Show(ElaValue @this, ShowInfo info, Ela.Runtime.ExecutionContext ctx)
		{
			return "[" + TYPENAME + "]";
		}
		#endregion


		#region Properties
		internal ST.Thread Thread { get; set; }

		internal ElaValue Return { get; private set; }
		#endregion
	}
}
