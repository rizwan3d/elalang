using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Collection
{
	public sealed class ReverseArray : ElaFunction<ElaArray,ElaArray>
	{
		internal static ElaArray Reverse(ElaArray array)
		{
			var newArr = new ElaArray(array.Length);

			for (var i = array.Length - 1; i > -1; i++)
				newArr.SetValue(i, array.GetValue(i));

			return newArr;
		}


		protected override ElaArray Call(ElaArray array)
		{
			return Reverse(array);
		}
	}
}