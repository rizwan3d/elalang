﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Ela.Debug;

namespace Ela.Runtime.ObjectModel
{
	public class ElaList : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Show | ElaTraits.Eq | ElaTraits.Get | ElaTraits.Len | ElaTraits.Gen | ElaTraits.Fold | ElaTraits.Cons | ElaTraits.Concat | ElaTraits.Convert | ElaTraits.Seq;
		internal static readonly ElaList Nil = new ElaList(null, new ElaValue(ElaUnit.Instance));

		public ElaList(object value)
			: this(Nil, ElaValue.FromObject(value))
		{

		}


		public ElaList(ElaObject next, object value)
			: this(next, ElaValue.FromObject(value))
		{

		}


		public ElaList(ElaObject next, ElaValue value)
			: base(ObjectType.List, TRAITS)
		{
			InternalNext = next;
			InternalValue = value;
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

			if (index.Type != ElaMachine.INT)
			{
				ctx.InvalidIndexType(index.DataType);
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
			return this == Nil;
		}


		protected internal override ElaValue Cons(ElaObject next, ElaValue value, ExecutionContext ctx)
		{
			if (next is ElaList || (next.Traits & ElaTraits.Thunk) == ElaTraits.Thunk)
				return new ElaValue(new ElaList(next, value));

			ctx.Fail(ElaRuntimeError.InvalidType, ObjectType.List, (ObjectType)next.TypeId);
			return Default();
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.LST)
			{
				if (right.Type == ElaMachine.LST)
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


		protected internal override ElaValue Convert(ObjectType type, ExecutionContext ctx)
		{
			if (type == ObjectType.List)
				return new ElaValue(this);

			ctx.ConversionFailed(new ElaValue(this), type);
			return base.Convert(type, ctx);
		}


		protected internal override string Show(ExecutionContext ctx, ShowInfo info)
		{
			return "[" + FormatHelper.FormatEnumerable((IEnumerable<ElaValue>)this, ctx, info) + "]";
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
			var list = ElaList.Nil;

			foreach (var e in seq)
				list = new ElaList(list, ElaValue.FromObject(e));

			return list.Reverse();
		}


		public static ElaList FromEnumerable(IEnumerable<Object> seq)
		{
			return FromEnumerable(seq.Select<Object,ElaValue>(ElaValue.FromObject));
		}


		public static ElaList FromEnumerable(IEnumerable<ElaValue> seq)
		{
			var list = ElaList.Nil;

			foreach (var e in seq.Reverse())
				list = new ElaList(list, e);

			return list;
		}


		public static ElaList GetNil()
		{
			return ElaList.Nil;
		}
		

		public ElaList Reverse()
		{
			var newLst = ElaList.Nil;
			var lst = this;

			while (lst != ElaList.Nil)
			{
				newLst = new ElaList(newLst, lst.Value);
				lst = lst.Next;
			}

			return newLst;
		}


		public IEnumerator<ElaValue> GetEnumerator()
		{
			if (this != Nil)
			{
				ElaObject xs = this;

				do
				{
					yield return xs.Head(ElaObject.DummyContext).Id(DummyContext);
					xs = xs.Tail(ElaObject.DummyContext).Ref;

					if ((xs.Traits & ElaTraits.Thunk) == ElaTraits.Thunk)
						xs.Force(ElaObject.DummyContext);
					else if ((xs.Traits & ElaTraits.Fold) != ElaTraits.Fold)
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
		internal ElaValue InternalValue;

		internal ElaObject InternalNext;

		public ElaList Next
		{
			get
			{
				return InternalNext != null && InternalNext.TypeId == ElaMachine.LST ?
					(ElaList)InternalNext : null;
			}
		}

		public ElaValue Value
		{
			get { return InternalValue; }
		}

		public int Length
		{
			get { return GetLength(DummyContext).AsInteger(); }
		}
		#endregion
	}
}
