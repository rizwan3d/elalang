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
            CallStack = new FastStack<CallPoint>();
			Context = new ExecutionContext();
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
			ret.CallStack = new FastStack<CallPoint>();
			ret.Module = Assembly.GetRootModule();
			ret.Context = new ExecutionContext();
			ret.Offset = 1;
			return ret;
		}
		#endregion


		#region Properties
		internal CodeAssembly Assembly { get; private set; }

		internal FastStack<CallPoint> CallStack { get; private set; }

		internal int Offset { get; set; }

		internal CodeFrame Module { get; private set; }

		internal int ModuleHandle { get; private set; }

		internal ExecutionContext Context { get; private set; }
		#endregion
	}
}