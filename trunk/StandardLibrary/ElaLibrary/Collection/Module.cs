using System;
using Ela.Linking;

namespace Ela.StandardLibrary.Collection
{
	public sealed class Module : ForeignModule
	{
		public override void Initialize()
		{
			Add("reverse", new Reverse());
			Add("reverseList", new ReverseList());
			Add("reverseArray", new ReverseList());
			Add("listItem", new ListItem());
			Add("listLength", new ListLength());
			Add("arrayToList", new ArrayToList());
			Add("listToArray", new ListToArray());
			Add("sortList", new SortList());
			Add("sortArray", new SortArray());
			Add("length", new Length());
			Add("distinct", new Distinct());
		}
	}
}
