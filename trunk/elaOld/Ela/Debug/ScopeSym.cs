using System;

namespace Ela.Debug
{
	public sealed class ScopeSym
	{
		#region Construction
		internal ScopeSym(int index, int parentIndex, int startOffset)
		{
			Index = index;
			ParentIndex = parentIndex;
			StartOffset = startOffset;
		}
		#endregion


		#region Properties
		public int Index { get; private set; }

		public int ParentIndex { get; private set; }

		public int StartOffset { get; private set; }

		public int EndOffset { get; internal set; }
		#endregion
	}
}
