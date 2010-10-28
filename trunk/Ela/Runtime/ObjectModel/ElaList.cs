using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaList : ElaIndexedObject, IEnumerable<RuntimeValue>
	{
		#region Construction
		private const string HEAD = "head";
		private const string TAIL = "tail";
		
		public static readonly ElaList Nil = new ElaList(null, new RuntimeValue(ElaObject.Unit));


		public ElaList(object value) : this(Nil, RuntimeValue.FromObject(value))
		{
			
		}


		public ElaList(ElaList next, object value) : this(next, RuntimeValue.FromObject(value))
		{
			
		}

		
		public ElaList(ElaList next, RuntimeValue value) : base(ObjectType.List)
		{
			Next = next;
			Value = value;
		}
		#endregion


		#region Methods
		public static ElaList FromEnumerable(IEnumerable<Object> seq)
		{
			return FromEnumerable(seq.Select(e => RuntimeValue.FromObject(e)));
		}


		public static ElaList FromEnumerable(IEnumerable<RuntimeValue> seq)
		{
			var list = ElaList.Nil;

			foreach (var e in seq.Reverse())
				list = new ElaList(list, e);

			return list;
		}


		protected internal override RuntimeValue GetValue(RuntimeValue index)
		{
			if (index.Type == ElaMachine.INT && index.I4 > -1 && index.I4 < GetLength())
			{
				var idx = index.I4;
				var l = this;

				while (idx --> 0)
					l = l.Next;

				return l.Value;
			}
			else
				return new RuntimeValue(ElaObject.Invalid);
		}
		

		public IEnumerator<RuntimeValue> GetEnumerator()
		{
			if (this != Nil)
			{
				var xs = this;

				do
				{
					yield return xs.Value;
					xs = xs.Next;
				}
				while (xs != Nil);
			}
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		protected internal override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case HEAD: return Value;
				case TAIL: return new RuntimeValue(Next);
				default: return base.GetAttribute(name);
			}
		}


		private int GetLength()
		{
			if (this == Nil)
				return 0;

			var count = 0;
			var xs = this;

			do
			{
				count++;
				xs = xs.Next;
			}
			while (xs != Nil);

			return count;
		}
		#endregion


		#region Properties
		public ElaList Next { get; private set; }

		public RuntimeValue Value { get; private set; }

		public override int Length { get { return GetLength(); } }
		#endregion
	}
}
