using System;
using System.Collections.Generic;
using System.Collections;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ChunkEnumerable : IEnumerable<ElaValue>
	{
		private IEnumerable<ElaValue> seq;

		public ChunkEnumerable(IEnumerable<ElaValue> seq)
		{
			this.seq = seq;
		}

		public IEnumerator<ElaValue> GetEnumerator()
		{
			return seq.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return seq.GetEnumerator();
		}
	}
}
