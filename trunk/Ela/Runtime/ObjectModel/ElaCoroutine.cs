using System;
using System.Collections;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaCoroutine : ElaFunction
	{
		#region Construction
		private const int VOID = 2;

		internal ElaCoroutine(int handle, int module, FastList<RuntimeValue[]> captures, ElaMachine vm,
			RuntimeValue[] mem) : base(handle, module, 0, captures, vm, ObjectType.Coroutine)
		{
			Memory = mem;
		}
		#endregion


		#region Methods
		//public IEnumerator<RuntimeValue> GetEnumerator()
		//{
		//    var ret = default(RuntimeValue);

		//    do
		//    {
		//        ret = Call();
		//        yield return ret;
		//    }
		//    while (ret.Type != VOID);
		//}

		
		//IEnumerator IEnumerable.GetEnumerator()
		//{
		//    return GetEnumerator();
		//}
		#endregion


		#region Properties
		internal RuntimeValue[] Memory { get; private set; }
		#endregion
	}
}