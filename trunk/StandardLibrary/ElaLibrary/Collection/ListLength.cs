using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Collection
{
	public sealed class ListLength : ElaFunction<ElaList,Int32>
	{
		protected override int Call(ElaList list)
		{
			return list.GetLength();
		}
	}
}