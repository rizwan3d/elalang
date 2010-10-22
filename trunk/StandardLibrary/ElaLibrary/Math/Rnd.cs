using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Math
{
	public sealed class Rnd : ElaFunction<Int32,Int32>
	{
		protected override int Call(int val)
		{
			var rnd = new Random();
			return rnd.Next(val);
		}
	}
}
