using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
	internal sealed class CallPoint
	{
		#region Construction
		internal CallPoint(int retAddr, int modHandle, ElaLazy lazy, RuntimeValue[] locals, FastList<RuntimeValue[]> captures)
		{
			ReturnAddress = retAddr;
			ModuleHandle = modHandle;
			ReturnValue = lazy;
			Locals = locals;
			Captures = captures;
		}
		#endregion


		#region Fields
		internal readonly int ReturnAddress;
				
		internal readonly int ModuleHandle;
		
		internal readonly RuntimeValue[] Locals;

		internal readonly FastList<RuntimeValue[]> Captures;
		
		internal ElaLazy ReturnValue;
		
		internal int? CatchMark;

		internal int CatchOffset;
		#endregion
	}
}
