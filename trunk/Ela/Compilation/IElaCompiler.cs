using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	public interface IElaCompiler
	{
		CompilerResult Compile(ElaExpression expr, CompilerOptions options, BuiltinVars builtins);

		CompilerResult Compile(ElaExpression expr, CompilerOptions options, BuiltinVars builtins, CodeFrame frame, Scope globalScope);

		event EventHandler<ModuleEventArgs> ModuleInclude;
	}
}
