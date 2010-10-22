using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Debug
{
	public sealed class GetArray : ElaFunction<Int32,ElaArray>
	{
		protected override ElaArray Call(int size)
		{
			var arr = new ElaArray(size);

			for (var i = 0; i < size; i++)
				arr.SetValue(i, new RuntimeValue(i));

			return arr;
		}
	}
}
