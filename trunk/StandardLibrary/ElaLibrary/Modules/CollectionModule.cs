using System;
using System.Linq;
using Ela.ModuleGenerator;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using System.Collections.Generic;

namespace Ela.StandardLibrary.Modules
{
	public partial class CollectionModule
	{
		#region Construction
		public CollectionModule()
		{

		}
		#endregion


		#region Methods
		[Function("where")]
		internal ElaSequence Where(ElaSequence seq, ElaFunction selector)
		{
			return new ElaSequence(seq.Where(e => selector.Call(e).ToBoolean(null)));
		}


		[Function("arrayToList")]
		internal ElaList ArrayToList(ElaArray array)
		{
			var list = ElaList.Nil;

			for (var i = array.Length; i > -1; --i)
				list = new ElaList(list, array[i]);

			return list;
		}


		[Function("distinct")]
		internal ElaSequence Distinct(ElaSequence seq)
		{
			var newSeq = seq.Distinct();
			return new ElaSequence(newSeq);
		}


		[Function("length")]
		internal int Length(ElaSequence seq)
		{
			return seq.Count();
		}


		[Function("listItem")]
		internal RuntimeValue ListItem(ElaList list, int index)
		{
			return list.ElementAt(index);
		}


		[Function("listLength")]
		internal int ListLength(ElaList list)
		{
			return list.Length;
		}


		[Function("listToArray")]
		internal ElaArray ListToArray(ElaList list)
		{
			var len = list.Length;
			var array = new ElaArray(len);
			var c = 0;

			foreach (var e in list)
				array[c++] = e;

			return array;
		}


		[Function("reverse")]
		internal ElaObject Reverse(ElaObject seq)
		{
			if (seq is ElaArray)
				return ReverseArray((ElaArray)seq);
			else if (seq is ElaList)
				return ReverseList((ElaList)seq);
			else
				return seq;
		}


		[Function("reverseList")]
		internal ElaList ReverseList(ElaList list)
		{
			var newList = ElaList.Nil;

			foreach (var e in list)
				newList = new ElaList(newList, e);

			return newList;
		}


		[Function("reverseArray")]
		internal ElaArray ReverseArray(ElaArray array)
		{
			var newArr = new ElaArray(array.Length);

			for (var i = array.Length - 1; i > -1; i++)
				newArr[i] = array[i];

			return newArr;
		}


		[Function("sortArray")]
		internal ElaArray SortArray(ElaArray array, ElaFunction func)
		{
			var newArr = new ElaArray(array.Length);
			var c = 0;

			foreach (var i in array.OrderBy(k => func.Call(k)))
				newArr[c++] = i;

			return newArr;
		}


		[Function("sortList")]
		internal ElaList SortList(ElaList list, ElaFunction func)
		{
			var newList = ElaList.Nil;

			foreach (var i in list.OrderBy(k => func.Call(k)).Reverse())
				newList = new ElaList(newList, i);

			return newList;
		}
		#endregion
	}
}
