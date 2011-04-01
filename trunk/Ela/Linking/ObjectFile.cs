using System;
using System.IO;

namespace Ela.Linking
{
	public abstract class ObjectFile
	{
		#region Construction
		private const int VERSION = 11;

		protected ObjectFile(FileInfo file)
		{
			File = file;
		}
		#endregion


		#region Properties
		public FileInfo File { get; private set; }

		public int Version { get { return VERSION; } }
		#endregion
	}
}
