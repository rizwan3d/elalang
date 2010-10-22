using System;

namespace Ela.Runtime
{
	internal struct Pervasive
	{
		#region Construction
		internal Pervasive(int module, int name)
		{
			Module = module;
			Name = name;
		}
		#endregion


		#region Fields
		internal readonly int Module;

		internal readonly int Name;
		#endregion
	}
}
