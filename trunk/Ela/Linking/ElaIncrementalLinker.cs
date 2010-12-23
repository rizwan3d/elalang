using System;
using System.IO;
using Ela.Compilation;
using Ela.Parsing;

namespace Ela.Linking
{
	public sealed class ElaIncrementalLinker : ElaIncrementalLinker<ElaParser,ElaCompiler>
	{
		#region Construction
		public ElaIncrementalLinker(LinkerOptions linkerOptions, CompilerOptions compOptions, FileInfo file) :
			base(linkerOptions, compOptions, file)
		{

		}

		public ElaIncrementalLinker(LinkerOptions linkerOptions, CompilerOptions compOptions) :
			base(linkerOptions, compOptions, null)
		{
			
		}
		#endregion
	}


	public class ElaIncrementalLinker<P,C> : ElaLinker<P,C>
		where P : IElaParser, new() where C : IElaCompiler, new()
	{
		#region Construction
		private string source;
		
		public ElaIncrementalLinker(LinkerOptions linkerOptions, CompilerOptions compOptions, FileInfo file) :
			base(linkerOptions, compOptions, file)
		{

		}

		public ElaIncrementalLinker(LinkerOptions linkerOptions, CompilerOptions compOptions) :
			base(linkerOptions, compOptions, null)
		{
			
		}
		#endregion


		#region Methods
		public override LinkerResult Build()
		{
			Messages.Clear();
			Success = true;
			var mod = new ModuleReference(
				RootFile == null ? FILE : Path.GetFileNameWithoutExtension(RootFile.Name));
			var frame = default(CodeFrame);
			var scope = default(Scope);
			var scratch = true;
						
			if (Assembly.ModuleCount != 0)
			{
				var root = Assembly.GetRootModule();
				frame = root.Clone();
				scope = root.GlobalScope.Clone();
				scratch = false;
			}

			frame = Build(mod, RootFile, source, frame, scope);			
			RegisterFrame(mod, frame, RootFile);

			if (Success)
				Assembly.RefreshRootModule(frame);
			else if (scratch)
				Assembly = new CodeAssembly();
			
			return new LinkerResult(Assembly, Success, Messages);
		}


		public void SetSource(string source)
		{
			this.source = source;
		}
		#endregion
	}
}
