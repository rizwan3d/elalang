using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public class ElaArray : ElaIndexedObject, IEnumerable<RuntimeValue>
	{
		#region Construction
		private const int DEFAULT_SIZE = 4;
		private const string ADD = "add";
		private const string REMOVE = "remove";
		private const string INSERT = "insert";
		private const string CLEAR = "clear";

		private int size;
		private RuntimeValue[] array;


		public ElaArray(RuntimeValue[] arr) : base(ObjectType.Array)
		{
			if (arr == null)
				throw new ArgumentNullException("arr");

			array = new RuntimeValue[arr.Length == 0 ? DEFAULT_SIZE : arr.Length];
			
			if (arr.Length != 0)
				Array.Copy(arr, array, arr.Length);

			size = array.Length;
		}


		public ElaArray(object[] arr) : base(ObjectType.Array)
		{
			if (arr == null)
				throw new ArgumentNullException("arr");
	
			array = new RuntimeValue[arr.Length == 0 ? DEFAULT_SIZE : arr.Length];

			for (var i = 0; i < arr.Length; i++)
				array[i] = RuntimeValue.FromObject(arr[i]);

			size = array.Length;
		}


		public ElaArray(int size) : base(ObjectType.Array)
		{
			array = new RuntimeValue[size == 0 ? DEFAULT_SIZE : size];
		}


		public ElaArray() : this(DEFAULT_SIZE)
		{
			
		}
		#endregion


		#region Ela Functions
		private sealed class AddFunction : ElaFunction
		{
			private ElaArray array;

			internal AddFunction(ElaArray array) : base(1)
			{
				this.array = array;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				array.Add(args[0]);
				return new RuntimeValue(ElaObject.Unit);
			}
		}


		private sealed class InsertFunction : ElaFunction
		{
			private ElaArray array;

			internal InsertFunction(ElaArray array) : base(2)
			{
				this.array = array;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				var index = args[0];
				var val = args[1];

				if (index.DataType != ObjectType.Integer)
					throw new ElaParameterTypeException(ObjectType.Integer, index.DataType);

				array.Insert(index.I4, val);
				return new RuntimeValue(ElaObject.Unit);
			}
		}


		private sealed class RemoveFunction : ElaFunction
		{
			private ElaArray array;

			internal RemoveFunction(ElaArray array) : base(1)
			{
				this.array = array;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				var index = args[0];

				if (index.DataType != ObjectType.Integer)
					throw new ElaParameterTypeException(ObjectType.Integer, index.DataType);

				array.Remove(index.I4);
				return new RuntimeValue(ElaObject.Unit);
			}
		}


		private sealed class ClearFunction : ElaFunction
		{
			private ElaArray array;

			internal ClearFunction(ElaArray array) : base(0)
			{
				this.array = array;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				array.Clear();
				return new RuntimeValue(ElaObject.Unit);
			}
		}
		#endregion


		#region Methods
		public IEnumerator<RuntimeValue> GetEnumerator()
		{
			return array.Take(size).GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<RuntimeValue>)this).GetEnumerator();
		}


		protected internal override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case ADD: return new RuntimeValue(new AddFunction(this));
				case REMOVE: return new RuntimeValue(new RemoveFunction(this));
				case INSERT: return new RuntimeValue(new InsertFunction(this));
				case CLEAR: return new RuntimeValue(new ClearFunction(this));
				default: return base.GetAttribute(name);
			}
		}


		protected internal override RuntimeValue GetValue(RuntimeValue index)
		{
			return index.Type == ElaMachine.INT && index.I4 < size && index.I4 > -1 ? array[index.I4] : new RuntimeValue(Invalid);
		}


		protected internal override bool SetValue(RuntimeValue index, RuntimeValue value)
		{
			if (index.Type == ElaMachine.INT && index.I4 > -1 && index.I4 < size)
			{
				array[index.I4] = value;
				return true;
			}
			else
				return false;
		}


		internal void InternalSetValue(int index, RuntimeValue value)
		{
			array[index] = value;
		}


		public void Add(RuntimeValue value)
		{
			if (size == array.Length)
				EnsureSize(size + 1);

			array[size] = value;
			size++;
		}


		public bool Remove(int index)
		{
			if (index < 0 || index >= size)
				return false;
			else
			{
				if (index < --size)
					Array.Copy(array, index + 1, array, index, size - index);
				
				if (array[size].Type > 6)
					array[size].Ref = null;

				return true;
			}
		}


		public bool Insert(int index, RuntimeValue value)
		{
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
			}
		}


		internal void Copy(int offset, ElaArray elaList)
		{
			Array.Copy(elaList.array, 0, array, offset, elaList.size);
			size += elaList.size;
		}


		internal void Stretch(int newSize)
		{
			EnsureSize(newSize);
			size = newSize;
		}


		private void EnsureSize(int newSize)
		{
			if (array.Length < newSize)
			{
				var newArr = new RuntimeValue[array.Length == 0 ? DEFAULT_SIZE : array.Length * 2];
				Array.Copy(array, newArr, array.Length);
				array = newArr;
			}
		}
		#endregion


		#region Properties
		public override int Length
		{
			get { return size; }
		}


		public RuntimeValue this[int index]
		{
			get 
			{
				if (index < size && index > -1)
					return array[index];
				else
					throw new IndexOutOfRangeException();
			}
			set 
			{
				if (index > -1 && index < size)
					array[index] = value;
				else
					throw new IndexOutOfRangeException();
			}
		}


		internal override bool ReadOnly { get { return false; } }
		#endregion
	}
}