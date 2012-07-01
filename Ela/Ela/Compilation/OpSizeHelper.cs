using System;

namespace Ela.Compilation
{
    internal static class OpSizeHelper
    {
        internal static int[] OpSize = 
		{
			1, //Nop
			1, //Len
			1, //Pushunit
			1, //Pushelem
			1, //PushI4_0
			1, //PushI1_0
			1, //PushI1_1
			1, //Pop
			1, //Pushstr_0
			1, //Genfin
			1, //Cons
			1, //Gen
			1, //Tail
			1, //Header
			1, //Ret
			1, //Concat
			1, //Add
			1, //Mul
			1, //Div
			1, //Rem
			1, //Pow
			1, //Sub
			1, //Shr
			1, //Shl
			1, //Ceq
			1, //Cneq
			1, //Clt
			1, //Cgt
			1, //Cgteq
			1, //Clteq
			1, //AndBw
			1, //OrBw
			1, //Xor
			1, //Not
			1, //Neg
			1, //NotBw
			1, //Dup
			1, //Swap
			1, //Newlazy
			1, //Newlist
			1, //Newtup_2
			1, //Stop
			1, //NewI8
			1, //NewR8
			1, //Leave
			1, //Flip,
			1, //LazyCall
			1, //Call
			1, //Callt
			1, //Throw
			1, //Rethrow
			1, //Force
			1, //Type
			1, //Isnil
			1, //Succ
			1, //Pred
			1, //Show
			1, //Untag
			1, //Skiphtag
			1, //Gettag
			1, //Cast
            1, //Recfld
            1, //Addmbr            
            1, //Traitch

            5, //Newtype
            5, //Reccons
			5, //Tupcons
            5, //Typeid
            5, //Classid
            5, //Newfunc
            5, //Newmod
			5, //Pushext
            5, //Skiptag
			5, //Newvar
			5, //Newrec
			5, //Newtup
			5, //Failwith
			5, //Start
			5, //Pushstr
			5, //PushCh
			5, //PushI4
			5, //PushR4
            5, //Pushloc
			5, //Pushvar
			5, //Pushfld
            5, //Poploc
			5, //Popvar
			5, //Runmod
			5, //Br
			5, //Brtrue
			5, //Brfalse
			5, //Newfun			
		};
    }
}