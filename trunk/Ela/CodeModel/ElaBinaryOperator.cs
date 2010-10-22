using System;

namespace Ela.CodeModel
{
	public enum ElaBinaryOperator
	{
		None,

		Add,

		Subtract,

		Multiply,

		Divide,

		Modulus,

		Power,

		ConsList,

		Equals,

		NotEquals,

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

		PipeForward,

		PipeBackward,

		Custom
	}
}