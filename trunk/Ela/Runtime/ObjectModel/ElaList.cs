using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
	public class ElaList : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		public static readonly ElaList Empty = ElaNilList.Instance;

		public ElaList(ElaList next, object value) : this(next, ElaValue.FromObject(value))
		{

		}


		public ElaList(ElaList next, ElaValue value) : base(ElaTypeCode.List)
		{
			InternalNext = next;
			InternalValue = value;
		}
		#endregion


		#region Nested Classes
		private sealed class ElaNilList : ElaList
		{
			internal static readonly ElaNilList Instance = new ElaNilList();

			internal ElaNilList() : base(null, new ElaValue(ElaUnit.Instance)) { }

			protected internal override ElaValue Tail(ExecutionContext ctx)
			{
				ctx.Fail("NilList", "List is nil.");
				return Default();
			}

			protected internal override ElaValue Head(ExecutionContext ctx)
			{
				ctx.Fail("NilList", "List is nil.");
				return Default();
			}

			protected internal override bool IsNil(ExecutionContext ctx)
			{
				return true;
			}
		}
		#endregion


		#region Operations
		protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			if (index.TypeId != ElaMachine.INT)
			{
				ctx.InvalidIndexType(index);
				return Default();
			}
			else if (index.I4 < 0)
			{
				ctx.IndexOutOfRange(index, new ElaValue(this));
				return Default();
			}

			ElaList xs = this;

			for (var i = 0; i < index.I4; i++)
			{
				xs = xs.Tail(ctx).Ref as ElaList;

				if (xs == null)
				{
					ctx.Fail(new ElaError("InvalidList", "Invalid list definition."));
					return Default();
				}

				if (xs.IsNil(ctx))
				{
					ctx.IndexOutOfRange(index, new ElaValue(this));
					return Default();
				}
			}

			return xs.Head(ctx);
		}


		protected internal override ElaValue Head(ExecutionContext ctx)
		{
			return InternalValue;
		}


		protected internal override ElaValue Tail(ExecutionContext ctx)
		{
			return new ElaValue(InternalNext);
		}


		protected internal override bool IsNil(ExecutionContext ctx)
		{
			return false;
		}


		protected internal override ElaValue Cons(ElaObject next, ElaValue value, ExecutionContext ctx)
		{
			var xs = next as ElaList;

			if (xs != null)
				return new ElaValue(new ElaList(xs, value));

			ctx.Fail(ElaRuntimeError.InvalidType, ElaTypeCode.List, (ElaTypeCode)next.TypeId);
			return Default();
		}


		protected internal override ElaValue Nil(ExecutionContext ctx)
		{
			return new ElaValue(Empty);
		}


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			if (type == ElaTypeCode.List)
				return new ElaValue(this);

			ctx.ConversionFailed(@this, type);
            return Default();
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return "[" + FormatHelper.FormatEnumerable(this, ctx, info) + "]";
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			return new ElaValue(new ElaList(this, value));
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
            return new ElaValue(Reverse());
		}
		#endregion


		#region Methods
        internal override string GetTag()
        {
            return "List#";
        }


        internal override int GetLength(ExecutionContext ctx)
        {
            var count = 0;
            ElaList xs = this;

            while (!xs.IsNil(ctx))
            {
                count++;
                xs = xs.Tail(ctx).Ref as ElaList;

                if (xs == null)
                {
                    if (ctx == ElaObject.DummyContext)
                        throw new ElaRuntimeException("InvalidList", "Invalid list definition.");
                    else
                    {
                        ctx.Fail("InvalidList", "Invalid list definition.");
                        return 0;
                    }
                }
            }

            return count;
        }
        
        
        public override ElaPatterns GetSupportedPatterns()
        {
            return ElaPatterns.Tuple|ElaPatterns.HeadTail;
        }


		public static ElaList FromEnumerable(IEnumerable seq)
		{
			var list = ElaList.Empty;

			foreach (var e in seq)
				list = new ElaList(list, ElaValue.FromObject(e));

			return list.Reverse();
		}


		public static ElaList FromEnumerable(IEnumerable<ElaValue> seq)
		{
			var list = ElaList.Empty;

			foreach (var e in seq)
				list = new ElaList(list, e);

			return list.Reverse();
		}


        public ElaList Concatenate(ElaList other)
        {
            var list = other;

            foreach (var e in Reverse())
                list = new ElaList(list, e);

            return list;
        }


		public virtual ElaList Reverse()
		{
			var newLst = ElaList.Empty;
			var lst = this;

			while (lst != ElaList.Empty)
			{
				newLst = new ElaList(newLst, lst.Value);
				lst = lst.Next;
			}

			return newLst;
		}


		public IEnumerator<ElaValue> GetEnumerator()
		{
			ElaList xs = this;
			var ctx = new ExecutionContext();

			while (!xs.IsNil(ctx))
			{
				yield return xs.Head(ctx);
				xs = xs.Tail(ctx).Ref as ElaList;

				if (xs == null)
					throw InvalidDefinition();
			}
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		protected virtual Exception InvalidDefinition()
		{
			return new ElaRuntimeException("InvalidList", "Invalid list definition.");
		}
		#endregion


		#region Properties
		protected ElaValue InternalValue;
		protected ElaList InternalNext;

		public virtual ElaList Next
		{
			get
			{
                if (InternalNext == null)
                    return null;
				else
					return InternalNext;
			}
		}

		public virtual ElaValue Value
		{
			get { return InternalValue; }
		}

		public virtual int Length
		{
			get { return GetLength(ElaObject.DummyContext); }
		}
		#endregion
	}
}
