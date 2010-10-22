using System;
using System.Linq;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Collection
{
	public sealed class SortList : ElaFunction<ElaList,ElaFunction,ElaList>
	{
		protected override ElaList Call(ElaList list, ElaFunction func)
		{
			var newList = ElaList.Nil;

			foreach (var i in list.OrderBy(k => func.Call(k)).Reverse())
				newList = new ElaList(newList, i);

			return newList;
		}
	}
}