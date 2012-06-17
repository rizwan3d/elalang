using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
	public class ElaLazyList : ElaList
	{
		#region Construction
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
                    ctx.Fail("InvalidLazyList", "Invalid lazy rec definition.");
                    return Default();
                }

				thunk = null;
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


        public override string ToString(string format, IFormatProvider formatProvider)
        {
            return "[lazy list]";
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
