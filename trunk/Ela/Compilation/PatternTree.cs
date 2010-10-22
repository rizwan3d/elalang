using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
	internal sealed class PatternTree
	{
		#region Construction
		internal PatternTree()
		{

		}
		#endregion


		#region Properties
		internal ElaPatternAffinity Affinity { get; set; }

		internal PatternTree Next { get; set; }

		internal PatternTree Child { get; set; }
		#endregion
	}
}
