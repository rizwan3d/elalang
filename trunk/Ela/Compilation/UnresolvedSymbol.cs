using System;

namespace Ela.Compilation
{
	internal struct UnresolvedSymbol
	{
		internal readonly string Name;
		internal readonly int Address;
		internal readonly int Line;
		internal readonly int Column;

		internal UnresolvedSymbol(string name, int address, int line, int col)
		{
			Name = name;
			Address = address;
			Line = line;
			Column = col;
		}
	}
}
