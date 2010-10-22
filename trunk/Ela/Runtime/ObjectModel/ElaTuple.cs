using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public class ElaTuple : ElaIndexedObject, IEnumerable<RuntimeValue>
	{
		#region Construction
		public ElaTuple(params object[] args) : base(ObjectType.Tuple)
		{
			Values = args.Select(o => RuntimeValue.FromObject(o)).ToArray();
		}


		public ElaTuple(params RuntimeValue[] args) : base(ObjectType.Tuple)
		{
			Values = args;
		}


		internal ElaTuple(int size) : this(size, ObjectType.Tuple)
		{

		}

		
		internal ElaTuple(int size, ObjectType type) : base(type)
		{
			Values = new RuntimeValue[size];
		}
		#endregion


		#region Methods
		public override ElaTypeInfo GetTypeInfo()
		{
			return new ElaTupleInfo(Tag, (ObjectType)TypeId);
		}


		public IEnumerator<RuntimeValue> GetEnumerator()
		{
			return Values.Take(Length).GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		protected internal override RuntimeValue GetValue(RuntimeValue key)
		{
			return key.Type == ElaMachine.INT && key.I4 < Length && key.I4 > -1 ?
				Values[key.I4] : new RuntimeValue(Invalid);
		}


		internal void InternalSetValue(int index, RuntimeValue value)
		{
			Values[index] = value;
		}
		#endregion


		#region Properties
		public RuntimeValue this[int index]
		{
			get { return GetValue(new RuntimeValue(index)); }
		}

		public override int Length { get { return Values.Length; } }
		
		internal string Tag { get; set; }
				
		protected RuntimeValue[] Values { get; private set; }
		#endregion
	}
}
