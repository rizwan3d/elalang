using System;

namespace Ela.Debug
{
	public sealed class FunSym
	{
		#region Construction
		internal FunSym(string name, int handle, int offset, int pars)
		{
			Name = name;
			Handle = handle;
			StartOffset = offset;
			Parameters = pars;
		}
		#endregion


		#region Properties
		public string Name { get; private set; }

		public int Parameters { get; private set; }
		
		public int Handle { get; private set; }

		public int StartOffset { get; private set; }

		public int EndOffset { get; internal set; }
		#endregion
	}
}
