using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Collection
{
	public sealed class ReverseList : ElaFunction<ElaList,ElaList>
	{
		internal static ElaList Reverse(ElaList list)
		{
			var newList = ElaList.Nil;

			foreach (var e in list)
				newList = new ElaList(newList, e);

			return newList;
		}


		protected override ElaList Call(ElaList list)
		{
			return Reverse(list);
		}
	}
}