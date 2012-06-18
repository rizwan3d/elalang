using System;
using System.Collections;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
	public struct ElaValue : IFormattable
	{
		#region Construction
        internal ElaValue(int val, ElaObject obj)
        {
            I4 = val;
            Ref = obj;
        }
        
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

		#region Casting
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
            return ((ElaString)Ref).Value;
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

		public override string ToString()
		{
            return ToString(String.Empty, Culture.NumberFormat);   
        }
        
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (Ref == null)
                return "_|_";

            if (TypeId == ElaMachine.INT)
                return I4.ToString(format, formatProvider);
            else if (TypeId == ElaMachine.REA)
                return DirectGetReal().ToString(format, Culture.NumberFormat);
            else if (TypeId == ElaMachine.CHR)
                return ((Char)I4).ToString();
            else if (TypeId == ElaMachine.BYT)
                return I4 == 1 ? Boolean.TrueString : Boolean.FalseString;
            else
                return Ref.ToString(format, formatProvider);
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
                
		public string GetTypeName()
		{
			return Ref != null ? Ref.GetTypeName() : "<unknown>";
		}
        
        public bool Is<T>()
        {
			return Ref is T;
        }
        
        public T As<T>() where T : class
        {
			return Ref as T;
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
			if (ti == typeof(Int32) && TypeId == ElaMachine.INT)
                return I4;
            else if (ti == typeof(Single) && TypeId <= ElaMachine.REA)
                return GetReal();
            else if (ti == typeof(Int64) && TypeId <= ElaMachine.LNG)
                return GetLong();
            else if (ti == typeof(Double) && TypeId <= ElaMachine.DBL)
                return GetDouble();
            else if (ti == typeof(Boolean) && TypeId == ElaMachine.BYT)
                return I4 == 1;
            else if (ti == typeof(String) && TypeId == ElaMachine.STR)
                return DirectGetString();
            else if (ti == typeof(Char) && TypeId == ElaMachine.CHR)
                return (Char)I4;
            else if (ti == typeof(ElaList) && TypeId == ElaMachine.LST)
                return (ElaList)Ref;
            else if (ti == typeof(ElaRecord) && TypeId == ElaMachine.REC)
                return (ElaRecord)Ref;
            else if (ti == typeof(ElaTuple) && TypeId == ElaMachine.TUP)
                return (ElaTuple)Ref;
            else if (ti == typeof(ElaVariant) && TypeId == ElaMachine.VAR)
                return (ElaVariant)Ref;
            else if (ti == typeof(ElaFunction) && TypeId == ElaMachine.FUN)
                return (ElaFunction)Ref;
            else if (ti == typeof(ElaModule) && TypeId == ElaMachine.MOD)
                return (ElaModule)Ref;
            else if (ti == typeof(ElaTypeInfo) && TypeId == ElaMachine.TYP)
                return (ElaTypeInfo)Ref;
            else if (ti == typeof(ElaUnit) && TypeId == ElaMachine.UNI)
                return ElaUnit.Instance;
            else if (ti == typeof(ElaObject))
                return Ref;
            else if (ti == typeof(ElaValue))
                return this;
            else if (ti == typeof(Object))
                return AsObject();
            else if (ti.IsArray)
                return ConvertToArray(ti.GetElementType());
            else if (ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return ConvertToArray(ti.GetGenericArguments()[0]);
            else
            {
                try
                {
                    return System.Convert.ChangeType(Ref, ti);
                }
                catch (Exception) { }
            }

            throw InvalidCast(ti);
		}

		private object ConvertToArray(Type el)
		{
			var seq = (IEnumerable<ElaValue>)Ref;
            var len = 0;

            if (Ref is ElaList)
                len = ((ElaList)Ref).Length;
            else if (Ref is ElaTuple)
                len = ((ElaTuple)Ref).Length;

			var arr = Array.CreateInstance(el, len);
			var i = 0;

			foreach (var e in seq)
			{
				var o = e.Convert(el);
				arr.SetValue(o, i++);
			}

			return arr;
		}

		public object AsObject()
		{
			switch (TypeCode)
			{
				case ElaTypeCode.Boolean: return I4 == 1;
				case ElaTypeCode.Char: return (Char)I4;
				case ElaTypeCode.Double: return Ref.AsDouble();
				case ElaTypeCode.Integer: return I4;
				case ElaTypeCode.Long: return Ref.AsLong();
				case ElaTypeCode.Single: return DirectGetReal();
				case ElaTypeCode.String: return DirectGetString();
				case ElaTypeCode.Unit: return null;
				case ElaTypeCode.Lazy:
                    return Ref.Force(this, ElaObject.DummyContext).Ref;
				default:
					if (Ref == null)
						throw new InvalidOperationException();
					else
						return Ref;
			}
		}
        
		public int AsInt32()
		{
			if (TypeCode == ElaTypeCode.Integer)
				return I4;

            throw InvalidCast(typeof(Int32));
		}

        public long AsInt64()
        {
            if (TypeCode == ElaTypeCode.Long)
                return Ref.AsLong();

            throw InvalidCast(typeof(Int64));
        }

        public char AsChar()
        {
            if (TypeCode == ElaTypeCode.Char)
                return (Char)I4;

            throw InvalidCast(typeof(Char));
        }
        
        public float AsSingle()
        {
            if (TypeCode == ElaTypeCode.Single)
                return DirectGetReal();

            throw InvalidCast(typeof(Single));
        }

        public double AsDouble()
        {
            if (TypeCode == ElaTypeCode.Double)
                return Ref.AsDouble();

            throw InvalidCast(typeof(Double));
        }        

        public string AsString()
        {
            if (TypeCode == ElaTypeCode.String)
                return DirectGetString();

            throw InvalidCast(typeof(String));
        }

        public bool AsBoolean()
        {
            if (TypeCode == ElaTypeCode.Boolean)
                return I4 == 1;

            throw InvalidCast(typeof(Boolean));
        }
        
		private InvalidCastException InvalidCast(System.Type target)
		{
            return new InvalidCastException(Strings.GetMessage("InvalidCast", TypeCodeFormat.GetShortForm(TypeCode), 
                target.Name));
		}
		#endregion
        
		#region Properties
		internal int I4;

		internal ElaObject Ref;
		
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