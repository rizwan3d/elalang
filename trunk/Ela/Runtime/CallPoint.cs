using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
	internal sealed class CallPoint
	{
		#region Construction
		internal CallPoint(int retAddr, int modHandle, int offset,
			ElaLazy lazy, RuntimeValue[] locals, FastList<RuntimeValue[]> captures)
		{
			ReturnAddress = retAddr;
			ModuleHandle = modHandle;
			StackOffset = offset;
			ReturnValue = lazy;
			Locals = locals;
			Captures = captures;
		}
		#endregion


		#region Fields
		internal int ReturnAddress;
				
		internal readonly int ModuleHandle;
		
		internal readonly int StackOffset;
		
		internal ElaLazy ReturnValue;
		
		internal readonly RuntimeValue[] Locals;

		internal readonly FastList<RuntimeValue[]> Captures;

		internal int? CatchMark;
		#endregion
	}
}
