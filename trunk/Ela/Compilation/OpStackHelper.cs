using System;

namespace Ela.Compilation
{
	internal static class OpStackHelper
	{
		internal static int[] Op = 
		{
			0, //Nop
			0, //Len
			1, //Pushunit
			-1, //Pushelem
			1, //PushI4_0
			1, //PushI1_0
			1, //PushI1_1
			-1, //Pop
			-2, //Popelem
			1, //Pushstr_0
			-3, //Reccons
			0, //Genfin
			-1, //Cons
			-1, //Gen
			0, //Tail
			0, //Head
			0, //Ret
			-1, //Concat
			-1, //Add
			-1, //Mul
			-1, //Div
			-1, //Rem
			-1, //Pow
			-1, //Sub
			-1, //Shr
			-1, //Shl
			-1, //Ceq
			-1, //Cneq
			-1, //Clt
			-1, //Cgt
			-1, //Cgteq
			-1, //Clteq
			-1, //AndBw
			-1, //OrBw
			-1, //Xor
			0, //Not
			0, //Neg
			0, //NotBw
			1, //Dup
			0, //Swap
			1, //Newlazy
			1, //Newlist
			1, //Newmod
			-1, //Newtup_2
			0, //Typeid
			0, //Stop
			-1, //NewI8
			-1, //NewR8
			0, //Leave
			1, //Flip,
			-1, //LazyCall
			-1, //Call
			-1, //Callt
			-2, //Throw
			-1, //Rethrow
			0, //Force
			0, //TypeId
			0, //Isnil
			0, //Succ
			0, //Pred
			0, //Max
			0, //Min
			-1, //Show
			0, //Untag
			0, //Nil
            0, //Clone			
            0, //Gettag            
            -3, //Ovr            	
	        -1, //Conv
            -1, //Has
			
            0, //Newvar
			1, //Newrec
			1, //Newtup
			-1, //Tupex
			0, //Failwith
			0, //Start
			1, //Pushstr
			1, //PushCh
			1, //PushI4
			1, //PushR4
			1, //Pushvar
			-1, //Popvar
			0, //Runmod
			0, //Br
			-1, //Brtrue
			-1, //Brfalse
			0, //Newfun	
		};
	}
}