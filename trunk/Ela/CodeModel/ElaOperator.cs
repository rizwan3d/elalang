using System;

namespace Ela.CodeModel
{
	public enum ElaOperator
	{
		None,

		Assign,

		Add,

		Subtract,

		Multiply,

		Divide,

		Remainder,

		Power,

		Cons,

		Equals,

		NotEquals,

		Concat,

		Greater,

		Lesser,

		GreaterEqual,

		LesserEqual,

		BooleanAnd,

		BooleanOr,

		BitwiseAnd,

		BitwiseOr,

		BitwiseXor,
				

		ShiftRight,

		ShiftLeft,

		CompForward,

		CompBackward,

		Swap,

		Sequence,

        Negate,

        BitwiseNot
	}
}