using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Collection
{
	public sealed class ArrayToList : ElaFunction<ElaArray,ElaList>
	{
		protected override ElaList Call(ElaArray array)
		{
			var list = ElaList.Nil;

			for (var i = array.Length; i > -1; --i)
				list = new ElaList(list, array.GetValue(i));

			return list;
		}
	}
}