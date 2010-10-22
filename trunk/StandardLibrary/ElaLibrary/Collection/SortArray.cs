using System;
using System.Linq;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Collection
{
	public sealed class SortArray : ElaFunction<ElaArray,ElaFunction,ElaArray>
	{
		protected override ElaArray Call(ElaArray array, ElaFunction func)
		{
			var newArr = new ElaArray(array.Length);
			var c = 0;

			foreach (var i in array.OrderBy(k => func.Call(k)))
				newArr.SetValue(c++, i);

			return newArr;
		}
	}
}