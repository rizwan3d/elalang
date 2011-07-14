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
		internal ElaLazy thunk;
		
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
		internal override ElaValue Tail(ExecutionContext ctx)
		{
			if (thunk != null)
			{
				if (thunk.IsEvaluated())
				{
					InternalNext = thunk.Value.Ref as ElaList;

					if (InternalNext == null)
					{
						ctx.Fail("InvalidLazyList", "Invalid lazy list definition.");
						return Default();
					}

					thunk = null;
				}
				else
				{
					ctx.Failed = true;
					ctx.Thunk = thunk;
					return Default();
				}
			}

			return new ElaValue(InternalNext);
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


		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append('[');
			sb.Append(InternalValue.ToString());

			var ctx = new ExecutionContext();
			var cc = 0;
			var tail = default(ElaList);
			var xs = (ElaList)this;

			do
			{
				tail = xs.Tail(ctx).Ref as ElaList;
				xs = tail;

				if (tail != null)
				{
					sb.Append(',');
					sb.Append(tail.InternalValue.ToString());
				}
				else
				{
					sb.Append(",<thunk>...");
				}

				cc++;
			}
			while (!ctx.Failed && cc < 30);

			sb.Append(']');
			return sb.ToString();
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
