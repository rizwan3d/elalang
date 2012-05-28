using System;
using Ela.CodeModel;
using Ela.Debug;

namespace Ela.Compilation
{
	public struct LateBoundSymbol
	{
        public readonly string Name;
        public readonly int Address;
        public readonly int Data;
        public readonly int Line;
        public readonly int Column;

		internal LateBoundSymbol(string name, int address, int data, int line, int col)
		{
			Name = name;
			Address = address;
			Data = data;
			Line = line;
			Column = col;
		}
	}
}
