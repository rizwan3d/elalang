using System;

namespace Ela.Compilation
{
	internal sealed class MemoryLayout
	{
		#region Construction
		internal MemoryLayout(int size, int address)
		{
			Size = size;
			Address = address;
		}
		#endregion


		#region Properties
		internal int Size { get; set; }

		internal int Address { get; private set; }
		#endregion
	}
}