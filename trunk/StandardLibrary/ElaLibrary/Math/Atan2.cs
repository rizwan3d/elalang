using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Math
{
	public sealed class Atan2 : ElaFunction<Double,Double,Double>
	{
		protected override Double Call(Double val, Double val2)
		{
			return System.Math.Atan2(val, val2);
		}
	}
}
