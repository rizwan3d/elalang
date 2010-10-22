using System;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaEnumerator : ElaObject
	{
		#region Construction
		private IEnumerator<RuntimeValue> enumerator;
		private ElaFunction func;

		internal ElaEnumerator(ElaObject obj) : base(ObjectType.Enumerator)
		{
			if (obj.DataType == ObjectType.Coroutine)
				this.func = (ElaFunction)obj;
			else
				this.enumerator = ((IEnumerable<RuntimeValue>)obj).GetEnumerator();
		}
		#endregion


		#region Methods
		public RuntimeValue GetNext()
		{
			if (enumerator != null)
			{
				var res = enumerator.MoveNext();
				return res ? enumerator.Current : new RuntimeValue(ElaObject.Void);
			}
			else
			{
				var ret = func.Call();
				return ret;
			}
		}
		#endregion
	}
}