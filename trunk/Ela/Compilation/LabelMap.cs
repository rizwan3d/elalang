using System;
using System.Collections.Generic;

namespace Ela.Compilation
{
	internal sealed class LabelMap
	{
		#region Construction
		internal LabelMap(Label exit)
		{
			Exit = exit;
		}


		internal LabelMap(LabelMap old)
		{
			Exit = old.Exit;
			FunStart = old.FunStart;
			FunctionName = old.FunctionName;
			FunctionParameters = old.FunctionParameters;
		}
		#endregion


		#region Properties
		internal Label Exit { get; private set; }

		internal Label BlockStart { get; set; }

		internal Label BlockEnd { get; set; }

		internal Label FunStart { get; set; }

		internal string FunctionName { get; set; }

		internal int FunctionParameters { get; set; }

		internal Scope FunctionScope { get; set; }
		#endregion
	}
}