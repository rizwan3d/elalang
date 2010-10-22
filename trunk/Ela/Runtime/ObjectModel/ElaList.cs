using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaList : ElaObject, IEnumerable<RuntimeValue>
	{
		#region Construction
		private const string LENGTH = "length";
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
				case LENGTH: return new RuntimeValue(GetLength());
				case HEAD: return Value;
				case TAIL: return new RuntimeValue(Next);
				default: return base.GetAttribute(name);
			}
		}


		public int GetLength()
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
		#endregion
	}
}
