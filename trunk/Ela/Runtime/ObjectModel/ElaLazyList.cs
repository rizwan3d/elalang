using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
	public class ElaLazyList : ElaList
	{
		#region Construction
		public static readonly new ElaList Empty = ElaNilLazyList.Instance;
		private ElaLazy thunk;
		
		public ElaLazyList(ElaLazy next, object value) : this(next, ElaValue.FromObject(value))
		{

		}


		public ElaLazyList(ElaLazy next, ElaValue value) : this((ElaLazyList)null, value)
		{
			this.thunk = next;
		}


		public ElaLazyList(ElaLazyList next, ElaValue value) : base(next, value)
		{
			
		}
		#endregion

				
		#region Nested Classes
		private sealed class ElaNilLazyList : ElaLazyList
		{
			internal static readonly ElaNilLazyList Instance = new ElaNilLazyList();

			internal ElaNilLazyList() : base((ElaLazy)null, new ElaValue(ElaUnit.Instance)) { }

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
		protected internal override ElaValue Tail(ExecutionContext ctx)
		{
			if (thunk != null)
			{
				InternalNext = thunk.Force(ctx).Ref as ElaList;

				if (InternalNext == null)
					throw InvalidDefinition();

				thunk = null;
			}

			return new ElaValue(InternalNext);
		}


		protected internal override ElaValue Nil(ExecutionContext ctx)
		{
			return new ElaValue(ElaLazyList.Empty);
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Ref != this)
			{
				var xs = (ElaList)left.Ref;
				var newLst = this;
				
				foreach (var e in xs.Reverse())
					newLst = new ElaLazyList(newLst, e);

				return new ElaValue(newLst);
			}
			else if (right.Ref != this)
			{
				var xs = ElaList.FromEnumerable(this).Concatenate((ElaList)right.Ref);
				return new ElaValue(xs);
			}
			
			ctx.InvalidLeftOperand(left, right, "concat");
			return Default();
		}


		protected internal override ElaValue Cons(ElaObject next, ElaValue value, ExecutionContext ctx)
		{
			var t = next as ElaLazy;

			if (t != null)
				return new ElaValue(new ElaLazyList(t, value));

			var tl = next as ElaLazyList;

			if (tl != null)
				return new ElaValue(new ElaLazyList(tl, value));

			ctx.Fail("InvalidLazyList", "Invalid lazy list definition.");
			return Default();
		}


		protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			var nin = new ShowInfo(info.StringLength, 50, info.Format);
			return "[" + FormatHelper.FormatEnumerable(this, ctx, nin) + "]";
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			if (thunk != null)
				return new ElaValue(new ElaLazyList(thunk, value));
			else
				return new ElaValue(new ElaLazyList((ElaLazyList)InternalNext, value));
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return new ElaValue(this);
		}
		#endregion


		#region Methods
		protected override Exception InvalidDefinition()
		{
			return new ElaRuntimeException("InvalidLazyList", "Invalid lazy list definition.");
		}
		#endregion


		#region Properties
		public override ElaList Next
		{
			get
			{
				if (thunk != null)
				{
					var xs = thunk.Force().Ref as ElaList;

					if (xs == null)
						throw InvalidDefinition();

					thunk = null;
					InternalNext = xs;
				}

				return InternalNext;
			}
		}
		#endregion
	}
}
