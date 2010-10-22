using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Math
{
	public sealed class Sinh : ElaFunction<Double,Double>
	{
		protected override Double Call(Double val)
		{
			return System.Math.Sinh(val);
		}
	}
}
