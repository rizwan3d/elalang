using System;
using System.Collections;
using System.Collections.Generic;

namespace Ela.Runtime
{
	internal class EvalStack : IEnumerable<RuntimeValue>
	{
		#region Construction
		private const int DEFAULT_SIZE = 20;
		private RuntimeValue[] array;
		private int size;
		private int initialSize;

		internal EvalStack() : this(DEFAULT_SIZE)
		{

		}


		internal EvalStack(int size)
		{
			this.initialSize = size;
			array = new RuntimeValue[size];
		}
		#endregion


		#region Methods
		public IEnumerator<RuntimeValue> GetEnumerator()
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
			var newArr = new RuntimeValue[array.Length];
			Array.Copy(array, 0, newArr, 0, offset);
			array = newArr;
			size = offset;
		}


		internal EvalStack Clone()
		{
			var ret = (EvalStack)MemberwiseClone();
			ret.array = (RuntimeValue[])array.Clone();
			return ret;
		}


		internal RuntimeValue Pop()
		{
            --size;

            if (array[size].Type > 6)
            {
                var ret = array[size];
                array[size].Ref = null;
                return ret;
            }
            else
                return array[size]; 
		}


		internal RuntimeValue Peek()
		{
			return array[size - 1];
		}


		internal void Push(RuntimeValue val)
		{
			if (size == array.Length)
			{
				var dest = new RuntimeValue[array.Length * 2];
				Array.Copy(array, 0, dest, 0, size);
				array = dest;
			}

			array[size++] = val;
		}


		internal void Push(int val)
		{
			if (size == array.Length)
			{
				var dest = new RuntimeValue[array.Length * 2];
				Array.Copy(array, 0, dest, 0, size);
				array = dest;
			}

			array[size++] = new RuntimeValue(val);
		}


		internal void Push(bool val)
		{
			if (size == array.Length)
			{
				var dest = new RuntimeValue[array.Length * 2];
				Array.Copy(array, 0, dest, 0, size);
				array = dest;
			}

			array[size++] = new RuntimeValue(val);
		}


		internal void Trim(int num)
		{
			var iter = size - num;

			for (var i = size; i > iter; --i)
				array[i - 1] = default(RuntimeValue);

			size -= num;
		}


		internal void Replace(RuntimeValue val)
		{
			array[size - 1] = val;
		}
		#endregion


		#region Properties
		internal int Count
		{
			get { return size; }
		}


		internal RuntimeValue this[int index]
		{
			get { return array[index]; }
		}
		#endregion
	}
}