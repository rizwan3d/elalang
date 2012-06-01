using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
	public class ElaList : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.List), (Int32)ElaTypeCode.List, true, typeof(ElaList));
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
		protected internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return Equal(left, right.Force(ctx), "equal", ctx);
		}


		protected internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return !Equal(left, right.Force(ctx), "notequal", ctx);
		}


		private bool Equal(ElaValue left, ElaValue right, string op, ExecutionContext ctx)
		{
			if (left.TypeId != ElaMachine.LST)
			{
				ctx.InvalidLeftOperand(left, right, op);
				return false;
			}

			if (right.TypeId != ElaMachine.LST)
			{
				ctx.InvalidRightOperand(left, right, op);
				return false;
			}

			ElaList xs1 = (ElaList)left.Ref;
			ElaList xs2 = (ElaList)right.Ref;

			while (!xs1.IsNil(ctx) && !xs2.IsNil(ctx))
			{
				var eq = xs1.Value.Equal(xs1.Value, xs2.Value, ctx);

				if (!eq)
					return false;

				xs1 = xs1.Tail(ctx).Ref as ElaList;
				xs2 = xs2.Tail(ctx).Ref as ElaList;

				if (xs1 == null || xs2 == null)
				{
					ctx.Fail(new ElaError("InvalidList", "Invalid list definition."));
					return false;
				}
			}

			return xs1.IsNil(ctx) && xs2.IsNil(ctx);
		}


		protected internal override ElaValue GetLength(ExecutionContext ctx)
		{
			var count = 0;
			ElaList xs = this;
			
			while (!xs.IsNil(ctx))
			{
				count++;
				xs = xs.Tail(ctx).Ref as ElaList;

				if (xs == null)
				{
					ctx.Fail(new ElaError("InvalidList", "Invalid list definition."));
					return Default();
				}
			}

			return new ElaValue(count);
		}


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


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.LST && right.TypeId == ElaMachine.LST)
			{
				var list = (ElaList)right.Ref;
                var revList = ((ElaList)left.Ref).Reverse(ctx);

                if (ctx.Failed)
                    return Default();

				foreach (var e in revList)
					list = new ElaList(list, e);

				return new ElaValue(list);				
			}
			else if (left.TypeId == ElaMachine.LST)
				return right.Ref.Concatenate(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "concat");
			return Default();
		}


        protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
			if (type.ReflectedTypeCode == ElaTypeCode.List)
				return new ElaValue(this);

            ctx.ConversionFailed(@this, type.ReflectedTypeName);
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
            var ctx = new ExecutionContext();
            return Reverse(ctx);
		}
        
        public virtual ElaList Reverse(ExecutionContext ctx)
		{
            var newLst = ElaList.Empty;
			var lst = this;

            while (!lst.IsNil(ctx))
            {
				newLst = new ElaList(newLst, lst.Value);
				lst = lst.Tail(ctx).Ref as ElaList;

                if (ctx.Failed)
                    return null;
			}

			return newLst;
		}


        public virtual ElaValue Tail()
        {
            return new ElaValue(InternalNext);
        }


        public virtual ElaValue Head()
        {
            return InternalValue;
        }


		public IEnumerator<ElaValue> GetEnumerator()
		{
			ElaList xs = this;
			var ctx = new ExecutionContext();

			while (!xs.IsNil(ctx))
			{
				yield return xs.Head();

                

                var tl = xs.Tail().Ref;
				xs = tl as ElaList;

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
			get { return GetLength(DummyContext).AsInteger(); }
		}
		#endregion
	}
}
