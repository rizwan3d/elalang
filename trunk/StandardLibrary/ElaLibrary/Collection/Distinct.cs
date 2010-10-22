using System;
using System.Linq;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
using System.Collections;
using System.Collections.Generic;

namespace Ela.StandardLibrary.Collection
{
	public sealed class Distinct : ElaFunction<IEnumerable<RuntimeValue>,ElaSequence>
	{
		protected override ElaSequence Call(IEnumerable<RuntimeValue> seq)
		{
			var newSeq = seq.Distinct();
			return new ElaSequence(newSeq);
		}
	}
}