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
            return "Lazy#";
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


		protected internal override ElaValue Force(ElaValue @this, ExecutionContext ctx)
		{
			if (ctx == ElaObject.DummyContext)
				return Force();

			return Force(ctx);
		}


		internal override ElaValue InternalForce(ElaValue @this, ExecutionContext ctx)
		{
			if (Value.Ref == null)
			{
				Value = Function.Call(
					Function.LastParameter.Ref != null ? Function.LastParameter :
					new ElaValue(ElaUnit.Instance), ctx);
			}

			return Value;
		}


		internal ElaValue Force(ExecutionContext ctx)
		{
			//if (Value.Ref == null)
			//{
			//    Value = Function.Call(
			//        Function.LastParameter.Ref != null ? Function.LastParameter :
			//        new ElaValue(ElaUnit.Instance), ctx);
			//}

			//return Value;
			var f = Function;

			if (f != null)
			{
				ctx.Failed = true;
				ctx.Thunk = this;
				return new ElaValue(ElaDummyObject.Instance);
			}

			return Value;
		}
		#endregion


		#region Operations
		protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			return Force(ctx).Ref.GetValue(index.Force(ctx), ctx);
		}


		protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
		{
			Force(ctx).SetValue(index.Force(ctx), value, ctx);
		}

		protected internal override bool Bool(ElaValue @this, ExecutionContext ctx)
		{
			return Force(ctx).Ref.Bool(Value, ctx);
		}
		
		
		protected internal override ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
		{
			return new ElaValue(new ElaLazyList(this, value));
		}


		protected internal override ElaValue Nil(ExecutionContext ctx)
		{
			return new ElaValue(ElaLazyList.Empty);
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			return new ElaValue(new ElaLazyList(this, value));
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return new ElaValue(this);
		}


		protected internal override ElaValue GetField(string field, ExecutionContext ctx)
		{
			return Force(ctx).Ref.GetField(field, ctx);
		}


		protected internal override void SetField(string field, ElaValue value, ExecutionContext ctx)
		{
			Force(ctx).Ref.SetField(field, value, ctx);
		}


		protected internal override bool HasField(string field, ExecutionContext ctx)
		{
			return Force(ctx).Ref.HasField(field, ctx);
		}


		protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			if (Function == null)
				return Value.Ref.Show(@this, info, ctx);
			else
				return "<thunk>";
		}


		protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
		{
			return Force(ctx).Ref.Call(arg, ctx);
		}


		protected internal override ElaValue Untag(ElaValue arg, ExecutionContext ctx)
		{
			return Force(ctx).Ref.Untag(arg, ctx);
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