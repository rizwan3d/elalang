using System;

namespace Ela.CodeModel
{
	[Flags]
	public enum ElaVariableFlags
	{
		None = 0x00,

		Immutable = 0x01,

		Private = 0x02,

		Module = 0x04,

		Function = 0x08,

		ObjectLiteral = 0x10,

		SpecialName = 0x20,

		Parameter = 0x40
	}
}
