using System;
using System.Collections.Generic;
using System.IO;

namespace Ela.Linking
{
	public sealed class LinkerOptions
	{
		#region Construction
		public LinkerOptions()
		{
			CodeBase = new CodeBase();
		}
		#endregion


		#region Properties
		public CodeBase CodeBase { get; private set; }

		public string StandardLibrary { get; set; }

		public bool ForceRecompile { get; set; }

		public bool SkipTimeStampCheck { get; set; }

		public bool NoWarnings { get; set; }

		public bool WarningsAsErrors { get; set; }

		public bool Sandbox { get; set; }
		#endregion
	}
}
