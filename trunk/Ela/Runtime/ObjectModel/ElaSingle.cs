using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaSingle : ElaObject
	{
		#region Construction
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.Single), (Int32)ElaTypeCode.Single, false, typeof(ElaSingle));
		internal static readonly ElaSingle Instance = new ElaSingle();
		
		private ElaSingle() : base(ElaTypeCode.Single)
		{

		}
		#endregion


		#region Methods
        protected internal override float AsSingle(ElaValue value)
        {
            return value.DirectGetReal();
        }


        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Single ? @this.DirectGetReal().CompareTo(other.DirectGetReal()) :
				other.TypeCode == ElaTypeCode.Integer ? @this.DirectGetReal().CompareTo((Single)other.I4) :
                other.TypeCode == ElaTypeCode.Long ? @this.DirectGetReal().CompareTo((Single)((ElaLong)other.Ref).Value) :
				other.TypeCode == ElaTypeCode.Double ? ((Double)@this.DirectGetReal()).CompareTo(other.GetDouble()) :
				-1;
		}
		#endregion


		#region Operations
		protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() == right.GetReal()) :
					right.Ref.Equal(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "equal");
			return Default();
		}


		protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() != right.GetReal()) :
					right.Ref.NotEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "notequal");
			return Default();
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() > right.GetReal()) :
					right.Ref.Greater(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "greater");
			return Default();
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() < right.GetReal()) :
					right.Ref.Lesser(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "lesser");
			return Default();
		}


		protected internal override ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() >= right.GetReal()) :
					right.Ref.GreaterEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "greaterequal");
			return Default();
		}


		protected internal override ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() <= right.GetReal()) :
					right.Ref.LesserEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "lesserequal");
			return Default();
		}


		protected internal override ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(float.MaxValue);
		}


		protected internal override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(float.MinValue);
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.DirectGetReal() + 1);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(@this.DirectGetReal() - 1);
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? @this.DirectGetReal().ToString(info.Format, Culture.NumberFormat) :
					@this.DirectGetReal().ToString(Culture.NumberFormat);
			}
			catch (FormatException)
			{
				if (ctx == ElaObject.DummyContext)
					throw;

				ctx.InvalidFormat(info.Format, @this);
				return String.Empty;
			}
		}


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
            switch (type.ReflectedTypeCode)
			{
				case ElaTypeCode.Integer: return new ElaValue((Int32)@this.DirectGetReal());
				case ElaTypeCode.Single: return @this;
				case ElaTypeCode.Double: return new ElaValue((Double)@this.DirectGetReal());
				case ElaTypeCode.Long: return new ElaValue((Int64)@this.DirectGetReal());
				case ElaTypeCode.Char: return new ElaValue((Char)@this.DirectGetReal());
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
                    ctx.ConversionFailed(@this, type.ReflectedTypeName);
					return Default();
			}
		}


		protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(-@this.DirectGetReal());
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() + right.GetReal()) :
					right.Ref.Add(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "add");
			return Default();
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() - right.GetReal()) :
					right.Ref.Subtract(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "subtract");
			return Default();
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() * right.GetReal()) :
					right.Ref.Multiply(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "multiply");
			return Default();
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() / right.GetReal()) :	
					right.Ref.Divide(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "divide");
			return Default();
		}


		protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue(left.GetReal() % right.GetReal()) :	
					right.Ref.Remainder(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "remainder");
			return Default();
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.REA)
				return right.TypeId <= ElaMachine.REA ? new ElaValue((Single)Math.Pow(left.GetReal(), right.GetReal())) :
					right.Ref.Power(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "power");
			return Default();
		}
		#endregion
	}
}