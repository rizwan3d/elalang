using System;

namespace Ela.CodeModel
{
	public enum ElaBuiltinKind
	{
		None,

		Typeid,

		Not,

		Flip,

		Force,

		Length,
				
		Type,

		Succ,

		Pred,

		Max,

		Min,

		Show,

		Nil,

        Head,

        Tail,

        IsNil,

        Fst,

        Snd,

        Fst3,

        Snd3,


		Negate,

		BitwiseNot,


		Showf,

		IsRef,

		
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

		BitwiseAnd,

		BitwiseOr,

		BitwiseXor,

		ShiftRight,

		ShiftLeft,

		CompForward,

		CompBackward
	}
}