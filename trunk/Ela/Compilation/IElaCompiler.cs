using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	public interface IElaCompiler
	{
		CompilerResult Compile(ElaCodeUnit unit, CompilerOptions options);

		CompilerResult Compile(ElaCodeUnit unit, CompilerOptions options, CodeFrame frame, Scope globalScope);
	}
}
