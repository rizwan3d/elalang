using System;
using System.Collections;
using System.Collections.Generic;

namespace Ela
{
	internal sealed class Lookup<T> : IEnumerable<T>
	{
		#region Construction
		private static readonly object dummy = new Object();
		private Dictionary<T,Object> map;

		internal Lookup(Lookup<T> copy)
		{
			map = new Dictionary<T,Object>(copy.map);
		}


		internal Lookup()
		{
			map = new Dictionary<T,Object>();
		}
		#endregion


		#region Methods
		public IEnumerator<T> GetEnumerator()
		{
			foreach (var v in map.Keys)
				yield return v;
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		internal void Add(T value)
		{
			map.Add(value, dummy);
		}


		internal bool Contains(T value)
		{
			return map.ContainsKey(value);
		}
		#endregion
	}
}
