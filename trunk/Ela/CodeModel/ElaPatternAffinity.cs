using System;

namespace Ela.CodeModel
{
	internal enum ElaPatternAffinity
	{
		Any = 0x00,

		Integer = 0x02,

		String = 0x04,

		Char = 0x08,

		Float = 0x10,
				
		Boolean = 0x80,

		List = 0x100,

		Array = 0x200,

		Sequence = 0x400
	}
}
