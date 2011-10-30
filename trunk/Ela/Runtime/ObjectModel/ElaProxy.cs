using System;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
    public abstract class ElaProxy : ElaObject
    {
        #region Construction
        protected ElaProxy(ElaValue value) : this(ElaTypeCode.Object, value)
        {

        }


        internal ElaProxy(ElaTypeCode type, ElaValue value) : base(type)
        {
            Value = value;
        }
        #endregion


        #region Operations
        protected internal override string GetTag(ExecutionContext ctx)
        {
            return Value.GetTag(ctx);
        }


        protected internal override ElaValue Untag(ExecutionContext ctx)
        {
            return Value.Untag(ctx);
        }


        protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Self(left).Ref.Equal(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.NotEqual(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.Greater(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.Lesser(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.GreaterEqual(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.LesserEqual(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue GetLength(ExecutionContext ctx)
        {
            return Value.Ref.GetLength(ctx);
        }


        protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
        {
            return Value.Ref.Successor(Value, ctx);
        }


        protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
        {
            return Value.Ref.Predecessor(Value, ctx);
        }


        protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
        {
            return Value.Ref.GetValue(index, ctx);
        }


        protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
        {
            Value.Ref.SetValue(index, value, ctx);
        }


        protected internal override ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
        {
            return Value.Ref.GetMax(Value, ctx);
        }


		protected internal override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
        {
            return Value.Ref.GetMin(Value, ctx);
        }


        protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.Concatenate(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.Add(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.Subtract(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.Multiply(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.Divide(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.Remainder(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.Power(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.BitwiseOr(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.BitwiseAnd(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.BitwiseXor(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
        {
            return Value.Ref.BitwiseNot(Value, ctx);
        }


        protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.ShiftRight(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
			return Self(left).Ref.ShiftLeft(Self(left), Self(right), ctx);
        }


        protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
        {
            return Value.Ref.Negate(Value, ctx);
        }


        protected internal override bool Bool(ElaValue @this, ExecutionContext ctx)
        {
            return Value.Ref.Bool(Value, ctx);
        }


        protected internal override ElaValue Head(ExecutionContext ctx)
        {
            return Value.Ref.Head(ctx);
        }


        protected internal override ElaValue Tail(ExecutionContext ctx)
        {
            return Value.Ref.Tail(ctx);
        }


        protected internal override bool IsNil(ExecutionContext ctx)
        {
            return Value.Ref.IsNil(ctx);
        }


        protected internal override ElaValue Cons(ElaObject next, ElaValue value, ExecutionContext ctx)
        {
            return Value.Ref.Cons(next, value, ctx);
        }


        protected internal override ElaValue Nil(ExecutionContext ctx)
        {
            return Value.Ref.Nil(ctx);
        }


        protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
        {
            return Value.Ref.Generate(value, ctx);
        }


        protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
        {
            return Value.Ref.GenerateFinalize(ctx);
        }


        protected internal override ElaValue GetField(string field, ExecutionContext ctx)
        {
            return Value.Ref.GetField(field, ctx);
        }


        protected internal override void SetField(string field, ElaValue value, ExecutionContext ctx)
        {
            Value.Ref.SetField(field, value, ctx);
        }


        protected internal override bool HasField(string field, ExecutionContext ctx)
        {
            return Value.Ref.HasField(field, ctx);
        }


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
        {
            return Value.Show(info, ctx);
        }


        protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
        {
            return Value.Ref.Convert(Value, type, ctx);
        }


        protected internal override ElaValue Call(ElaValue value, ExecutionContext ctx)
        {
            return Value.Ref.Call(value, ctx);
        }


        protected internal override ElaValue Force(ElaValue @this, ExecutionContext ctx)
        {
            return Value.Ref.Force(Value, ctx);
        }


		protected internal override ElaValue Clone(ExecutionContext ctx)
		{
			return base.Clone(ctx);
		}


        private ElaValue Self(ElaValue val)
        {
            return val.Ref == this ? Value : val;
        }
        #endregion


        #region Methods
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


        protected internal override int Compare(ElaValue @this, ElaValue other)
        {
            return Value.CompareTo(other);
        }
        #endregion


        #region Properties
        public ElaValue Value { get; protected set; }
        #endregion
    }
}