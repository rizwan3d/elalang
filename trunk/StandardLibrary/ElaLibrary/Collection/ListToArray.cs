using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Collection
{
	public sealed class ListToArray : ElaFunction<ElaList,ElaArray>
	{
		protected override ElaArray Call(ElaList list)
		{
			var len = list.GetLength();
			var array = new ElaArray(len);
			var c = 0;

			foreach (var e in list)
				array.SetValue(c++, e);
			
			return array;
		}
	}
}