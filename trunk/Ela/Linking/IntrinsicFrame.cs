using System;
using Ela.Compilation;
using Ela.Runtime;

namespace Ela.Linking
{
	public sealed class IntrinsicFrame : CodeFrame
	{
		#region Construction
		private ForeignModule module;

		internal IntrinsicFrame(ElaValue[] mem, ForeignModule module)
		{
			Memory = mem;
			this.module = module;
		}
		#endregion


		#region Properties
		internal ElaValue[] Memory { get; private set; }
		#endregion
	}
}
