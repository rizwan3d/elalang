using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Math
{
	public sealed class Log2 : ElaFunction<Double,Double,Double>
	{
		protected override Double Call(Double val1, Double val2)
		{
			return System.Math.Log(val1, val2);
		}
	}
}
