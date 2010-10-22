using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Math
{
	public sealed class Acos : ElaFunction<Double,Double>
	{
		protected override Double Call(Double val)
		{
			return System.Math.Acos(val);
		}
	}
}
