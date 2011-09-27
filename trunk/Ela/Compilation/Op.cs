using System;

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

		Popelem,

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

		Newmod,

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

		Max,

		Min,

		Show,

		Untag,

		Nil,

        Clone,
		
        Gettag,

        Ovr,
				
        Conv,

		Has,


        Callb1,

        Callb2,

        Callb3,
        
        Makefun,

		Newvar,

		Newrec,

		Newtup,

		Tupex,

		Failwith,

		Start,

		Pushstr,

		PushCh,

		PushI4,

		PushR4,

		Pushvar,

		Popvar,

		Runmod,

		Br,

		Brtrue,

		Brfalse,

		Newfun        
	}
}