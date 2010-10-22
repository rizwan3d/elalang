using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaSequence : ElaObject, IEnumerable<RuntimeValue>
	{
		#region Construction
		private IEnumerator<RuntimeValue> enumerator;
		private IEnumerable<RuntimeValue> enumerable;


		public ElaSequence(IEnumerable<Object> seq) : base(ObjectType.Sequence)
		{
			if (seq == null)
				throw new ArgumentNullException("seq");

			var rvSeq = seq.Select(e => RuntimeValue.FromObject(e));
			this.enumerable = rvSeq;
			this.enumerator = rvSeq.GetEnumerator();
		}

				
		public ElaSequence(IEnumerable<RuntimeValue> seq) : base(ObjectType.Sequence)
		{
			if (seq == null)
				throw new ArgumentNullException("seq");

			this.enumerable = seq;
			this.enumerator = seq.GetEnumerator();
		}


		internal ElaSequence(ElaObject obj) : base(ObjectType.Sequence)
		{
			if ((ObjectType)obj.TypeId == ObjectType.Function)
				Function = (ElaNativeFunction)obj;
			else
			{
				this.enumerable = (IEnumerable<RuntimeValue>)obj;
				this.enumerator = enumerable.GetEnumerator();
			}
		}
		#endregion


		#region Methods
		public override ElaTypeInfo GetTypeInfo()
		{
			return new ElaSequenceInfo(Lazy);
		}


		public IEnumerator<RuntimeValue> GetEnumerator()
		{
			var val = default(RuntimeValue);

			while (GetValue(out val))
				yield return val;
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		internal bool GetValue(out RuntimeValue value)
		{
			value = GetNext();
			return value.Type != ElaMachine.PTR;				
		}
		

		internal RuntimeValue GetNext()
		{
			if (enumerator != null)
			{
				var res = enumerator.MoveNext();

				if (res)
				{
					return enumerator.Current;
				}
				else
				{
					enumerator = enumerable.GetEnumerator();
					return new RuntimeValue(ElaObject.Pointer);
				}
			}
			else
				return Function.Call();
		}
		#endregion


		#region Properties
		public bool Lazy
		{
			get { return Function != null; }
		}

		internal ElaNativeFunction Function { get; private set; }
		#endregion
	}
}