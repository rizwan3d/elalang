using System;
using System.IO;
using System.Collections.Generic;

namespace Ela.Compilation
{
	public sealed class ModuleReference
	{
		#region Construction
		private const string FORMAT = "{0}[{1}]";

		internal ModuleReference(string moduleName) : this(moduleName, null, null, 0, 0)
		{

		}


		internal ModuleReference(string moduleName, string dllName, string[] path, int line, int column)
		{
			ModuleName = moduleName;
			DllName = dllName;
			Path = path ?? new string[0];
			Line = line;
			Column = column;
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return DllName == null ? BuildFullName(ModuleName) : 
				BuildFullName(String.Format(FORMAT, ModuleName, DllName));
		}


		private string BuildFullName(string name)
		{
			return Path.Length == 0 ? name :
				String.Concat(String.Join(System.IO.Path.DirectorySeparatorChar.ToString(), Path), 
					System.IO.Path.DirectorySeparatorChar, name);
		}
		#endregion


		#region Fields
		public readonly string ModuleName;

		public readonly string DllName;

		public readonly string[] Path;

		public readonly int Line;

		public readonly int Column;
		#endregion


		#region Properties
		internal bool IsStandardLibrary { get; set; }

		internal bool NoPrelude { get; set; }
		#endregion
	}
}
