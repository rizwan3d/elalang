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

			internal override ElaValue Tail(ExecutionContext ctx)
			{
				ctx.Fail("NilList", "List is nil.");
				return Default();
			}

			internal override ElaValue Head(ExecutionContext ctx)
			{
				ctx.Fail("NilList", "List is nil.");
				return Default();
			}
		}
		#endregion


		#region Operations
		internal virtual ElaValue Head(ExecutionContext ctx)
		{
			return InternalValue;
		}


		internal virtual ElaValue Tail(ExecutionContext ctx)
		{
			return new ElaValue(InternalNext);
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
            return new ElaValue(new ElaList(this, value));

            //if (this == Empty)
            //    return new ElaValue(new ElaList(this, value));
            //else
            //{
            //    var tail = new ElaList(Empty, value);

            //    var xs = this;

            //    while (xs.InternalNext != Empty)
            //        xs = xs.InternalNext;

            //    xs.InternalNext = tail;
            //    return new ElaValue(this);
            //}
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


		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append('[');
			var cc = 0;

			foreach (var e in this)
			{
				if (cc > 30)
				{
					sb.Append(",...");
					break;
				}

				if (cc > 0)
					sb.Append(',');

				sb.Append(e.ToString());
				cc++;
			}

			sb.Append(']');
			return sb.ToString();
		}


        internal override int GetLength(ExecutionContext ctx)
        {
            var count = 0;
            ElaList xs = this;

            while (xs != Empty)
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

			while (xs != Empty)
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
		protected internal ElaValue InternalValue;
        protected internal ElaList InternalNext;

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
