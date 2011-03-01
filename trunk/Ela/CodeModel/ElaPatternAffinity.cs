using System;

namespace Ela.CodeModel
{
    [Flags]
	internal enum ElaPatternAffinity
	{
        None = 0x00,

		Any = 0x02,

		Integer = 0x04,

		String = 0x08,

		Char = 0x10,

		Real = 0x20,
				
		Boolean = 0x40,

		Fold = 0x80,

		Sequence = 0x100,

        Record = 0x200
	}
}
