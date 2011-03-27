using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	internal struct UnresolvedSymbol
	{
		internal readonly string Name;
		internal readonly int Address;
		internal readonly int Data;
		internal readonly int Line;
		internal readonly int Column;

		internal UnresolvedSymbol(string name, int address, int data, int line, int col)
		{
			Name = name;
			Address = address;
			Data = data;
			Line = line;
			Column = col;
		}
	}
}
