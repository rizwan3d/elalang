using System;
using System.Linq;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Collection
{
	public sealed class ListItem : ElaFunction<ElaList,Int32,RuntimeValue>
	{
		protected override RuntimeValue Call(ElaList list, int index)
		{
			return list.ElementAt(index);
		}
	}
}
