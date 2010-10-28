using System;

namespace Ela.Runtime
{
	internal struct Pervasive
	{
		#region Construction
		internal Pervasive(int module, int address)
		{
			Module = module;
			Address = address;
		}
		#endregion


		#region Fields
		internal readonly int Module;

		internal readonly int Address;
		#endregion
	}
}
