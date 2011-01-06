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

		Newarr,

		Newtup_2,

		Cout,

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

		Tagval,

		Skiphtag,




		Skiptag,

		Isfun,

		Conv,

		Newvar,

		Newrec,

		Newtup,

		Skiptn,

		Skiptl,

		Trait,

		PushelemI4,

		Pushelemi,

		Popelemi,

		Tupex,

		Failwith,

		Start,

		Incr,

		Decr,

		Pushstr,

		PushCh,

		PushI4,

		PushR4,

		Pusharg,

		Pushperv,

		Pushvar,

		Pushfld,

		Popvar,

		Popfld,

		Runmod,

		Hasfld,

		Br,

		Brtrue,

		Brfalse,

		Br_lt,

		Br_gt,

		Br_eq,

		Br_neq,

		Newfun
	}
}