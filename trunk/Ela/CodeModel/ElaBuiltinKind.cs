using System;

namespace Ela.CodeModel
{
	public enum ElaBuiltinKind
	{
		None,

		Not,

		Flip,

		Force,

		Length,
				
		Type,

		Succ,

		Pred,

		Max,

		Min,

		Nil,

        Head,

        Tail,

        IsNil,

        Fst,

        Snd,

        Fst3,

        Snd3,

		Clone,

        Gettag,

        Untag,

        Apply,

        
		Negate,

		BitwiseNot,


		Showf,


        GetValue,

        Add,

		Subtract,

		Multiply,

		Divide,

		Remainder,

		Power,

		Cons,

		Equal,

		NotEqual,

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