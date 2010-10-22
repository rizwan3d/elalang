using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Debug
{
	public sealed class GetList : ElaFunction<Int32,ElaList>
	{
		protected override ElaList Call(int size)
		{
			var list = ElaList.Nil;

			for (var i = size; i > -1; --i)
				list = new ElaList(list, new RuntimeValue(i));

			return list;
		}
	}
}
