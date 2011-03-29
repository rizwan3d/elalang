using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	internal sealed class FunData
	{
		internal int Start;
		internal int Finish;
		internal int Module;
		internal int Pars;
		internal ElaFunctionLiteral Literal;
	}
}
