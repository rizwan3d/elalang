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

		Add,

		Mul,

		Div,
        		
		Sub,


        Concat,

        Rem,

        Pow,

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

		Untag,

		Skiphtag,

		Nil,

        Gettag,


        Pushext,

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

		Popfld,

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