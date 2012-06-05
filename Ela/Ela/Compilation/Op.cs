﻿using System;

namespace Ela.Compilation
{
    public enum Op
    {
        Nop,

        Len,

        Pushunit,

        Pushelem,

        PushI4_0,

        PushI1_0,

        PushI1_1,

        Pop,

        Pushstr_0,

        Reccons,

        Genfin,

        Cons,

        Gen,

        Tail,

        Head,

        Ret,

        Concat,

        Add,

        Mul,

        Div,

        Rem,

        Pow,

        Sub,

        Shr,

        Shl,

        Ceq,

        Cneq,

        Clt,

        Cgt,

        Cgteq,

        Clteq,

        AndBw,

        OrBw,

        Xor,

        Not,

        Neg,

        NotBw,

        Dup,

        Swap,

        Newlazy,

        Newlist,

        Newtup_2,

        Typeid,

        Stop,

        NewI8,

        NewR8,

        Leave,

        Flip,

        LazyCall,

        Call,

        Callt,

        Throw,

        Rethrow,

        Force,

        Type,

        Isnil,

        Succ,

        Pred,

        Show,

        Untag,

        Skiphtag,

        Nil,

        Gettag,

        Conv,


        Newmod,

        Pushext,

        Elem,

        Skiptag,

        Newvar,

        Newrec,

        Newtup,

        Skiptn,

        Skiptl,

        Pat,

        Tupex,

        Failwith,

        Start,

        Pushstr,

        PushCh,

        PushI4,

        PushR4,

        Pushvar,

        Pushfld,

        Popvar,

        Runmod,

        Has,

        Br,

        Brtrue,

        Brfalse,

        Br_lt,

        Br_gt,

        Br_eq,

        Br_neq,

        Brnil,

        Newfun
    }
}