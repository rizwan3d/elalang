using System;
using Ela.CodeModel;
using Ela.Compilation;

namespace Ela
{
	internal static class Builtins
	{
		internal static int Params(ElaBuiltinKind kind)
		{
			return kind == ElaBuiltinKind.Apply ? 1 : 2;
		}


		internal static ElaBuiltinKind Kind(string func)
		{
			switch (func)
			{
				case "apply": return ElaBuiltinKind.Apply;
				case "compforward": return ElaBuiltinKind.CompForward;
				case "compbackward": return ElaBuiltinKind.CompBackward;
				default: 
                    return ElaBuiltinKind.None;
			}
		}
	}
}
