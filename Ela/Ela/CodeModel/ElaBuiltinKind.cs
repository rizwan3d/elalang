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

        Head,

        Tail,

        IsNil,

        Gettag,

        Untag,

        Negate,

        BitwiseNot,

        

        Showf,

        Readf,

        Cast,
        
        ForwardPipe,

        BackwardPipe,


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

        Get,

        RecField
    }
}