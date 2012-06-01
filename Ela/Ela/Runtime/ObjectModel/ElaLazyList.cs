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
        public override ElaValue Tail()
        {
            if (thunk != null)
            {
                InternalNext = thunk.Force().Ref as ElaList;
                thunk = null;
            }

            return new ElaValue(InternalNext);
        }

		protected internal override ElaValue Tail(ExecutionContext ctx)
		{
			if (thunk != null)
			{
				InternalNext = thunk.Force(new ElaValue(thunk), ctx).Ref as ElaList;

                if (InternalNext == null)
                {
                    ctx.Fail("InvalidLazyList", "Invalid lazy list definition.");
                    return Default();
                }

				thunk = null;
			}

			return new ElaValue(InternalNext);
		}


		protected internal override ElaValue Nil(ExecutionContext ctx)
		{
			return new ElaValue(ElaLazyList.Empty);
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
            var sb = new StringBuilder();
            ElaList xs = this;
            var cc = 0;
            sb.Append('[');
            
            while (!xs.IsNil(ctx))
            {
                if (cc++ > 0)
                    sb.Append(',');

                if (cc > 100)
                {
                    sb.Append("...");
                    break;
                }

                var h = xs.Head(ctx);
                sb.Append(h.Show(info, ctx));
                xs = xs.Tail(ctx).Ref as ElaList;

                if (xs == null)
                {
                    sb.Append(",<error>");
                    break;
                }
            }

            sb.Append(']');
            return sb.ToString();
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
