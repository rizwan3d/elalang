using System;
using System.IO;

namespace Ela.Compilation
{
	public sealed class ModuleReference
	{
		#region Construction
		private const string FORMAT = "{0}[{1}]";

		internal ModuleReference(string moduleName) : this(moduleName, null, null, 0, 0)
		{

		}


		internal ModuleReference(string moduleName, string dllName, string folder, int line, int column)
		{
			ModuleName = moduleName;
			DllName = dllName;
			Folder = folder;
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
			return String.IsNullOrEmpty(Folder) ? name :
				String.Concat(Folder, Path.DirectorySeparatorChar, name);
		}
		#endregion


		#region Fields
		public readonly string ModuleName;

		public readonly string DllName;

		public readonly string Folder;

		public readonly int Line;

		public readonly int Column;
		#endregion


		#region Properties
		internal bool IsStandardLibrary { get; set; }
		#endregion
	}
}
