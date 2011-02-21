using System;
using System.Collections.Generic;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public class ElaVariant : ElaObject
	{
		#region Construction
		public ElaVariant(string tag, ElaValue value) : base(ObjectType.Variant, value.Ref.Traits | ElaTraits.Tag)
		{
			Tag = tag;
			Value = value;
		}
		#endregion


		#region Traits
		protected internal override string GetTag(ExecutionContext ctx)
		{
			return Tag;
		}
		
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Equals(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.NotEquals(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Greater(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Lesser(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.GreaterEquals(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.LesserEquals(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue GetLength(ExecutionContext ctx)
		{
			return Value.Ref.GetLength(ctx);
		}


		protected internal override ElaValue Successor(ExecutionContext ctx)
		{
			return Value.Ref.Successor(ctx);
		}


		protected internal override ElaValue Predecessor(ExecutionContext ctx)
		{
			return Value.Ref.Predecessor(ctx);
		}


		protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			return Value.Ref.GetValue(index, ctx);
		}


		protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
		{
			Value.Ref.SetValue(index, value, ctx);
		}


		protected internal override ElaValue GetMax(ExecutionContext ctx)
		{
			return Value.Ref.GetLength(ctx);
		}


		protected internal override ElaValue GetMin(ExecutionContext ctx)
		{
			return Value.Ref.GetMin(ctx);
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Concatenate(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Add(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Subtract(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Multiply(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Divide(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Modulus(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.Power(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.BitwiseOr(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.BitwiseAnd(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.BitwiseXor(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue BitwiseNot(ExecutionContext ctx)
		{
			return Value.Ref.BitwiseNot(ctx);
		}


		protected internal override ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.ShiftRight(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return Value.Ref.ShiftLeft(Self(left), Self(right), ctx);
		}


		protected internal override ElaValue Negate(ExecutionContext ctx)
		{
			return Value.Ref.Negate(ctx);
		}


		protected internal override bool Bool(ExecutionContext ctx)
		{
			return Value.Ref.Bool(ctx);
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


		protected internal override string Show(ExecutionContext ctx, ShowInfo info)
		{
			return "`" + Tag + (Value.Ref != ElaUnit.Instance ? " " + Value.Ref.Show(Value, ctx, info) : String.Empty);
		}


		protected internal override ElaValue Convert(ObjectType type, ExecutionContext ctx)
		{
			if (type == ObjectType.Variant)
				return new ElaValue(this);
			else
				return Value.Ref.Convert(type, ctx);
		}


		public override ElaValue Call(ElaValue value, ExecutionContext ctx)
		{
			return Value.Ref.Call(value, ctx);
		}


		protected internal override ElaValue Force(ExecutionContext ctx)
		{
			return Value.Ref.Force(ctx);
		}


		private ElaValue Self(ElaValue val)
		{
			return val.Ref == this ? Value : val;
		}
		#endregion


		#region Singleton Traits
		internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return Value.Ref.Successor(Value, ctx);
		}


		internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return Value.Ref.Predecessor(Value, ctx);
		}


		internal override ElaValue BitwiseNot(ElaValue @this, ExecutionContext ctx)
		{
			return Value.Ref.BitwiseNot(Value, ctx);
		}


		internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return Value.Ref.Negate(Value, ctx);
		}


		internal override bool Bool(ElaValue @this, ExecutionContext ctx)
		{
			return Value.Ref.Bool(Value, ctx);
		}


		internal override string Show(ElaValue @this, ExecutionContext ctx, ShowInfo info)
		{
			return Show(ctx, info);
		}


		internal override ElaValue Convert(ElaValue @this, ObjectType type, ExecutionContext ctx)
		{
			return Value.Ref.Convert(Value, type, ctx);
		}
		#endregion


		#region Properties
		public ElaValue Value { get; protected set; }

		public string Tag { get; protected set; }
		#endregion
	}
}