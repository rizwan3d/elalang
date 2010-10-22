using System;
using System.Collections.Generic;

namespace Ela
{
	public interface IReadOnlyMap<K,V> : IEnumerable<KeyValuePair<K,V>>
	{
		V this[K key] { get; }

		int Count { get; }
	}
}
