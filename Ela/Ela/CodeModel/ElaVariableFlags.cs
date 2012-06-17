using System;

namespace Ela.CodeModel
{
	[Flags]
	public enum ElaVariableFlags
	{
		None = 0x00,

		External = 1 << 0,

        Private = 1 << 1,

        Module = 1 << 2,

        Function = 1 << 3,

        ObjectLiteral = 1 << 4,

        SpecialName = 1 << 5,

        Parameter = 1 << 6,

        Builtin = 1 << 7,

        Inline = 1 << 8,

        NoInit = 1 << 9,

        // = 1 << 10,

        ClassFun = 1 << 11,

        TypeFun = 1 << 12
	}
}
