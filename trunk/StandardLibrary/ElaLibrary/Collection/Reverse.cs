using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Collection
{
	public sealed class Reverse : ElaFunction<ElaObject,ElaObject>
	{
		protected override ElaObject Call(ElaObject seq)
		{
			if (seq is ElaArray)
				return ReverseArray.Reverse((ElaArray)seq);
			else if (seq is ElaList)
				return ReverseList.Reverse((ElaList)seq);
			else
				return seq;
		}
	}
}