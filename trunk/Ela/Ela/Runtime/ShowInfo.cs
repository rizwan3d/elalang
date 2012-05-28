using System;
using System.Collections.Generic;
using Ela.Compilation;

namespace Ela.Runtime
{
	public sealed class ShowInfo
	{
		#region Construction
		public static readonly ShowInfo Default = new ShowInfo(0, 0);
		public static readonly ShowInfo Debug = new ShowInfo(0, 40);

		public ShowInfo(int stringLength, int seqLength) : this(stringLength, seqLength, null)
		{

		}
				

		public ShowInfo(int stringLength, int seqLength, string format)
		{
			StringLength = stringLength;
			SequenceLength = seqLength;
			Format = format;
		}
		#endregion


		#region Properties
		public int StringLength { get; private set; }

		public int SequenceLength { get; private set; }

		public string Format { get; private set; }

		public int Flags { get; set; }
		#endregion
	}
}
