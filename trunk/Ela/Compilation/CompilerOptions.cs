using System;

namespace Ela.Compilation
{
	public sealed class CompilerOptions
	{
		#region Construction
		public static readonly CompilerOptions Default = new CompilerOptions
		{
			WarningsAsErrors = false,
			ShowHints = true,
			Optimize = true
		};

		public CompilerOptions()
		{

		}
		#endregion


		#region Properties
		public bool WarningsAsErrors { get; set; }

		public bool NoWarnings { get; set; }
		
		public bool ShowHints { get; set; }

		public bool GenerateDebugInfo { get; set; }

		public bool Optimize { get; set; }

		public bool StrictMode { get; set; }
		#endregion
	}
}