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
		public CompilerResult Compile(ElaCodeUnit unit, CompilerOptions options)
		{
			var frame = new CodeFrame();
			frame.Layouts.Add(new MemoryLayout(0, 1));
			return Compile(unit, options, frame, new Scope(false, null));
		}


		public CompilerResult Compile(ElaCodeUnit unit, CompilerOptions options, CodeFrame frame, Scope globalScope)
		{
			try
			{
				Options = options;
				var helper = new CompilerHelper(frame, Options, globalScope);
				frame.Layouts[0].Size = helper.CompileUnit(unit);
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