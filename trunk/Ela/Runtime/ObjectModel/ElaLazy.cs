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
        internal override ElaValue Convert(ElaValue @this, ElaTypeCode type)
        {
            return Force().Convert(type);
        }
        
        
        internal override string GetTag()
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


		#region Operations
		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			return new ElaValue(new ElaLazyList(this, value));
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return new ElaValue(this);
		}


		protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			if (Function == null)
				return Value.Ref.Show(@this, info, ctx);
			else
				return "<thunk>";
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