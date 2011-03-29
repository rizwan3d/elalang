using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
	internal sealed class CallPoint
	{
		#region Construction
		internal CallPoint(int modHandle, EvalStack stack, ElaValue[] locals, FastList<ElaValue[]> captures)
		{
			ModuleHandle = modHandle;
			Locals = locals;
			Captures = captures;
			Stack = stack;
		}
		#endregion


		#region Fields
		internal readonly int ModuleHandle;

		internal readonly ElaValue[] Locals;

		internal readonly EvalStack Stack;

		internal readonly FastList<ElaValue[]> Captures;

		internal int BreakAddress;

		internal int StackOffset;

		internal int? CatchMark;

		internal ElaValue[] Table;
		#endregion
	}
}
