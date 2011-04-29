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


		#region Methods
		internal CompilerOptions Clone()
		{
			return new CompilerOptions
			{
				WarningsAsErrors = this.WarningsAsErrors,
				NoWarnings = this.NoWarnings,
				ShowHints = this.ShowHints,
				GenerateDebugInfo = this.GenerateDebugInfo,
				Optimize = this.Optimize,
				Prelude = this.Prelude
			};
		}
		#endregion


		#region Properties
		public bool WarningsAsErrors { get; set; }

		public bool NoWarnings { get; set; }
		
		public bool ShowHints { get; set; }

		public bool GenerateDebugInfo { get; set; }

		public bool Optimize { get; set; }

		public string Prelude { get; set; }
		#endregion
	}
}