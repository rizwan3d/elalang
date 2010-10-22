using System;
using Ela.Compilation;
using Ela.Linking;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
	public class WorkerThread
	{
		#region Construction
		internal const int EndAddress = 0;

		private WorkerThread()
		{
			
		}
		
		internal WorkerThread(CodeAssembly asm)
		{
			Assembly = asm;
			Module = asm.GetRootModule();
			EvalStack = new EvalStack();
			CallStack = new FastStack<CallPoint>();
		}
		#endregion


		#region Methods
		internal void SwitchModule(int handle)
		{
			ModuleHandle = handle;
			Module = Assembly.GetModule(handle);
		}


		internal WorkerThread Clone()
		{
			var ret = new WorkerThread();
			ret.Assembly = Assembly;
			ret.EvalStack = EvalStack.Clone();
			ret.CallStack = new FastStack<CallPoint>();
			ret.Module = Assembly.GetRootModule();
			ret.Offset = 1;
			return ret;
		}
		#endregion


		#region Properties
		internal CodeAssembly Assembly { get; private set; }

		internal EvalStack EvalStack { get; private set; }

		internal FastStack<CallPoint> CallStack { get; private set; }

		internal int Offset { get; set; }

		internal bool Busy { get; set; }

		internal CodeFrame Module { get; private set; }

		internal int ModuleHandle { get; private set; }
		
		internal ElaLazy ReturnValue { get; set; }
		#endregion
	}
}