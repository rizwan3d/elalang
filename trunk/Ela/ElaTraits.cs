using System;

namespace Ela
{
	public enum ElaTraits
	{
		None = 0x00,

		Eq = 0x02,

		Ord = 0x04,

		Len = 0x08,

		Get = 0x10,

		Set = 0x20,

		Call = 0x40,

		Gen = 0x80,

		// = 0x100,

		Bound = 0x200,

		Enum = 0x400,

		Num = 0x800,

		Bit = 0x1000,

		Concat = 0x2000,

		Neg = 0x4000,

		Thunk = 0x8000,

		Seq = 0x10000,

		Fold = 0x20000,

		Show = 0x40000,

		FieldGet = 0x80000,

		FieldSet = 0x100000,

		Bool = 0x200000,

		Cons = 0x400000,

		Convert = 0x800000,

		// = 0x1000000
	}
}
