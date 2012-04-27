using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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


        #region Methods
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var pi in typeof(LinkerOptions).GetProperties())
                if (pi.Name != "CodeBase")
                    sb.AppendFormat("{0}={1};", pi.Name, pi.GetValue(this, null));

            sb.AppendFormat("LookupStartupDirectory={0}", CodeBase.LookupStartupDirectory);
            return sb.ToString();
        }
        #endregion


        #region Properties
        public CodeBase CodeBase { get; private set; }

		public string StandardLibrary { get; set; }

		public bool ForceRecompile { get; set; }

		public bool SkipTimeStampCheck { get; set; }

		public bool NoWarnings { get; set; }

		public bool WarningsAsErrors { get; set; }
		#endregion
	}
}
