using System;
using System.Collections;
using System.Collections.Generic;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
using System.Text;

namespace Ela.Library.Collections
{
	public sealed class ElaArray : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
        private const string TAG = "Array#";
		private const int DEFAULT_SIZE = 4;
		private int size;
		private ElaValue[] array;
		internal int headIndex;


        public ElaArray(ElaMachine vm) : this(vm.GetTypeId(TAG))
        {

        }


        public ElaArray(ElaValue[] arr, ElaMachine vm) : this(arr, vm.GetTypeId(TAG))
        {

        }


        public ElaArray(object[] arr, ElaMachine vm) : this(arr, vm.GetTypeId(TAG))
        {

        }


        internal ElaArray(ElaValue[] arr, TypeId typeId) : base(typeId)
		{
			if (arr == null)
				throw new ArgumentNullException("arr");

			array = new ElaValue[arr.Length == 0 ? DEFAULT_SIZE : arr.Length];

			if (arr.Length != 0)
			{
				Array.Copy(arr, array, arr.Length);
				size = array.Length;
			}
		}


        internal ElaArray(object[] arr, TypeId typeId) : base(typeId)
		{
			if (arr == null)
				throw new ArgumentNullException("arr");

			array = new ElaValue[arr.Length == 0 ? DEFAULT_SIZE : arr.Length];

			for (var i = 0; i < arr.Length; i++)
				array[i] = ElaValue.FromObject(arr[i]);

			if (arr.Length != 0)
				size = array.Length;
		}


		internal ElaArray(int size, TypeId typeId) : base(typeId)
		{
			array = new ElaValue[size == 0 ? DEFAULT_SIZE : size];
		}


		internal ElaArray(TypeId typeId) : this(DEFAULT_SIZE, typeId)
		{

		}


		internal ElaArray(ElaValue[] arr, int size, int headIndex, TypeId typeId) : base(typeId)
		{
			array = arr;
			this.size = size;
			this.headIndex = headIndex;
		}
		#endregion
		

		#region Operations
        internal ElaValue GetValue(int index)
		{
            if (index >= Length || index < 0)
                throw new ElaRuntimeException(ElaRuntimeError.IndexOutOfRange);

            return array[index];
		}


		internal void SetValue(int index, ElaValue value)
		{
			if (index < 0 || index >= Length)
                throw new ElaRuntimeException(ElaRuntimeError.IndexOutOfRange);

			array[index] = value;
		}


		internal ElaValue Head()
		{
			return array[headIndex];
		}


		internal ElaArray Tail(TypeId typeId)
		{
			return new ElaArray(array, size, headIndex + 1, typeId);
		}


		internal bool IsNil()
		{
			return headIndex == size;
		}


		internal ElaArray Generate(ElaValue value)
		{
			Add(value);
			return this;			
		}
		#endregion


		#region Methods
        public override string GetTag()
        {
            return TAG;
        }


		public IEnumerator<ElaValue> GetEnumerator()
		{
			for (var i = 0; i < Length; i++)
				yield return array[i];
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		internal ElaValue FastGet(int index)
		{
			return index < Length ? array[index] : Default();
		}


		internal void FastSet(int index, ElaValue value)
		{
			array[index] = value;
		}


		public void Add(ElaValue value)
		{
			if (size == array.Length)
				EnsureSize(size + 1);

			array[size] = value;
			size++;
		}


		public bool Remove(int index)
		{
			index += headIndex;

			if (index < 0 || index >= size)
				return false;
			else
			{
				if (index < --size)
					Array.Copy(array, index + 1, array, index, size - index);

				array[size] = default(ElaValue);
				return true;
			}
		}


		public bool Insert(int index, ElaValue value)
		{
			index += headIndex;

			if (index < 0 || index >= size)
				return false;
			else
			{
				if (size == array.Length)
					EnsureSize(size + 1);

				if (index < size)
					Array.Copy(array, index, array, index + 1, size - index);

				array[index] = value;
				size++;
				return true;
			}
		}


		public void Clear()
		{
			if (size > 0)
			{
				Array.Clear(array, 0, size);
				size = 0;
				headIndex = 0;
			}
		}


		internal void Copy(int offset, ElaArray elaArr)
		{
			Array.Copy(elaArr.array, 0, array, offset, elaArr.Length);
			size += elaArr.size;
		}


		private void EnsureSize(int newSize)
		{
			if (array.Length < newSize)
			{
				var newArr = new ElaValue[array.Length == 0 ? DEFAULT_SIZE : array.Length * 2];
				Array.Copy(array, newArr, array.Length);
				array = newArr;
			}
		}


        internal ElaValue[] GetRawArray()
        {
            return array;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("array[");

            for (var i = headIndex; i < size; i++)
            {
                if (i > headIndex)
                    sb.Append(',');

                sb.Append(array[i].ToString());
            }

            sb.Append(']');
            return sb.ToString();
        }
		#endregion


		#region Properties
		public int Length
		{
			get { return size - headIndex; }
		}


		public ElaValue this[int index]
		{
			get
			{
				if (index < Length && index > -1)
					return array[index];
				else
					throw new IndexOutOfRangeException();
			}
			set
			{
				if (index > -1 && index < Length)
					array[index] = value;
				else
					throw new IndexOutOfRangeException();
			}
		}
		#endregion
	}
}