using System;

namespace Ela.Compilation
{
	internal sealed class LabelMap
	{
		#region Construction
		internal LabelMap(Label exit)
		{
			Exit = exit;
			GotoParam = -1;
		}


		internal LabelMap(LabelMap old)
		{
			Exit = old.Exit;
			FunStart = old.FunStart;
			GotoParam = old.GotoParam;
			FunctionName = old.FunctionName;
		}
		#endregion


		#region Properties
		internal Label Exit { get; private set; }

		internal Label BlockStart { get; set; }

		internal Label BlockEnd { get; set; }

		internal Label FunStart { get; set; }

		internal string FunctionName { get; set; }

		internal int GotoParam { get; set; }
		#endregion
	}
}