using System;

namespace Ela.CodeModel
{
    public enum ElaBuiltinKind
    {
        /* One argument */
        None,
        Not,
        Force,
        Length,
        Head,
        Tail,
        IsNil,
        Negate,
        BitwiseNot,
                
        Api1,
        Api2,
        Api3,
        Api4,
        Api5,
        Api6,
        Api7,
        Api8,
        Api9,
        
        /* Two arguments */
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
        GetValue,
        GetValueR,
        GetField,
        HasField,

        Api101,
        Api102,
        Api103,
        Api104,
        Api105,
        Api106,
        Api107,
    }
}