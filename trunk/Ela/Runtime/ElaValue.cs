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
			if (Ref == null) 
				return String.Empty;

			switch (TypeId)
			{
				case ElaMachine.INT: return I4.ToString();
				case ElaMachine.REA: return DirectGetReal().ToString();
				case ElaMachine.BYT: return (I4 == 1).ToString();
				case ElaMachine.CHR: return ((Char)I4).ToString();
				default: return Ref.ToString();
			}
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
			return obj is ElaValue ? Equals((ElaValue)obj) : false;
        }


        public bool Equals(ElaValue other)
        {
            switch (TypeCode)
            {
                case ElaTypeCode.Boolean:
                    if (other.TypeCode == ElaTypeCode.Boolean)
                        return I4 == other.I4;
                    else
                        return false;
                case ElaTypeCode.Integer:
                    if (other.TypeCode == ElaTypeCode.Integer)
                        return I4 == other.I4;
                    else
                        return false;
                case ElaTypeCode.Char:
                    if (other.TypeCode == ElaTypeCode.Char)
                        return I4 == other.I4;
                    else
                        return false;
                case ElaTypeCode.Single:
                    if (other.TypeCode == ElaTypeCode.Single)
                        return DirectGetReal() == other.DirectGetReal();
                    else
                        return false;
                default:
                    return Ref.Equals(other);
                    break;
            }
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
            else if (value is ElaTypeInfo)
                return new ElaValue(((ElaTypeInfo)value).ToRecord());
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

			var ret = Ref.Convert(this, type).AsObject();
			return ret;
		}


		private object ConvertToArray(Type el)
		{
			var seq = (IEnumerable<ElaValue>)Ref;
			var len = Ref.GetLength(ElaObject.DummyContext);
			var arr = Array.CreateInstance(el, len);
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
					return Ref.IsEvaluated() ? ((ElaLazy)Ref).Value.AsObject() : Ref;
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
        public ElaValue Convert(ElaTypeCode type)
        {
            return Ref.Convert(this, type);
        }


        public string GetTag()
        {
            return Ref.GetTag();
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