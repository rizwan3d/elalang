using System;
using Ela.CodeModel;
using Ela.Compilation;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLazy : ElaObject
	{
		#region Construction
		private const int SEVAL = -1000;
		private CodeFrame curMod;

		internal ElaLazy(ElaFunction function, CodeFrame curMod) : base(ElaTypeCode.Lazy)
		{
			Function = function;
			_value = default(ElaValue);
			this.curMod = curMod;
		}
		#endregion


		#region Methods
		public override int GetHashCode()
        {
            return Value.Ref != null ? Value.GetHashCode() : 0;
        }


		public string AsString()
		{
			return Force().AsString();
		}


		public bool AsBoolean()
		{
			return Force().AsBoolean();
		}


		public char AsChar()
		{
			return Force().AsChar();
		}


		public double AsDouble()
		{
			return Force().AsDouble();
		}


		public float AsSingle()
		{
			return Force().AsSingle();
		}


		public int AsInteger()
		{
			return Force().AsInteger();
		}


		public long AsLong()
		{
			return Force().AsLong();
		}


		public ElaList AsList()
		{
			return Force().AsList();
		}


		public ElaTuple AsTuple()
		{
			return Force().AsTuple();
		}


		public ElaRecord AsRecord()
		{
			return Force().AsRecord();
		}


		public ElaFunction AsFunction()
		{
			return Force().AsFunction();
		}


		public object AsObject()
		{
			switch (Force().TypeCode)
			{
				case ElaTypeCode.Boolean: return Value.AsBoolean();
				case ElaTypeCode.Char: return Value.AsChar();
				case ElaTypeCode.Double: return Value.AsDouble();
				case ElaTypeCode.Function: return Value.AsFunction();
				case ElaTypeCode.Integer: return Value.AsInteger();
				case ElaTypeCode.List: return Value.AsList();
				case ElaTypeCode.Long: return Value.AsLong();
				case ElaTypeCode.Record: return Value.AsRecord();
				case ElaTypeCode.Single: return Value.AsSingle();
				case ElaTypeCode.String: return Value.AsString();
				case ElaTypeCode.Tuple: return Value.AsTuple();
				case ElaTypeCode.Unit: return null;
				case ElaTypeCode.Lazy: return Value.AsObject();
				default:
					if (Value.Ref != null)
						return Value.Ref;
					else
						throw new NotSupportedException();
			}
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
			return Force(ctx);
		}


		internal ElaValue Force(ExecutionContext ctx)
		{
			if (Value.Ref == null)
			{
				Value = Function.Call(
					Function.LastParameter.Ref != null ? Function.LastParameter :
					new ElaValue(ElaUnit.Instance), ctx);
			}

			return Value;
		}
		#endregion


		#region Operations
		protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Equal(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).NotEqual(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Greater(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).Lesser(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).GreaterEqual(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.Force(ctx).LesserEqual(left.Force(ctx), right.Force(ctx), ctx);
		}


		protected internal override ElaValue GetLength(ExecutionContext ctx)
		{
			return Force(ctx).Ref.GetLength(ctx);
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return Force(ctx).Ref.Successor(Value, ctx);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return Force(ctx).Ref.Predecessor(Value, ctx);
		}


		protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			return Force(ctx).Ref.GetValue(index.Force(ctx), ctx);
		}


		protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
		{
			Force(ctx).SetValue(index.Force(ctx), value, ctx);
		}


		protected internal override ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
		{
			return Force(ctx).Ref.GetMax(Value, ctx);
		}


		protected internal override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
		{
			return Force(ctx).Ref.GetMin(Value, ctx);
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Ref is ElaLazyList)
				return ((ElaLazyList)left.Ref).Concatenate(left, right, ctx);
			else if (right.Ref is ElaLazyList)
				return ((ElaLazyList)right.Ref).Concatenate(left, right, ctx);
			else if (left.Ref == this)
				return Force(ctx).Ref.Concatenate(left, right, ctx);
			else if (right.Ref == this && left.Ref is ElaList)
			{
				var xs = (ElaList)left.Ref;
				var c = 0;
				var newLst = default(ElaLazyList);

				foreach (var e in xs.Reverse())
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
			{
				ctx.Fail("Unable to concatenate two entities.");
				return Default();
			}
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


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
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


		protected internal override ElaValue Clone(ExecutionContext ctx)
		{
			return Force(ctx).Ref.Clone(ctx);
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