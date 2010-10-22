using System;

namespace Ela.CodeModel
{
	public enum ElaUnaryOperator
	{
		None,

		Negate,

		Positive,

		BooleanNot,

		BitwiseNot,

		Increment,

		Decrement,

		PostIncrement,

		PostDecrement,

		ValueOf,

		Custom
	}
}