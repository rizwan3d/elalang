using System;
using System.Linq;
using Ela.Runtime.ObjectModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;

namespace Ela.Runtime
{
	public struct ElaValue
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
		public override string ToString()
		{
			return Ref.Show(this, ElaObject.DummyContext, new ShowInfo(0, 40));
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


		internal ElaValue Id(ExecutionContext ctx)
		{
			return (Ref.Traits & ElaTraits.Thunk) == ElaTraits.Thunk ? Ref.Force(ctx) : this;
		}


		internal float GetReal()
		{
			if (DataType == ObjectType.Integer)
				return (Single)I4;
			else if (DataType == ObjectType.Long)
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
			return DataType == ObjectType.Double ? ((ElaDouble)Ref).InternalValue :
				DataType == ObjectType.Single ? DirectGetReal() :
				DataType == ObjectType.Long ? ((ElaLong)Ref).InternalValue :
				(Double)I4;
		}


		internal long GetLong()
		{
			return DataType == ObjectType.Long ? ((ElaLong)Ref).InternalValue : I4;				
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
            else if (value is Array)
            {
                var arr = (Array)value;
                var ret = new ElaArray(arr.Length);

                for (var i = 0; i < arr.Length; i++)
                    ret.Add(ElaValue.FromObject(arr.GetValue(i)));

                return new ElaValue(ret);
            }
            else if (value is IEnumerable)
                return new ElaValue(ElaList.FromEnumerable((IEnumerable)value));
            else
                throw new InvalidCastException();
		}


		public T ChangeType<T>()
		{
			var ti = typeof(T);
			return (T)ChangeType(ti);
		}


		public object ChangeType(Type ti)
		{
			var ctx = new ExecutionContext();
			var type = default(ObjectType);

            if (ti == typeof(Int32))
                type = ObjectType.Integer;
            else if (ti == typeof(Single))
                type = ObjectType.Single;
            else if (ti == typeof(Int64))
                type = ObjectType.Long;
            else if (ti == typeof(Double))
                type = ObjectType.Double;
            else if (ti == typeof(Boolean))
                type = ObjectType.Boolean;
            else if (ti == typeof(String))
                type = ObjectType.String;
            else if (ti == typeof(Char))
                type = ObjectType.Char;
            else if (ti == typeof(ElaArray))
                type = ObjectType.Array;
            else if (ti == typeof(ElaList))
                type = ObjectType.List;
            else if (ti == typeof(ElaRecord))
                type = ObjectType.Record;
            else if (ti == typeof(ElaTuple))
                type = ObjectType.Tuple;
            else if (ti == typeof(ElaVariant))
                type = ObjectType.Variant;
            else if (ti == typeof(ElaFunction))
                type = ObjectType.Function;
            else if (ti == typeof(ElaModule))
                type = ObjectType.Module;
            else if (ti == typeof(ElaValue))
                return this;
            else
            {
                if (ti == typeof(ElaUnit))
                {
                    if (DataType == ObjectType.Unit)
                        return ElaUnit.Instance;
                    else
                        throw InvalidCast(DataType, type);
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

            if (type == DataType)
                return AsObject();

			var ret = Ref.Convert(this, type, ctx).AsObject();

			if (ctx.Failed)
				throw InvalidCast(DataType, type);

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
				var o = e.ChangeType(el);
				arr.SetValue(o, i++);
			}

			return arr;
		}
		#endregion


		#region Cast Methods
		public object AsObject()
		{
			switch (DataType)
			{
				case ObjectType.Array: return AsArray();
				case ObjectType.Boolean: return AsBoolean();
				case ObjectType.Char: return AsChar();
				case ObjectType.Double: return AsDouble();
				case ObjectType.Function: return AsFunction();
				case ObjectType.Integer: return AsInteger();
				case ObjectType.List: return AsList();
				case ObjectType.Long: return AsLong();
				case ObjectType.Record: return AsRecord();
				case ObjectType.Single: return AsSingle();
				case ObjectType.String: return AsString();
				case ObjectType.Tuple: return AsTuple();
				case ObjectType.Unit: return null;
				case ObjectType.Lazy: return ((ElaLazy)Ref).AsObject();
				default:
					if (Ref == null)
						throw new NotSupportedException();
					else
						return Ref;
			}
		}


		public string AsString()
		{
			if (DataType == ObjectType.String)
				return ((ElaString)Ref).GetValue();
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsString();
			else
				throw InvalidCast(ObjectType.String, DataType);
		}


		public bool AsBoolean()
		{
			if (DataType == ObjectType.Boolean)
				return I4 > 0;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsBoolean();
			else
				throw InvalidCast(ObjectType.Boolean, DataType);
		}


		public char AsChar()
		{
			if (DataType == ObjectType.Char)
				return (Char)I4;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsChar();
			else
				throw InvalidCast(ObjectType.Char, DataType);
		}


		public double AsDouble()
		{
			if (DataType == ObjectType.Double)
				return ((ElaDouble)Ref).InternalValue;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsDouble();
			else
				throw InvalidCast(ObjectType.Double, DataType);
		}


		public float AsSingle()
		{
			if (DataType == ObjectType.Single)
				return GetReal();
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsSingle();
			else
				throw InvalidCast(ObjectType.Single, DataType);
		}


		public int AsInteger()
		{
			if (DataType == ObjectType.Integer)
				return I4;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsInteger();
			else
				throw InvalidCast(ObjectType.Integer, DataType);
		}


		public long AsLong()
		{
			if (DataType == ObjectType.Long)
				return ((ElaLong)Ref).InternalValue;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsLong();
			else
				throw InvalidCast(ObjectType.Long, DataType);
		}


		public ElaArray AsArray()
		{
			if (DataType == ObjectType.Array)
				return (ElaArray)Ref;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsArray();
			else
				throw InvalidCast(ObjectType.Array, DataType);
		}


		public ElaList AsList()
		{
			if (DataType == ObjectType.List)
				return (ElaList)Ref;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsList();
			else
				throw InvalidCast(ObjectType.List, DataType);
		}


		public ElaTuple AsTuple()
		{
			if (DataType == ObjectType.Tuple)
				return (ElaTuple)Ref;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsTuple();
			else
				throw InvalidCast(ObjectType.Tuple, DataType);
		}


		public ElaRecord AsRecord()
		{
			if (DataType == ObjectType.Record)
				return (ElaRecord)Ref;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsRecord();
			else
				throw InvalidCast(ObjectType.Record, DataType);
		}


		public ElaFunction AsFunction()
		{
			if (DataType == ObjectType.Function)
				return (ElaFunction)Ref;
			else if (DataType == ObjectType.Lazy)
				return ((ElaLazy)Ref).AsFunction();
			else
				throw InvalidCast(ObjectType.Function, DataType);
		}


		public ElaUnit AsUnit()
		{
			if (DataType == ObjectType.Unit)
				return (ElaUnit)Ref;
			else
				throw InvalidCast(ObjectType.Unit, DataType);
		}


		private InvalidCastException InvalidCast(ObjectType target, ObjectType source)
		{
			return new InvalidCastException(Strings.GetMessage("InvalidCast", source.GetShortForm(), target.GetShortForm()));
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


        public ElaValue Convert(ObjectType type, ExecutionContext ctx)
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
        #endregion
        

		#region Fields
		internal int I4;

		internal ElaObject Ref;
		#endregion


		#region Properties
		public ObjectType DataType
		{
			get { return Ref != null ? (ObjectType)Type : ObjectType.None; }
		}


		internal int Type
		{
			get { return Ref.TypeId; }
		}
		#endregion
	}
}