using System;

namespace Ela.Compilation
{
	public enum Op
	{
		Nop,

		Len,

		Pushunit,

		Pushptr,

		Pushelem,

		PushI4_0,

		PushI1_0,

		PushI1_1,

		Pop,

		Popelem,

		Pushseq,

		Pushstr_0,

		Listadd,

		Listgen,

		Listtail,

		Listelem,

		Ret,

		ConvI1,

		ConvI4,

		ConvR4,

		ConvStr,

		ConvCh,

		ConvI8,

		ConvR8,

		ConvSeq,

		Add,

		Mul,

		Div,

		Rem,

		Pow,

		Sub,

		Shr,

		Shl,

		Ctag,

		Htag,

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

		Pos,

		NotBw,

		Dup,

		Calla,

		Valueof,

		Newseq,

		Newlist,

		Newlazy,

		Newmod,

		Newarr_0,

		Newtup_2,

		Cout,

		Throw,

		Typeof,

		Term,

		NewI8,

		NewR8,

		Leave,

		Calld,

		Callt,


		Start,

		Call,

		Incr,

		Decr,

		Cop,

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

		Brptr,

		Br_lt,

		Br_gt,

		Br_eq,

		Br_neq,

		Brdyn,

		Yield,

		Newfun,

		Newfund,

		Newfuns,

		Newrec,

		Newarr,

		Newtup,

		Settag,

		Epilog,
	}
}