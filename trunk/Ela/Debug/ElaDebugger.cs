using System;
using System.Collections.Generic;
using Ela.Runtime;
using Ela.Linking;

namespace Ela.Debug
{
	public sealed class ElaDebugger
	{
		#region Construction
		private const string FUNC = "<function>";
		private const string FUNC_PARS = "<function@{0}>";
		
		internal ElaDebugger(CodeAssembly asm)
		{
			Assembly = asm;
		}
		#endregion



		#region Methods
		public CallStack BuildCallStack(WorkerThread thread)
		{
			var syms = new DebugReader(thread.Module.Symbols);
			var frames = new List<CallFrame>();			
			var lp = syms.FindLineSym(thread.Offset - 1);			
			var retval = new CallStack(
					thread.Module.File,
					lp != null ? lp.Line : 0,
					lp != null ? lp.Column : 0,
					frames
				);

			var callStack = thread.CallStack.Clone();
			var mem = default(CallPoint);
			var offset = thread.Offset - 1;
			var list = new List<CallFrame>();

			do
			{
				mem = callStack.Pop();
				var glob = mem.ReturnAddress == WorkerThread.EndAddress;
				var funSym = !glob ? syms.FindFunSym(offset) : null;
				var line = syms != null ? syms.FindLineSym(offset) : null;
				var mod = Assembly.GetModuleName(mem.ModuleHandle);
				frames.Add(new CallFrame(glob, mod,
					funSym != null ? 
						funSym.Name != null ? funSym.Name : String.Format(FUNC_PARS, funSym.Parameters) :
						glob ? null : FUNC,
					offset, line));
				offset = mem.ReturnAddress;
			}
			while (callStack.Count > 0);

			return retval;
		}
		#endregion


		#region Properties
		public CodeAssembly Assembly { get; private set; }
		#endregion
	}
}
