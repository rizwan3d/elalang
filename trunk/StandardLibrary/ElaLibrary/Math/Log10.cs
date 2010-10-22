using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Math
{
	public sealed class Log10 : ElaFunction<Double,Double>
	{
		protected override Double Call(Double val)
		{
			return System.Math.Log10(val);
		}
	}
}
