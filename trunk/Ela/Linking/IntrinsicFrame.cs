using System;
using Ela.Compilation;
using Ela.Runtime;

namespace Ela.Linking
{
	public sealed class IntrinsicFrame : CodeFrame
	{
		#region Construction
		internal IntrinsicFrame(RuntimeValue[] mem)
		{
			Memory = mem;
		}
		#endregion


		#region Properties
		internal RuntimeValue[] Memory { get; private set; }
		#endregion
	}
}
