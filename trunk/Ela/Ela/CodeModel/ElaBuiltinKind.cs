using System;

namespace Ela.CodeModel
{
    public enum ElaBuiltinKind
    {
        None,

        Recip,

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

        Untag,

        Negate,

        BitwiseNot,

        

        Showf,

        Cast,
        
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