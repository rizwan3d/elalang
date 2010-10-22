using System;

namespace Ela.Compilation
{
	internal enum Hints
	{
		None = 0x00,

		Left = 0x01,

		Assign = 0x02,

		Scope = 0x04,

		Comp = 0x08,

		Tail = 0x10,

		Throw = 0x20,

		Nested = 0x40
	}
}