﻿using System;

namespace Ela.Compilation
{
	[Flags]
	internal enum Hints
	{
		None = 0x00,

		Left = 0x01,

		Assign = 0x02,

		Scope = 0x04,

		__Reserved1 = 0x08,

		__Reserved2 = 0x10,

		Tail = 0x20,

		Throw = 0x40,

		Nested = 0x80,

		And = 0x100,

		FunBody = 0x200,

		Silent = 0x400,
	}
}