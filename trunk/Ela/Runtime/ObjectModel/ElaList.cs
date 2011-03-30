using System;
using System.Collections;
using System.Collections.Generic;
using Ela.Debug;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
	public class ElaList : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Show | ElaTraits.Eq | ElaTraits.Get | ElaTraits.Len | ElaTraits.Gen | ElaTraits.Seq | ElaTraits.Cons | ElaTraits.Concat | ElaTraits.Convert | ElaTraits.Ix;
		internal static readonly ElaList Empty = ElaNilList.Instance;

		public ElaList(ElaObject next, object value) : this(next, ElaValue.FromObject(value))
		{

		}


		public ElaList(ElaObject next, ElaValue value) : base(ElaTypeCode.List, TRAITS)
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
			var count = 0;
			ElaObject xs = this;

			while (!xs.IsNil(ctx))
			{
				count++;
				xs = xs.Tail(ctx).Ref;

				if (xs.TypeId == ElaMachine.LAZ)
				{
					var la = (ElaLazy)xs;

					if (la.Value.Ref == null)
					{
						try
						{
							la.Value = la.Force();
						}
						catch (ElaCodeException ex)
						{
							ctx.Fail(ex.ErrorObject);
							return Default();
						}
					}
				}
			}

			return new ElaValue(count);
		}


		protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			index = index.Id(ctx);

			if (index.TypeId != ElaMachine.INT)
			{
				ctx.InvalidIndexType(index);
				return base.GetValue(index, ctx);
			}
			else if (index.I4 < 0)
			{
				ctx.IndexOutOfRange(index, new ElaValue(this));
				return base.GetValue(index, ctx);
			}

			ElaObject l = this;

			for (var i = 0; i < index.I4; i++)
			{
				l = l.Tail(ctx).Ref;

				if (l.TypeId == ElaMachine.LAZ)
				{
					var la = (ElaLazy)l;

					if (la.Value.Ref == null)
					{
						try
						{
							la.Value = la.Force();
						}
						catch (ElaCodeException ex)
						{
							ctx.Fail(ex.ErrorObject);
							return base.GetValue(index, ctx);
						}
					}
				}

				if (l.IsNil(ctx))
				{
					ctx.IndexOutOfRange(index, new ElaValue(this));
					return base.GetValue(index, ctx);
				}
			}

			return l.Head(ctx);
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
			return this == Empty;
		}


		protected internal override ElaValue Cons(ElaObject next, ElaValue value, ExecutionContext ctx)
		{
			if (next is ElaList || (next.Traits & ElaTraits.Thunk) == ElaTraits.Thunk)
				return new ElaValue(new ElaList(next, value));

			ctx.Fail(ElaRuntimeError.InvalidType, ElaTypeCode.List, (ElaTypeCode)next.TypeId);
			return Default();
		}


		protected internal override ElaValue Nil(ExecutionContext ctx)
		{
			return new ElaValue(Empty);
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.LST)
			{
				if (right.TypeId == ElaMachine.LST)
				{
					var list = (ElaList)right.Ref;

					foreach (var e in ((ElaList)left.Ref).Reverse())
						list = new ElaList(list, e);

					return new ElaValue(list);
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


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			if (type == ElaTypeCode.List)
				return new ElaValue(this);

			ctx.ConversionFailed(new ElaValue(this), type);
            return Default();
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			var maxLen = info.SequenceLength;
			var sb = new StringBuilder();
			sb.Append('[');
			var count = 0;

			if (this != Empty)
			{
				ElaObject xs = this;

				do
				{
					if (maxLen > 0 && count > maxLen)
					{
						sb.Append("...");
						break;
					}
					
					var v = xs.Head(ElaObject.DummyContext).Id(DummyContext);

					if (count > 0)
						sb.Append(',');

					sb.Append(v.Show(info, ctx));
					count++;
					xs = xs.Tail(ElaObject.DummyContext).Ref;

					if ((xs.Traits & ElaTraits.Thunk) == ElaTraits.Thunk)
					{
						if (count < 50)
							xs.Force(ElaObject.DummyContext);
						else
						{
							sb.Append("...");
							break;
						}
					}
					else if (xs.TypeId != ElaMachine.LST)
						break;
				}
				while (!xs.IsNil(ElaObject.DummyContext));
			}

			sb.Append(']');
			return sb.ToString();
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


		public static ElaList GetNil()
		{
			return ElaList.Empty;
		}


        public ElaList Concatenate(ElaList other)
        {
            var list = other;

            foreach (var e in Reverse())
                list = new ElaList(list, e);

            return list;
        }


        public bool HasLazyTail()
        {
            return InternalNext != null && (InternalNext.Traits & ElaTraits.Thunk) == ElaTraits.Thunk;
        }
		

		public ElaList Reverse()
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


        public IEnumerable<ElaValue> GetEagerPart()
        {
            if (this != Empty)
            {
                ElaObject xs = this;

                do
                {
                    yield return xs.Head(ElaObject.DummyContext).Id(DummyContext);
					xs = xs.Tail(ElaObject.DummyContext).Ref;

                    if (xs.TypeId != ElaMachine.LST)
                        yield break;
                }
				while (!xs.IsNil(ElaObject.DummyContext));
            }
        }


		public IEnumerator<ElaValue> GetEnumerator()
		{
			if (this != Empty)
			{
				ElaObject xs = this;

				do
				{
					yield return xs.Head(ElaObject.DummyContext).Id(DummyContext);
					xs = xs.Tail(ElaObject.DummyContext).Ref;

					if ((xs.Traits & ElaTraits.Thunk) == ElaTraits.Thunk)
						xs.Force(ElaObject.DummyContext);
					else if (xs.TypeId != ElaMachine.LST)
						yield break;
				}
				while (!xs.IsNil(ElaObject.DummyContext));
			}
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion


		#region Properties
		internal protected ElaValue InternalValue;

		internal protected ElaObject InternalNext;

		public virtual ElaList Next
		{
			get
			{
                if (InternalNext == null)
                    return null;
                else if ((InternalNext.Traits & ElaTraits.Thunk) == ElaTraits.Thunk)
                {
                    var val = InternalNext.Force(DummyContext);

                    if (val.TypeId == ElaMachine.LST)
                        return (ElaList)val.Ref;
                    else
                        return new ElaList(ElaList.Empty, val);
                }
                else if (InternalNext.TypeId == ElaMachine.LST)
                    return (ElaList)InternalNext;
                else
                    return null;
			}
		}

		public virtual ElaValue Value
		{
			get { return InternalValue; }
		}

		public virtual int Length
		{
			get { return GetLength(DummyContext).AsInteger(); }
		}
		#endregion
	}
}
