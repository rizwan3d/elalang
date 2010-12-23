using System;

namespace Ela.CodeModel
{
	[Flags]
	public enum ElaExpressionFlags
	{
		None,

		Assignable = 0x02,

		ReturnsUnit = 0x04,

		BreaksExecution = 0x08
	}
}
