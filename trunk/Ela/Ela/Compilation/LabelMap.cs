using System;
using System.Collections.Generic;

namespace Ela.Compilation
{
	internal sealed class LabelMap
	{
		#region Construction
		internal LabelMap()
		{
			
		}


		internal LabelMap(LabelMap old)
		{
			FunStart = old.FunStart;
			FunctionName = old.FunctionName;
			FunctionParameters = old.FunctionParameters;
		}
		#endregion


		#region Properties
		internal Label FunStart { get; set; }

		internal bool InlineFunction { get; set; }

		internal string FunctionName { get; set; }

        internal string BuiltinName { get; set; }

		internal int FunctionParameters { get; set; }

		internal Scope FunctionScope { get; set; }
		#endregion
	}
}