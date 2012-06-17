using System;
using System.Collections;
using System.Collections.Generic;

namespace Ela.Runtime
{
	internal class EvalStack : IEnumerable<ElaValue>
	{
		#region Construction
		private const int DEFAULT_SIZE = 20;
		private ElaValue[] array;
		private int size;
		private int initialSize;

		internal EvalStack()
			: this(DEFAULT_SIZE)
		{

		}


		internal EvalStack(int size)
		{
			this.initialSize = size;
			array = new ElaValue[size];
		}
		#endregion


		#region Methods
		public IEnumerator<ElaValue> GetEnumerator()
		{
			for (var i = 0; i < size; i++)
				yield return array[i];
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		internal void Clear(int offset)
		{
			if (size > 0)
			{
				var newArr = new ElaValue[initialSize];
				Array.Copy(array, 0, newArr, 0, offset);
				array = newArr;
				size = offset;
			}
			else
			{
				array = new ElaValue[initialSize];
				size = 0;
			}
		}


		internal void PopVoid()
		{
			--size;

			if (array[size].Ref.TypeId > 5)
				array[size].Ref = null;
		}


		internal ElaValue PopFast()
		{
			return array[--size];
		}


		internal ElaValue Pop()
		{
            --size;

            if (array[size].Ref.TypeId > 5)
            {
                var ret = array[size];
                array[size].Ref = null;
                return ret;
            }

            return array[size];
		}


		internal ElaValue Peek()
		{
			return array[size - 1];
		}


		internal void Push(ElaValue val)
		{
			array[size++] = val;
		}


		internal void Push(int val)
		{
			array[size++] = new ElaValue(val);
		}


		internal void Push(bool val)
		{
			array[size++] = new ElaValue(val);
		}


		internal void Replace(ElaValue val)
		{
			array[size - 1] = val;
		}


		internal void Replace(int val)
		{
			array[size - 1] = new ElaValue(val);
		}

        internal void Replace(bool val)
		{
            array[size - 1] = new ElaValue(val);
		}
		#endregion


		#region Properties
		internal int Count
		{
			get { return size; }
		}


		internal ElaValue this[int index]
		{
			get { return array[index]; }
		}
		#endregion
	}
}