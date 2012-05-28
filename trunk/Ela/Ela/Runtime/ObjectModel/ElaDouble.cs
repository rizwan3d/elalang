using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaDouble : ElaObject
	{
		#region Construction
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.Double), (Int32)ElaTypeCode.Double, true, typeof(ElaDouble));
        
		public ElaDouble(double value) : base(ElaTypeCode.Double)
		{
			Value = value;
		}
		#endregion
		  

        #region Methods
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.Single ? Value.CompareTo(other.DirectGetReal()) :
				other.TypeCode == ElaTypeCode.Integer ? Value.CompareTo((Double)other.I4) :
                other.TypeCode == ElaTypeCode.Long ? Value.CompareTo((Double)((ElaLong)other.Ref).Value) :
				other.TypeCode == ElaTypeCode.Double ? Value.CompareTo(((ElaDouble)other.Ref).Value) :
				-1;
		}
        #endregion


        #region Operations
        protected internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return left.GetDouble() == right.GetDouble();
				else
					return right.Ref.Equal(left, right, ctx);
			}
			
			return false;
		}


		protected internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return left.GetDouble() != right.GetDouble();
				else
					return right.Ref.NotEqual(left, right, ctx);
			}
			
			return true;
		}


		protected internal override bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return left.GetDouble() > right.GetDouble();
				else
					return right.Ref.Greater(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "greater");
            return false;
		}


		protected internal override bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return left.GetDouble() < right.GetDouble();
				else
					return right.Ref.Lesser(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "lesser");
            return false;
		}


		protected internal override bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return left.GetDouble() >= right.GetDouble();
				else
					return right.Ref.GreaterEqual(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "greaterequal");
            return false;
		}


		protected internal override bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return left.GetDouble() >= right.GetDouble();
				else
					return right.Ref.LesserEqual(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "lesserequal");
            return false;
		}


		protected internal override ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Double.MaxValue);
		}


		protected internal override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Double.MinValue);
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Value + 1);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(Value - 1);
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			try
			{
				return !String.IsNullOrEmpty(info.Format) ? Value.ToString(info.Format, Culture.NumberFormat) :
					Value.ToString(Culture.NumberFormat);
			}
			catch (FormatException)
			{
				if (ctx == ElaObject.DummyContext)
					throw;

				ctx.InvalidFormat(info.Format, new ElaValue(this));
				return String.Empty;
			}
		}


        protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
            switch (type.ReflectedTypeCode)
			{
				case ElaTypeCode.Integer: return new ElaValue((Int32)Value);
				case ElaTypeCode.Single: return new ElaValue((float)Value);
				case ElaTypeCode.Double: return @this;
				case ElaTypeCode.Long: return new ElaValue((Int64)Value);
				case ElaTypeCode.Char: return new ElaValue((Char)Value);
				case ElaTypeCode.String: return new ElaValue(Show(@this, ShowInfo.Default, ctx));
				default:
                    ctx.ConversionFailed(@this, type.ReflectedTypeName);
					return Default();
			}
		}


		protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			return new ElaValue(-Value);
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() + right.GetDouble());
				else
					return right.Ref.Add(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "add");
			return Default();
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() - right.GetDouble());
				else
					return right.Ref.Subtract(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "subtract");
			return Default();
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() * right.GetDouble());
				else
					return right.Ref.Multiply(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "multiply");
			return Default();
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() / right.GetDouble());
				else
					return right.Ref.Divide(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "divide");
			return Default();
		}


		protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return new ElaValue(left.GetDouble() % right.GetDouble());
				else
					return right.Ref.Remainder(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "remainder");
			return Default();
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId <= ElaMachine.DBL)
			{
				if (right.TypeId <= ElaMachine.DBL)
					return new ElaValue(Math.Pow(left.GetDouble(), right.GetDouble()));
				else
					return right.Ref.Power(left, right, ctx);
			}
			
			ctx.InvalidLeftOperand(left, right, "power");
			return Default();
		}

		#endregion


		#region Properties
		internal double Value { get; private set; }
		#endregion
	}
}