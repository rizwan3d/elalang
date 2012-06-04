using System;
using Ela.Runtime.ObjectModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace Ela.Runtime
{
	public struct ElaValue : IComparable<ElaValue>, IEquatable<ElaValue>, IFormattable
	{
		#region Construction
		public ElaValue(ElaObject val)
		{
			I4 = 0;
			Ref = val;
		}


		public ElaValue(int val)
		{
			I4 = val;
			Ref = ElaInteger.Instance;
		}


		public ElaValue(char val)
		{
			I4 = (Int32)val;
			Ref = ElaChar.Instance;
		}


		public ElaValue(int val, ElaObject obj)
		{
			I4 = val;
			Ref = obj;
		}


		public ElaValue(float val)
		{
			var conv = new Conv();
			conv.R4 = val;
			I4 = conv.I4_1;
			Ref = ElaSingle.Instance;
		}


		public ElaValue(bool val)
		{
			I4 = val ? 1 : 0;
			Ref = ElaBoolean.Instance;
		}


		public ElaValue(string value)
		{
			I4 = 0;
			Ref = new ElaString(value);
		}


		public ElaValue(long val)
		{
			I4 = 0;
			Ref = new ElaLong(val);
		}


		public ElaValue(double val)
		{
			I4 = 0;
			Ref = new ElaDouble(val);
		}
		#endregion


		#region Methods
		public int CompareTo(ElaValue other)
		{
			return Ref.Compare(this, other);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Ref != null ? Ref.Show(this, new ShowInfo(0, 0, format), ElaObject.DummyContext) : "_|_";
        }


		public override string ToString()
		{
			return Ref != null ? Ref.Show(this, ShowInfo.Default, ElaObject.DummyContext) : "_|_";
		}


        public override int GetHashCode()
        {
            switch (Ref.TypeId)
            {
                case ElaMachine.INT: return I4.GetHashCode();
                case ElaMachine.REA: return DirectGetReal().GetHashCode();
                case ElaMachine.CHR: return ((Char)I4).GetHashCode();
                case ElaMachine.BYT: return (I4 == 1).GetHashCode();
                default: return Ref.GetHashCode();
            }
        }


        public override bool Equals(object obj)
        {
            return obj is ElaValue ? Equal(this, (ElaValue)obj, ElaObject.DummyContext) : false;
        }


        public bool Equals(ElaValue other)
        {
            return Equal(this, other, ElaObject.DummyContext);
        }


		public ElaValue Id(ExecutionContext ctx)
		{
			return Ref.TypeId == ElaMachine.LAZ ? Ref.Force(this, ctx) : this;
		}


		public string GetTypeName()
		{
			return Ref != null ? Ref.GetTypeName() : "<unknown>";
		}


		internal float GetReal()
		{
			if (TypeCode == ElaTypeCode.Integer)
				return (Single)I4;
			else if (TypeCode == ElaTypeCode.Long)
				return (Single)((ElaLong)Ref).Value;

			return DirectGetReal();
		}


		internal float DirectGetReal()
		{
			var conv = new Conv();
			conv.I4_1 = I4;
			return conv.R4;
		}


        internal string DirectGetString()
        {
            return ((ElaString)Ref).GetValue();
        }


		internal double GetDouble()
		{
			return TypeCode == ElaTypeCode.Double ? ((ElaDouble)Ref).Value :
				TypeCode == ElaTypeCode.Single ? DirectGetReal() :
				TypeCode == ElaTypeCode.Long ? ((ElaLong)Ref).Value :
				(Double)I4;
		}


		internal long GetLong()
		{
			return TypeCode == ElaTypeCode.Long ? ((ElaLong)Ref).Value : I4;				
		}


        public bool Is<T>()
        {
			return Ref is T;
        }


        public T As<T>() where T : class
        {
			return Ref as T;
        }


        public bool ReferenceEquals(ElaValue other)
        {
			return Ref == other.Ref;
        }


		public static ElaValue FromObject(object value)
		{
            if (value == null)
                return new ElaValue(ElaUnit.Instance);
            else if (value is ElaObject)
                return new ElaValue((ElaObject)value);
            else if (value is Int32)
                return new ElaValue((Int32)value);
            else if (value is Int64)
                return new ElaValue((Int64)value);
            else if (value is Single)
                return new ElaValue((Single)value);
            else if (value is Double)
                return new ElaValue((Double)value);
            else if (value is Boolean)
                return new ElaValue((Boolean)value);
            else if (value is Char)
                return new ElaValue((Char)value);
            else if (value is String)
                return new ElaValue((String)value);
            else if (value is ElaValue)
                return (ElaValue)value;
            else if (value is IEnumerable)
                return new ElaValue(ElaList.FromEnumerable((IEnumerable)value));
            else if (value is Delegate)
                return new ElaValue(new DynamicDelegateFunction("<f>", (Delegate)value));
            else
                throw new InvalidCastException();
		}


		public T Convert<T>()
		{
			var ti = typeof(T);
			return (T)Convert(ti);
		}


		public object Convert(Type ti)
		{
			var ctx = new ExecutionContext();
			var type = default(ElaTypeInfo);

            if (ti == typeof(Int32))
                type = ElaInteger.TypeInfo;
            else if (ti == typeof(Single))
                type = ElaSingle.TypeInfo;
            else if (ti == typeof(Int64))
                type = ElaLong.TypeInfo;
            else if (ti == typeof(Double))
                type = ElaDouble.TypeInfo;
            else if (ti == typeof(Boolean))
                type = ElaBoolean.TypeInfo;
            else if (ti == typeof(String))
                type = ElaString.TypeInfo;
            else if (ti == typeof(Char))
                type = ElaChar.TypeInfo;
            else if (ti == typeof(ElaList))
                type = ElaList.TypeInfo;
            else if (ti == typeof(ElaRecord))
                type = ElaRecord.TypeInfo;
            else if (ti == typeof(ElaTuple))
                type = ElaTuple.TypeInfo;
            else if (ti == typeof(ElaVariant))
                type = ElaVariant.TypeInfo;
            else if (ti == typeof(ElaFunction))
                type = ElaFunction.TypeInfo;
            else if (ti == typeof(ElaModule))
                type = ElaModule.TypeInfo;
            else if (ti == typeof(ElaValue))
                return this;
            else if (ti == typeof(Object))
                return AsObject();
            else
            {
                if (ti == typeof(ElaUnit))
                {
                    if (TypeCode == ElaTypeCode.Unit)
                        return ElaUnit.Instance;
                    else
                        throw InvalidCast(TypeCode, type.ReflectedTypeCode);
                }
                else if (ti == typeof(ElaObject))
                    return Ref;
                else if (ti.IsArray)
                    return ConvertToArray(ti.GetElementType());
                else if (ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return ConvertToArray(ti.GetGenericArguments()[0]);
                else
                    return System.Convert.ChangeType(Ref, ti);
            }

            if (type.ReflectedTypeCode == TypeCode)
                return AsObject();

			var ret = Ref.Convert(this, type, ctx).AsObject();

			if (ctx.Failed)
				throw InvalidCast(TypeCode, type.ReflectedTypeCode);

			return ret;
		}


		private object ConvertToArray(Type el)
		{
			var seq = (IEnumerable<ElaValue>)Ref;
			var len = Ref.GetLength(ElaObject.DummyContext);
			var arr = Array.CreateInstance(el, len.AsInteger());
			var i = 0;

			foreach (var e in seq)
			{
				var o = e.Convert(el);
				arr.SetValue(o, i++);
			}

			return arr;
		}
		#endregion


		#region Cast Methods
		public object AsObject()
		{
			switch (TypeCode)
			{
				case ElaTypeCode.Boolean: return AsBoolean();
				case ElaTypeCode.Char: return AsChar();
				case ElaTypeCode.Double: return ((ElaDouble)Ref).Value;
				case ElaTypeCode.Function: return (ElaFunction)Ref;
				case ElaTypeCode.Integer: return AsInteger();
				case ElaTypeCode.List: return (ElaList)Ref;
                case ElaTypeCode.Long: return ((ElaLong)Ref).Value;
				case ElaTypeCode.Record: return (ElaRecord)Ref;
				case ElaTypeCode.Single: return DirectGetReal();
				case ElaTypeCode.String: return DirectGetString();
				case ElaTypeCode.Tuple: return (ElaTuple)Ref;
				case ElaTypeCode.Unit: return null;
				case ElaTypeCode.Lazy:
                    var v = Ref.Force(this, ElaObject.DummyContext);
                    return v.AsObject();
				default:
					if (Ref == null)
						throw new InvalidOperationException();
					else
						return Ref;
			}
		}


		public int AsInteger()
		{
			if (TypeCode == ElaTypeCode.Integer)
				return I4;
            else 
                return Ref.AsInteger(this);
		}


        public char AsChar()
        {
            if (TypeCode == ElaTypeCode.Char)
                return (Char)I4;
            else
                return Ref.AsChar(this);
        }


        public float AsSingle()
        {
            if (TypeCode == ElaTypeCode.Single)
                return DirectGetReal();
            else
                return Ref.AsSingle(this);
        }
        

        public string AsString()
        {
            if (TypeCode == ElaTypeCode.String)
                return DirectGetString();
            else
                return ((Char)I4).ToString();
        }

        public bool AsBoolean()
        {
            if (TypeCode == ElaTypeCode.Boolean)
                return I4 == 1;
            else
                return Ref.AsBoolean(this);
        }


		private InvalidCastException InvalidCast(ElaTypeCode target, ElaTypeCode source)
		{
            return new InvalidCastException(Strings.GetMessage("InvalidCast", TypeCodeFormat.GetShortForm(source), 
                TypeCodeFormat.GetShortForm(target)));
		}
		#endregion


        #region Operations
        public bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Equal(left, right, ctx);
        }


        public bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.NotEqual(left, right, ctx);
        }


        public bool Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Greater(left, right, ctx);
        }


        public bool Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Lesser(left, right, ctx);
        }


        public bool GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.GreaterEqual(left, right, ctx);
        }


        public bool LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.LesserEqual(left, right, ctx);
        }


        public ElaValue GetLength(ExecutionContext ctx)
        {
            return Ref.GetLength(ctx);
        }


        public ElaValue Successor(ExecutionContext ctx)
        {
            return Ref.Successor(this, ctx);
        }


        public ElaValue Predecessor(ExecutionContext ctx)
        {
            return Ref.Predecessor(this, ctx);
        }


        public ElaValue GetValue(ElaValue index, ExecutionContext ctx)
        {
            return Ref.GetValue(index, ctx);
        }


        public ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Concatenate(left, right, ctx);
        }


        public ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Add(left, right, ctx);
        }


        public ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Subtract(left, right, ctx);
        }


        public ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Multiply(left, right, ctx);
        }


        public ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Divide(left, right, ctx);
        }


        public ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Remainder(left, right, ctx);
        }


        public ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Power(left, right, ctx);
        }


        public ElaValue BitwiseOr(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.BitwiseOr(left, right, ctx);
        }


        public ElaValue BitwiseAnd(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.BitwiseAnd(left, right, ctx);
        }


        public ElaValue BitwiseXor(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.BitwiseXor(left, right, ctx);
        }


        public ElaValue BitwiseNot(ExecutionContext ctx)
        {
            return Ref.BitwiseNot(this, ctx);
        }


        public ElaValue ShiftRight(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.ShiftRight(left, right, ctx);
        }


        public ElaValue ShiftLeft(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.ShiftLeft(left, right, ctx);
        }


        public ElaValue Negate(ExecutionContext ctx)
        {
            return Ref.Negate(this, ctx);
        }


        public ElaValue Head(ExecutionContext ctx)
        {
			return Ref.Head(ctx);
        }


        public ElaValue Tail(ExecutionContext ctx)
        {
			return Ref.Tail(ctx);
        }


        public bool IsNil(ExecutionContext ctx)
        {
			return Ref.IsNil(ctx);
        }


        public ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
        {
			return Ref.Cons(instance, value, ctx);
        }


		public ElaValue Nil(ExecutionContext ctx)
		{
			return Ref.Nil(ctx);
		}


        public ElaValue Generate(ElaValue value, ExecutionContext ctx)
        {
			return Ref.Generate(value, ctx);
        }


        public ElaValue GenerateFinalize(ExecutionContext ctx)
        {
			return Ref.GenerateFinalize(ctx);
        }


        public bool HasField(string field, ExecutionContext ctx)
        {
			return Ref.Has(field, ctx);
        }


        public string Show(ShowInfo info, ExecutionContext ctx)
        {
            return Ref.Show(this, info, ctx);
        }


        public ElaValue Convert(ElaTypeInfo type, ExecutionContext ctx)
        {
            return Ref.Convert(this, type, ctx);
        }


        public ElaValue Call(ElaValue value, ExecutionContext ctx)
        {
            return Ref.Call(value, ctx);
        }


        public ElaValue Force(ExecutionContext ctx)
        {
            return Ref.Force(this, ctx);
        }


        public string GetTag(ExecutionContext ctx)
        {
			return Ref.GetTag(ctx);
        }


        public ElaValue Untag(ExecutionContext ctx)
        {
			return Ref.Untag(ctx);
        }
        #endregion
        

		#region Fields
		internal int I4;

		internal ElaObject Ref;
		#endregion


		#region Properties
		public ElaTypeCode TypeCode
		{
			get { return Ref != null ? (ElaTypeCode)TypeId : ElaTypeCode.None; }
		}


		internal int TypeId
		{
			get { return Ref.TypeId; }
		}
		#endregion
    }
}