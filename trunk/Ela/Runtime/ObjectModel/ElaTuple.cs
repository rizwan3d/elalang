using System;
using System.Collections;
using System.Collections.Generic;
using Ela.Debug;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaTuple : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		private int cons;
		
		public ElaTuple(params object[] args) : base(ElaTypeCode.Tuple)
		{
			Values = new ElaValue[args.Length];

            for (var i = 0; i < args.Length; i++)
                Values[i] = ElaValue.FromObject(args[i]);

			cons = args.Length;
		}


		public ElaTuple(params ElaValue[] args) : base(ElaTypeCode.Tuple)
		{
			Values = args;
			cons = args.Length;
		}


		internal ElaTuple(int size) : this(size, ElaTypeCode.Tuple)
		{

		}

		
		internal ElaTuple(int size, ElaTypeCode type) : base(type)
		{
			Values = new ElaValue[size];
		}


		private ElaTuple() : base(ElaTypeCode.Tuple)
		{

		}
		#endregion


		#region Operations
        internal override string GetTag()
        {
            return "Tuple#";
        }


        internal override int GetLength(ExecutionContext ctx)
        {
            return Length;
        }


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return "(" + FormatHelper.FormatEnumerable((IEnumerable<ElaValue>)this, ctx, info) + 
				(Length < 2 ? ",)" : ")");
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			InternalSetValue(value);
			return new ElaValue(this);
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return new ElaValue(this);
		}
		#endregion


		#region Methods
        internal static ElaTuple FromArray(ElaValue[] array)
		{
			var tup = new ElaTuple();
			tup.Values = array;
			tup.cons = array.Length;
			return tup;
		}


		internal ElaValue FastGet(int index)
		{
			return index < Length ? Values[index] : Default();
		}


		public IEnumerator<ElaValue> GetEnumerator()
		{
			for (var i = 0; i < Values.Length; i++)
                yield return Values[i];
		}


        IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		internal void InternalSetValue(ElaValue value)
		{
			if (value.TypeId == ElaMachine.TUP)
			{
				var other = (ElaTuple)value.Ref;
				var arr = new ElaValue[Length + other.Length + (Values.Length - Length - 1)];
				Array.Copy(Values, 0, arr, 0, Length);
				other.Values.CopyTo(arr, Length);
				Values = arr;
				cons += other.Length;
			}
			else
				InternalSetValue(Length, value);
		}


		internal void InternalSetValue(int index, ElaValue value)
		{
			if (Length == Values.Length)
			{
				var arr = new ElaValue[Length + 1];
				Values.CopyTo(arr, 0);
				Values = arr;
			}
			
			Values[index] = value;
			cons++;
		}


		private ElaTuple GetOther(ElaTuple @this, ElaValue other, ExecutionContext ctx)
		{
			var t = other.Ref as ElaTuple;

			if (t == null)
			{
				ctx.InvalidType(GetTypeName(), other);
				return null;
			}

			if (@this != null && t.Length != @this.Length)
			{
				ctx.Fail(ElaRuntimeError.TuplesLength, ToString(), other.ToString());
				return null;
			}

			return t;
		}
		#endregion


        #region Properties
        public ElaValue this[int index]
		{
			get
			{
				if (index < Length && index > -1)
					return Values[index];
				else
					throw new IndexOutOfRangeException();
			}
		}


		public int Length 
		{ 
			get { return cons; } 
		}
		
		
		internal ElaValue[] Values { get; private set; }
		#endregion
	}
}
