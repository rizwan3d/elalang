using System;
using Ela.Runtime.ObjectModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace Ela.Runtime
{
	public struct ElaValue : IComparable<ElaValue>, IEquatable<ElaValue>
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


		public override string ToString()
		{
			return Ref.Show(this, ElaObject.DummyContext, ShowInfo.Default);
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
            return obj is ElaValue ? Equals(this, (ElaValue)obj, ElaObject.DummyContext).AsBoolean() : false;
        }


        public bool Equals(ElaValue other)
        {
            return Equals(this, other, ElaObject.DummyContext).AsBoolean();
        }


		public ElaValue Id(ExecutionContext ctx)
		{
			return (Ref.Traits & ElaTraits.Thunk) == ElaTraits.Thunk ? Ref.Force(ctx) : this;
		}


		public string GetTypeName()
		{
			return Ref.GetTypeName();
		}


		internal float GetReal()
		{
			if (TypeCode == ElaTypeCode.Integer)
				return (Single)I4;
			else if (TypeCode == ElaTypeCode.Long)
				return (Single)((ElaLong)Ref).InternalValue;

			return DirectGetReal();
		}


		internal float DirectGetReal()
		{
			var conv = new Conv();
			conv.I4_1 = I4;
			return conv.R4;
		}


		internal double GetDouble()
		{
			return TypeCode == ElaTypeCode.Double ? ((ElaDouble)Ref).InternalValue :
				TypeCode == ElaTypeCode.Single ? DirectGetReal() :
				TypeCode == ElaTypeCode.Long ? ((ElaLong)Ref).InternalValue :
				(Double)I4;
		}


		internal long GetLong()
		{
			return TypeCode == ElaTypeCode.Long ? ((ElaLong)Ref).InternalValue : I4;				
		}


        public bool Is<T>() where T : ElaObject
        {
            return Ref is T;
        }


        public T As<T>() where T : ElaObject
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
			var type = default(ElaTypeCode);

            if (ti == typeof(Int32))
                type = ElaTypeCode.Integer;
            else if (ti == typeof(Single))
                type = ElaTypeCode.Single;
            else if (ti == typeof(Int64))
                type = ElaTypeCode.Long;
            else if (ti == typeof(Double))
                type = ElaTypeCode.Double;
            else if (ti == typeof(Boolean))
                type = ElaTypeCode.Boolean;
            else if (ti == typeof(String))
                type = ElaTypeCode.String;
            else if (ti == typeof(Char))
                type = ElaTypeCode.Char;
            else if (ti == typeof(ElaList))
                type = ElaTypeCode.List;
            else if (ti == typeof(ElaRecord))
                type = ElaTypeCode.Record;
            else if (ti == typeof(ElaTuple))
                type = ElaTypeCode.Tuple;
            else if (ti == typeof(ElaVariant))
                type = ElaTypeCode.Variant;
            else if (ti == typeof(ElaFunction))
                type = ElaTypeCode.Function;
            else if (ti == typeof(ElaModule))
                type = ElaTypeCode.Module;
            else if (ti == typeof(ElaValue))
                return this;
            else
            {
                if (ti == typeof(ElaUnit))
                {
                    if (TypeCode == ElaTypeCode.Unit)
                        return ElaUnit.Instance;
                    else
                        throw InvalidCast(TypeCode, type);
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

            if (type == TypeCode)
                return AsObject();

			var ret = Ref.Convert(this, type, ctx).AsObject();

			if (ctx.Failed)
				throw InvalidCast(TypeCode, type);

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
				case ElaTypeCode.Double: return AsDouble();
				case ElaTypeCode.Function: return AsFunction();
				case ElaTypeCode.Integer: return AsInteger();
				case ElaTypeCode.List: return AsList();
				case ElaTypeCode.Long: return AsLong();
				case ElaTypeCode.Record: return AsRecord();
				case ElaTypeCode.Single: return AsSingle();
				case ElaTypeCode.String: return AsString();
				case ElaTypeCode.Tuple: return AsTuple();
				case ElaTypeCode.Unit: return null;
				case ElaTypeCode.Lazy: return ((ElaLazy)Ref).AsObject();
				default:
					if (Ref == null)
						throw new NotSupportedException();
					else
						return Ref;
			}
		}


		public string AsString()
		{
			if (TypeCode == ElaTypeCode.String)
				return ((ElaString)Ref).GetValue();
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsString();
			else
				throw InvalidCast(ElaTypeCode.String, TypeCode);
		}


		public bool AsBoolean()
		{
			if ((Traits & ElaTraits.Bool) == ElaTraits.Bool)
                return Bool(ElaObject.DummyContext);
            else if (TypeCode == ElaTypeCode.Lazy)
                return ((ElaLazy)Ref).AsBoolean();
            else
                throw InvalidCast(ElaTypeCode.Boolean, TypeCode);
		}


		public char AsChar()
		{
			if (TypeCode == ElaTypeCode.Char)
				return (Char)I4;
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsChar();
			else
				throw InvalidCast(ElaTypeCode.Char, TypeCode);
		}


		public double AsDouble()
		{
			if (TypeCode == ElaTypeCode.Double)
				return ((ElaDouble)Ref).InternalValue;
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsDouble();
			else
				throw InvalidCast(ElaTypeCode.Double, TypeCode);
		}


		public float AsSingle()
		{
			if (TypeCode == ElaTypeCode.Single)
				return GetReal();
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsSingle();
			else
				throw InvalidCast(ElaTypeCode.Single, TypeCode);
		}


		public int AsInteger()
		{
			if (TypeCode == ElaTypeCode.Integer)
				return I4;
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsInteger();
			else
				throw InvalidCast(ElaTypeCode.Integer, TypeCode);
		}


		public long AsLong()
		{
			if (TypeCode == ElaTypeCode.Long)
				return ((ElaLong)Ref).InternalValue;
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsLong();
			else
				throw InvalidCast(ElaTypeCode.Long, TypeCode);
		}


		public ElaList AsList()
		{
			if (TypeCode == ElaTypeCode.List)
				return (ElaList)Ref;
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsList();
			else
				throw InvalidCast(ElaTypeCode.List, TypeCode);
		}


		public ElaTuple AsTuple()
		{
			if (TypeCode == ElaTypeCode.Tuple)
				return (ElaTuple)Ref;
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsTuple();
			else
				throw InvalidCast(ElaTypeCode.Tuple, TypeCode);
		}


		public ElaRecord AsRecord()
		{
			if (TypeCode == ElaTypeCode.Record)
				return (ElaRecord)Ref;
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsRecord();
			else
				throw InvalidCast(ElaTypeCode.Record, TypeCode);
		}


		public ElaFunction AsFunction()
		{
			if (TypeCode == ElaTypeCode.Function)
				return (ElaFunction)Ref;
			else if (TypeCode == ElaTypeCode.Lazy)
				return ((ElaLazy)Ref).AsFunction();
			else
				throw InvalidCast(ElaTypeCode.Function, TypeCode);
		}


		public ElaUnit AsUnit()
		{
			if (TypeCode == ElaTypeCode.Unit)
				return (ElaUnit)Ref;
			else
				throw InvalidCast(ElaTypeCode.Unit, TypeCode);
		}


		private InvalidCastException InvalidCast(ElaTypeCode target, ElaTypeCode source)
		{
            return new InvalidCastException(Strings.GetMessage("InvalidCast", TypeCodeFormat.GetShortForm(source), 
                TypeCodeFormat.GetShortForm(target)));
		}
		#endregion


        #region Traits
        public ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Equals(left, right, ctx);
        }


        public ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.NotEquals(left, right, ctx);
        }


        public ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Greater(left, right, ctx);
        }


        public ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Lesser(left, right, ctx);
        }


        public ElaValue GreaterEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.GreaterEquals(left, right, ctx);
        }


        public ElaValue LesserEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.LesserEquals(left, right, ctx);
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


        public void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
        {
            Ref.SetValue(index, value, ctx);
        }


        public ElaValue GetMax(ExecutionContext ctx)
        {
            return Ref.GetMax(ctx);
        }


        public ElaValue GetMin(ExecutionContext ctx)
        {
            return Ref.GetMin(ctx);
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


        public ElaValue Modulus(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Ref.Modulus(left, right, ctx);
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


        public bool Bool(ExecutionContext ctx)
        {
            return Ref.Bool(this, ctx);
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


        public ElaValue GetField(string field, ExecutionContext ctx)
        {
            return Ref.GetField(field, ctx);
        }


        public void SetField(string field, ElaValue value, ExecutionContext ctx)
        {
            Ref.SetField(field, value, ctx);
        }


        public bool HasField(string field, ExecutionContext ctx)
        {
            return Ref.HasField(field, ctx);
        }


        public string Show(ExecutionContext ctx, ShowInfo info)
        {
            return Ref.Show(this, ctx, info);
        }


        public ElaValue Convert(ElaTypeCode type, ExecutionContext ctx)
        {
            return Ref.Convert(type, ctx);
        }


        public ElaValue Call(ElaValue value, ExecutionContext ctx)
        {
            return Ref.Call(value, ctx);
        }


        public ElaValue Force(ExecutionContext ctx)
        {
            return Ref.Force(ctx);
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


        public ElaTraits Traits
        {
            get { return Ref.Traits; }
        }
		#endregion
	}
}