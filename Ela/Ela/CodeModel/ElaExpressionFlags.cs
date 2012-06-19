using System;

namespace Ela.CodeModel
{
	[Flags]
	public enum ElaExpressionFlags
	{
		None,

		ReturnsUnit = 0x02,

        Lazy = 0x04
	}
}
