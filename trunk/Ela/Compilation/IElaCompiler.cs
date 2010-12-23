using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	public interface IElaCompiler
	{
		CompilerResult Compile(ElaExpression expr, CompilerOptions options);

		CompilerResult Compile(ElaExpression expr, CompilerOptions options, CodeFrame frame, Scope globalScope);
	}
}
