using System;

namespace Ela.Debug
{
	public sealed class VarSym
	{
		#region Construction
		internal VarSym(string name, int address, int offset, int scope)
		{
			Name = name;
			Address = address;
			Offset = offset;
			Scope = scope;
		}
		#endregion


		#region Properties
		public string Name { get; private set; }

		public int Address { get; private set; }

		public int Offset { get; private set; }

		public int Scope { get; private set; }
		#endregion
	}
}
