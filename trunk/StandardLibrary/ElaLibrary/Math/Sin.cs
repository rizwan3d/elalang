using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Math
{
	public sealed class Sin : ElaFunction<Double,Double>
	{
		protected override Double Call(Double val)
		{
			return System.Math.Sin(val);
		}
	}
}
