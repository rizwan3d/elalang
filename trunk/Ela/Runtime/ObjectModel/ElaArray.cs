using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Ela.Runtime.Reflection;
using Ela.Debug;

namespace Ela.Runtime.ObjectModel
{
	public class ElaArray : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		private const int DEFAULT_SIZE = 4;
		private const ElaTraits TRAITS = ElaTraits.Eq | ElaTraits.Len | ElaTraits.Get | ElaTraits.Set | ElaTraits.Gen | ElaTraits.Fold | ElaTraits.Concat | ElaTraits.Convert | ElaTraits.Show | ElaTraits.FieldGet | ElaTraits.Seq;
		private int size;
		private ElaValue[] array;
		private int headIndex;

		private const string ADD = "add";
		private const string INSERT = "insert";
		private const string REMOVE = "removeAt";
		private const string CLEAR = "clear";
		private const string LENGTH = "length";

		public ElaArray(ElaValue[] arr) : base(ObjectType.Array, TRAITS)
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


		public ElaArray(object[] arr) : base(ObjectType.Array, TRAITS)
		{
			if (arr == null)
				throw new ArgumentNullException("arr");

			array = new ElaValue[arr.Length == 0 ? DEFAULT_SIZE : arr.Length];

			for (var i = 0; i < arr.Length; i++)
				array[i] = ElaValue.FromObject(arr[i]);

			if (arr.Length != 0)
				size = array.Length;
		}


		public ElaArray(int size) : base(ObjectType.Array, TRAITS)
		{
			array = new ElaValue[size == 0 ? DEFAULT_SIZE : size];
		}


		public ElaArray() : this(DEFAULT_SIZE)
		{

		}


		private ElaArray(ElaValue[] arr, int size, int headIndex) : base(ObjectType.Array, TRAITS)
		{
			array = arr;
			this.size = size;
			this.headIndex = headIndex;
		}
		#endregion


		#region Ela Functions
		private abstract class ArrayFunction : ElaFunction
		{
			protected ArrayFunction(ElaArray arr, int args)
				: base(args)
			{
				Instance = arr;
			}

			protected ElaArray Instance { get; private set; }
		}


		private sealed class _Add : ArrayFunction
		{
			internal _Add(ElaArray arr) : base(arr, 1) { }

			public override ElaValue Call(params ElaValue[] args)
			{
				Instance.Add(args[0]);
				return Default();
			}
		}


		private sealed class _Remove : ArrayFunction
		{
			internal _Remove(ElaArray arr) : base(arr, 1) { }

			public override ElaValue Call(params ElaValue[] args)
			{
				Instance.Remove(args[0].Id(DummyContext).AsInteger());
				return Default();
			}
		}


		private sealed class _Insert : ArrayFunction
		{
			internal _Insert(ElaArray arr) : base(arr, 2) { }

			public override ElaValue Call(params ElaValue[] args)
			{
				Instance.Insert(args[0].Id(DummyContext).AsInteger(), args[1]);
				return Default();
			}
		}


		private sealed class _Clear : ArrayFunction
		{
			internal _Clear(ElaArray arr) : base(arr, 1) { }

			public override ElaValue Call(params ElaValue[] args)
			{
				Instance.Clear();
				return Default();
			}
		}
		#endregion


		#region Traits
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.Ref == right.Ref);
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.Ref != right.Ref);
		}


		protected internal override ElaValue GetLength(ExecutionContext ctx)
		{
			return new ElaValue(size - headIndex);
		}


		protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			index = index.Id(ctx);

			if (index.Type != ElaMachine.INT)
			{
				ctx.InvalidIndexType(index.DataType);
				return Default();
			}
			else if (index.I4 >= Length || index.I4 < 0)
			{
				ctx.IndexOutOfRange(index, new ElaValue(this));
				return Default();
			}

			return array[index.I4];
		}


		protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
		{
			index = index.Id(ctx);

			if (index.Type != ElaMachine.INT)
			{
				ctx.InvalidIndexType(index.DataType);
				return;
			}

			if (index.I4 < 0 || index.I4 >= Length)
			{
				ctx.IndexOutOfRange(index, new ElaValue(this));
				return;
			}

			array[index.I4] = value;
		}


		protected internal override ElaValue Head(ExecutionContext ctx)
		{
			return array[headIndex];
		}


		protected internal override ElaValue Tail(ExecutionContext ctx)
		{
			return new ElaValue(new ElaArray(array, size, headIndex + 1));
		}


		protected internal override bool IsNil(ExecutionContext ctx)
		{
			return headIndex == size;
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			Add(value);
			return new ElaValue(this);			
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return new ElaValue(this);
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.ARR)
			{
				if (right.Type == ElaMachine.ARR)
				{
					var thisArr = (ElaArray)left.Ref;
					var otherArr = (ElaArray)right.Ref;
					var arr = new ElaValue[thisArr.Length + otherArr.Length];
					Array.Copy(thisArr.array, 0, arr, 0, thisArr.Length);
					Array.Copy(otherArr.array, 0, arr, thisArr.Length, otherArr.Length);
					return new ElaValue(new ElaArray(arr, arr.Length, 0));
				}
				else
					return right.Ref.Concatenate(left, right, ctx);
			}
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Concat);
				return Default();
			}
		}


		protected internal override ElaValue Convert(ObjectType type, ExecutionContext ctx)
		{
			if (type == ObjectType.Array)
				return new ElaValue(this);

			ctx.ConversionFailed(new ElaValue(this), type);
			return base.Convert(type, ctx);
		}


		protected internal override string Show(ExecutionContext ctx, ShowInfo info)
		{
			return "[|" + FormatHelper.FormatEnumerable((IEnumerable<ElaValue>)this, ctx, info) + "|]";
		}


		protected internal override ElaValue GetField(string field, ExecutionContext ctx)
		{
			switch (field)
			{
				case ADD: return new ElaValue(new _Add(this));
				case INSERT: return new ElaValue(new _Insert(this));
				case REMOVE: return new ElaValue(new _Remove(this));
				case CLEAR: return new ElaValue(new _Clear(this));
				case LENGTH: return new ElaValue(Length);
				default:
					ctx.UnknownField(field, new ElaValue(this));
					return Default();
			}
		}


		protected internal override bool HasField(string field, ExecutionContext ctx)
		{
			return field == ADD || field == INSERT || field == REMOVE || field == CLEAR || field == LENGTH;
		}
		#endregion


		#region Methods
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

				if (array[size].Type > 6)
					array[size].Ref = null;

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


		internal void Copy(int offset, ElaArray elaList)
		{
			Array.Copy(elaList.array, 0, array, offset + headIndex, elaList.Length);
			size += elaList.size;
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