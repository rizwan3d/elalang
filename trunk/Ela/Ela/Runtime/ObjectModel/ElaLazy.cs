using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLazy : ElaObject
	{
		internal ElaLazy(ElaFunction function) : base(ElaTypeCode.Lazy)
		{
			Function = function;
			_value = default(ElaValue);
		}

		internal ElaValue Force()
		{
			if (Value.Ref == null)
			{
				Value = Function.CallWithThrow(
					Function.LastParameter.Ref != null ? Function.LastParameter :
					new ElaValue(ElaUnit.Instance));
			}

			return Value;
		}
        
		internal override ElaValue Force(ElaValue @this, ExecutionContext ctx)
		{
			if (ctx == ElaObject.DummyContext)
				return Force();

			return Force(ctx);
		}
        
		internal ElaValue Force(ExecutionContext ctx)
		{
            var f = Function;

			if (f != null)
			{
                ctx.Failed = true;
                ctx.Thunk = this;
                return new ElaValue(this);
			}

            if (Value.Ref == this)
            {
                ctx.Fail(ElaRuntimeError.Cyclic);
                return Default();
            }

			return Value;
		}

        public override string ToString(string format, IFormatProvider provider)
        {
            return "<thunk>";
        }
		        
        protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			return new ElaValue(new ElaLazyList(this, value));
		}
        
		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return new ElaValue(this);
		}

		internal ElaFunction Function;

		private ElaValue _value;
		internal ElaValue Value
		{
			get { return _value; }
			set
			{
				Function = null;
				_value = value;
			}
		}
	}
}