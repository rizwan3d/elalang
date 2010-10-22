using System;

namespace Ela.Runtime
{
	public sealed class ExecutionResult
	{
		#region Construction
		internal ExecutionResult(RuntimeValue val)
		{
			ReturnValue = val;
		}
		#endregion


		#region Properties
		public RuntimeValue ReturnValue { get; private set; }
		#endregion
	}
}