using System;
using Ela.CodeModel;
using Ela.Compilation;
using System.Threading;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLazy : ElaObject
	{
		#region Construction
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.Lazy), (Int32)ElaTypeCode.Lazy, true, typeof(ElaLazy));

		private const int SEVAL = -1000;

		internal ElaLazy(ElaFunction function) : base(ElaTypeCode.Lazy)
		{
			Function = function;
			_value = default(ElaValue);
		}
		#endregion


		#region Methods
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
                return new ElaValue(ElaDummyObject.Instance);
			}

			return Value;
		}
		#endregion


		#region Operations
        protected internal override bool True(ElaValue @this, ExecutionContext ctx)
        {
            return Force(ctx).Ref.True(Value, ctx);
        }

        protected internal override bool False(ElaValue @this, ExecutionContext ctx)
        {
            return Force(ctx).Ref.False(Value, ctx);
        }

        protected internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Equal(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).NotEqual(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Greater(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Lesser(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).GreaterEqual(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).LesserEqual(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue GetLength(ExecutionContext ctx)
		{
			return Force(ctx).Ref.GetLength(ctx);
		}


		protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			return Force(ctx).Ref.GetValue(index.Force(ctx), ctx);
		}


        protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        {
            return Force(ctx).Ref.Successor(Value, ctx);
        }


        protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        {
            return Force(ctx).Ref.Predecessor(Value, ctx);
        }

		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (right.Ref == this && left.Ref is ElaList)
			{
				var xs = (ElaList)left.Ref;
				var c = 0;
				var newLst = default(ElaLazyList);
                var rev = xs.Reverse(ctx);

                if (ctx.Failed)
                    return Default();
                
				foreach (var e in rev)
				{
					if (c == 0)
						newLst = new ElaLazyList(this, e);
					else
						newLst = new ElaLazyList(newLst, e);

					c++;
				}

				return new ElaValue(newLst);
			}
			else
				return left.Force(ctx).Concatenate(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Add(left.Force(ctx), right.Force(ctx), ctx);		
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Subtract(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Multiply(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Divide(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Remainder(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Power(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).BitwiseOr(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).BitwiseAnd(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).BitwiseXor(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
		{
			return Force(ctx).Ref.BitwiseNot(Value, ctx);
		}


		protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).ShiftRight(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).ShiftLeft(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return Force(ctx).Ref.Negate(Value, ctx);
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


		protected internal override bool Has(string field, ExecutionContext ctx)
		{
			return Force(ctx).Ref.Has(field, ctx);
		}


		protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			if (Function == null)
				return Value.Ref.Show(Value, info, ctx);
			else
				return "<thunk>";
		}


        protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
			return Force(ctx).Ref.Convert(Value, type, ctx);
		}


		protected internal override ElaValue Call(ElaValue arg, ExecutionContext ctx)
		{
			return Force(ctx).Ref.Call(arg, ctx);
		}


		protected internal override string GetTag(ExecutionContext ctx)
		{
			return Force(ctx).Ref.GetTag(ctx);
		}


		protected internal override ElaValue Untag(ExecutionContext ctx)
		{
			return Force(ctx).Ref.Untag(ctx);
		}


        protected internal override bool IsNil(ExecutionContext ctx)
        {
            return Force(ctx).Ref.IsNil(ctx);
        }


        protected internal override ElaValue Head(ExecutionContext ctx)
        {
            return Force(ctx).Ref.Head(ctx);
        }


        protected internal override ElaValue Tail(ExecutionContext ctx)
        {
            return Force(ctx).Ref.Tail(ctx);
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