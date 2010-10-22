using System;
using System.Linq;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
using System.Collections.Generic;

namespace Ela.StandardLibrary.Collection
{
	public sealed class Length : ElaFunction<IEnumerable<RuntimeValue>,Int32>
	{
		protected override int Call(IEnumerable<RuntimeValue> seq)
		{
			return seq.Count();
		}
	}
}