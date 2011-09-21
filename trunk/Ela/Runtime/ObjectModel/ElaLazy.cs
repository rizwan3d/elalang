using System;
using Ela.CodeModel;
using Ela.Compilation;
using System.Threading;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLazy : ElaObject
	{
		#region Construction
		private const int SEVAL = -1000;

		internal ElaLazy(ElaFunction function) : base(ElaTypeCode.Lazy)
		{
			Function = function;
			_value = default(ElaValue);
		}
		#endregion


		#region Methods
        internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
        {
            if (IsEvaluated())
                return Value.Ref.Convert(Value, type, ctx);
            else
            {
                ctx.Failed = true;
                ctx.Thunk = this;
                return Default();
            }
		}


		public override string ToString()
		{
			return Value.Ref == null ? "<thunk>" : Value.ToString();
		}


		public override string GetTag()
        {
            return Value.Ref == null ? "Lazy#" : Value.GetTag();
        }


        internal override bool IsEvaluated()
        {
            return Value.Ref != null;
        }


		public override int GetHashCode()
        {
            return Value.Ref != null ? Value.GetHashCode() : 0;
        }


        protected internal override T As<T>(ElaValue value)
        {
            return Force().As<T>();
        }


        protected internal override bool Is<T>(ElaValue value)
        {
            return Force().Is<T>();
        }


        protected internal override int AsInteger(ElaValue value)
        {
            return Force().AsInteger();
        }


        protected internal override float AsSingle(ElaValue value)
        {
            return Force().AsSingle();
        }


        protected internal override char AsChar(ElaValue value)
        {
            return Force().AsChar();
        }


        protected internal override bool AsBoolean(ElaValue value)
        {
            return Force().AsBoolean();
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
		#endregion


		#region Properties
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
		#endregion
	}
}