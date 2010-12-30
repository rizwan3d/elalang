using System;
using Ela.CodeModel;
using Ela.Linking;

namespace Ela.Compilation
{
	public sealed class ElaCompiler : IElaCompiler
	{
		#region Construction
		public ElaCompiler()
		{
			
		}
		#endregion


		#region Methods
		public CompilerResult Compile(ElaExpression expr, CompilerOptions options)
		{
			var frame = new CodeFrame();
			return Compile(expr, options, frame, new Scope(false, null));
		}


		public CompilerResult Compile(ElaExpression expr, CompilerOptions options, CodeFrame frame, Scope globalScope)
		{
			try
			{
				Options = options;
				var helper = new Builder(frame, Options, globalScope);
				helper.CompileUnit(expr);
				frame.Symbols = frame.Symbols == null ? helper.Symbols :
					helper.Symbols != null ? frame.Symbols.Merge(helper.Symbols) : frame.Symbols;
				frame.GlobalScope = globalScope;
				return new CompilerResult(helper.Success ? frame : null, 
					helper.Success, helper.Errors.ToArray());
			}
			catch (Exception ex)
			{
				throw new ElaCompilerException(Strings.GetMessage("Ice", ex.Message), ex);
			}
		}
		#endregion


		#region Properties
		internal CompilerOptions Options { get; private set; }
		#endregion
	}
}