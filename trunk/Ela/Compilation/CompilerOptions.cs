﻿using System;
using System.Text;

namespace Ela.Compilation
{
	public sealed class CompilerOptions
	{
		#region Construction
		public CompilerOptions()
		{

		}
		#endregion


		#region Methods
        public static CompilerOptions Default()
        {
            return new CompilerOptions
            {
                WarningsAsErrors = false,
                ShowHints = true,
                Optimize = true
            };
        }

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

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var pi in typeof(CompilerOptions).GetProperties())
                sb.AppendFormat("{0}={1};", pi.Name, pi.GetValue(this, null));

            return sb.ToString();
        }
		#endregion


		#region Properties
		public bool WarningsAsErrors { get; set; }

		public bool NoWarnings { get; set; }
		
		public bool ShowHints { get; set; }

		public bool GenerateDebugInfo { get; set; }

		public bool Optimize { get; set; }

		public string Prelude { get; set; }

        public bool IgnoreUndefined { get; set; }
		#endregion
	}
}