using System;

namespace Ela.CodeModel
{
    public enum ElaBuiltinKind
    {
        None,

        Not,

        Force,

        Length,

        Type,

        Succ,

        Pred,

        Head,

        Tail,

        IsNil,

        Gettag,

        Negate,

        BitwiseNot,

        

        Showf,

        ForwardPipe,

        BackwardPipe,


        Add,

        Subtract,

        Multiply,

        Divide,

        Quot,

        Remainder,

        Modulus,

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

        Get,

        RecField
    }
}