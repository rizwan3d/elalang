using System;
using Ela.Runtime.ObjectModel;
using System.Runtime.InteropServices;

namespace Ela.Runtime
{
	public struct RuntimeValue : IComparable<RuntimeValue>, IConvertible
	{
		#region Construction
		public RuntimeValue(ElaObject val)
		{
			I4 = 0;
			Ref = val;
		}


		public RuntimeValue(int val)
		{
			I4 = val;
			Ref = ElaObject.Integer;
		}


		public RuntimeValue(char val)
		{
			I4 = (Int32)val;
			Ref = ElaObject.Char;
		}


		public RuntimeValue(int val, ElaObject obj)
		{
			I4 = val;
			Ref = obj;
		}


		public RuntimeValue(float val)
		{
			var conv = new Conv();
			conv.R4 = val;
			I4 = conv.I4_1;
			Ref = ElaObject.Single;
		}


		public RuntimeValue(bool val)
		{
			I4 = val ? 1 : 0;
			Ref = ElaObject.Boolean;
		}


		public RuntimeValue(string value)
		{
			I4 = 0;
			Ref = new ElaString(value);
		}


		public RuntimeValue(long val)
		{
			I4 = 0;
			Ref = new ElaLong(val);
		}


		public RuntimeValue(double val)
		{
			I4 = 0;
			Ref = new ElaDouble(val);
		}
		#endregion


		#region Methods
		internal float GetReal()
		{
			if (DataType == ObjectType.Integer)
				return (Single)I4;
			else
			{
				var conv = new Conv();
				conv.I4_1 = I4;
				return conv.R4;
			}
		}


		internal double GetDouble()
		{
			return DataType == ObjectType.Double ? ((ElaDouble)Ref).Value :
				DataType == ObjectType.Single ? GetReal() :
				(Double)I4;
		}


		internal string GetString()
		{
			return ToString();
		}


		internal long GetLong()
		{
			return DataType == ObjectType.Long ? ((ElaLong)Ref).Value : I4;				
		}


		internal long GetInteger()
		{
			switch (DataType)
			{
				case ObjectType.Integer:
				case ObjectType.Char:
				case ObjectType.Boolean:
					return I4;
				case ObjectType.Long:
					return ((ElaLong)Ref).Value;
				case ObjectType.Double:
					return (Int32)((ElaDouble)Ref).Value;
				case ObjectType.Single:
					var conv = new Conv();
					conv.I4_1 = I4;
					return (Int32)conv.R4;
				default: return 0;
			}
		}


		public static RuntimeValue FromObject(object value)
		{
			if (value is ElaObject)
				return new RuntimeValue((ElaObject)value);
			else if (value is Int32)
				return new RuntimeValue((Int32)value);
			else if (value is Int64)
				return new RuntimeValue((Int64)value);
			else if (value is Single)
				return new RuntimeValue((Single)value);
			else if (value is Double)
				return new RuntimeValue((Double)value);
			else if (value is Boolean)
				return new RuntimeValue((Boolean)value);
			else if (value is Char)
				return new RuntimeValue((Char)value);
			else if (value is String)
				return new RuntimeValue((String)value);
			else if (value is RuntimeValue)
				return (RuntimeValue)value;
			else
				throw new NotSupportedException();
		}


		public object ToObject()
		{
			switch (DataType)
			{
				case ObjectType.Integer: return I4;
				case ObjectType.Char: return (Char)I4;
				case ObjectType.Long: return ((ElaLong)Ref).Value;
				case ObjectType.Single: return GetReal();
				case ObjectType.Double: return ((ElaDouble)Ref).Value;
				case ObjectType.Boolean: return I4 > 0;
				case ObjectType.String: return Ref.ToString();
				default: return Ref;
			}
		}


		public override string ToString()
		{
			switch (DataType)
			{
				case ObjectType.Integer: return I4.ToString();
				case ObjectType.Char: return ((Char)I4).ToString();
				case ObjectType.Single: return GetReal().ToString();
				case ObjectType.Boolean: return (I4 > 0).ToString();
				default: return Ref != null ? Ref.ToString() : String.Empty;
			}
		}


		public int CompareTo(RuntimeValue other)
		{
			var aff = ElaMachine.GetOperationAffinity(Ref, other.Ref);

			switch (aff)
			{
				case ObjectType.Integer: 
				case ObjectType.Char:
				case ObjectType.Boolean: 
					return GetInteger().CompareTo(other.GetInteger());
				case ObjectType.Single: return GetReal().CompareTo(other.GetReal());
				case ObjectType.Long: return GetLong().CompareTo(other.GetLong());
				case ObjectType.Double: return GetDouble().CompareTo(other.GetDouble());
				case ObjectType.String: return GetString().CompareTo(other.GetString());
				case ObjectType.Array:
				case ObjectType.Tuple:
					{
						var t = (ElaArray)Ref;
						var t2 = (ElaArray)other.Ref;

						if (t.Length == t2.Length)
						{
							for (var i = 0; i < t.Length; i++)
							{
								var val = 0;

								if ((val = t[i].CompareTo(t2[i])) != 0)
									return val;
							}

							return 0;
						}
						else
							return -1;
					}
				default:
					return Ref != other.Ref ? -1 : 0;
			}
		}
		#endregion


		#region IConvertible Members
		public TypeCode GetTypeCode()
		{
			throw new NotImplementedException();
		}

		
		public bool ToBoolean(IFormatProvider provider)
		{
			if (DataType == ObjectType.Boolean)
				return I4 > 0;
			else
				throw new ElaCastException(ObjectType.Boolean, DataType);
		}


		public byte ToByte(IFormatProvider provider)
		{
			if (DataType == ObjectType.Boolean)
				return (Byte)I4;
			else
				throw new ElaCastException(ObjectType.Boolean, DataType);
		}


		public char ToChar(IFormatProvider provider)
		{
			if (DataType == ObjectType.Char)
				return (Char)I4;
			else
				throw new ElaCastException(ObjectType.Char, DataType);
		}


		public DateTime ToDateTime(IFormatProvider provider)
		{
			if (DataType == ObjectType.String)
				return DateTime.Parse(Ref.ToString());
			else
				throw new ElaCastException(ObjectType.String, DataType);
		}


		public decimal ToDecimal(IFormatProvider provider)
		{
			if (DataType == ObjectType.Single)
				return (Decimal)GetReal();
			else if (DataType == ObjectType.Double)
				return (Decimal)((ElaDouble)Ref).Value;
			else
				throw new ElaCastException(ObjectType.Double, DataType);
		}


		public double ToDouble(IFormatProvider provider)
		{
			if (DataType == ObjectType.Double)
				return ((ElaDouble)Ref).Value;
			else if (DataType == ObjectType.Single)
				return (Double)GetReal();
			else
				throw new ElaCastException(ObjectType.Double, DataType);
		}


		public short ToInt16(IFormatProvider provider)
		{
			if (DataType == ObjectType.Integer)
				return (Int16)I4;
			else
				throw new ElaCastException(ObjectType.Integer, DataType);
		}


		public int ToInt32(IFormatProvider provider)
		{
			if (DataType == ObjectType.Integer)
				return I4;
			else
				throw new ElaCastException(ObjectType.Integer, DataType);
		}


		public long ToInt64(IFormatProvider provider)
		{
			if (DataType == ObjectType.Integer)
				return (Int64)I4;
			else if (DataType == ObjectType.Long)
				return ((ElaLong)Ref).Value;
			else
				throw new ElaCastException(ObjectType.Long, DataType);
		}


		public sbyte ToSByte(IFormatProvider provider)
		{
			if (DataType == ObjectType.Integer)
				return (SByte)I4;
			else
				throw new ElaCastException(ObjectType.Integer, DataType);
		}


		public float ToSingle(IFormatProvider provider)
		{
			if (DataType == ObjectType.Single)
				return GetReal();
			else
				throw new ElaCastException(ObjectType.Single, DataType);
		}


		public string ToString(IFormatProvider provider)
		{
			return ToString();
		}


		public object ToType(Type conversionType, IFormatProvider provider)
		{
			switch (System.Type.GetTypeCode(conversionType))
			{
				case TypeCode.Int16: return ToInt16(provider);
				case TypeCode.Int32: return ToInt32(provider);
				case TypeCode.Double: return ToDouble(provider);
				case TypeCode.Decimal: return ToDecimal(provider);
				case TypeCode.DateTime: return ToDateTime(provider);
				case TypeCode.Char: return ToChar(provider);
				case TypeCode.Byte: return ToByte(provider);
				case TypeCode.Boolean: return ToBoolean(provider);
				case TypeCode.Int64: return ToInt64(provider);
				case TypeCode.SByte: return ToSByte(provider);
				case TypeCode.Single: return ToSingle(provider);
				case TypeCode.String: return ToString(provider);
				case TypeCode.UInt16: return ToUInt16(provider);
				case TypeCode.UInt32: return ToUInt32(provider);
				case TypeCode.UInt64: return ToUInt64(provider);
				default: throw new ElaCastException(ObjectType.None, DataType);
			}
		}


		public ushort ToUInt16(IFormatProvider provider)
		{
			if (DataType == ObjectType.Integer)
				return (UInt16)I4;
			else
				throw new ElaCastException(ObjectType.Integer, DataType);
		}


		public uint ToUInt32(IFormatProvider provider)
		{
			if (DataType == ObjectType.Integer)
				return (UInt32)I4;
			else
				throw new ElaCastException(ObjectType.Integer, DataType);
		}


		public ulong ToUInt64(IFormatProvider provider)
		{
			if (DataType == ObjectType.Integer)
				return (UInt64)I4;
			else if (DataType == ObjectType.Long)
				return (UInt64)((ElaLong)Ref).Value;
			else
				throw new ElaCastException(ObjectType.Long, DataType);
		}


		public ElaArray ToArray()
		{
			if (DataType == ObjectType.Array ||
				DataType == ObjectType.Tuple ||
				DataType == ObjectType.String)
				return (ElaArray)Ref;
			else
				throw new ElaCastException(ObjectType.Array, DataType);
		}


		public ElaList ToList()
		{
			if (DataType == ObjectType.List)
				return (ElaList)Ref;
			else
				throw new ElaCastException(ObjectType.List, DataType);
		}


		public ElaTuple ToTuple()
		{
			if (DataType == ObjectType.Tuple)
				return (ElaTuple)Ref;
			else
				throw new ElaCastException(ObjectType.Tuple, DataType);
		}


		public ElaLazy ToLazy()
		{
			if (DataType == ObjectType.Lazy)
				return (ElaLazy)Ref;
			else
				throw new ElaCastException(ObjectType.Lazy, DataType);
		}


		public ElaRecord ToRecord()
		{
			if (DataType == ObjectType.Record)
				return (ElaRecord)Ref;
			else
				throw new ElaCastException(ObjectType.Record, DataType);
		}


		public ElaSequence ToSequence()
		{
			if (DataType == ObjectType.Sequence)
				return (ElaSequence)Ref;
			else if (DataType == ObjectType.Array ||
				DataType == ObjectType.List ||
				DataType == ObjectType.Tuple)
				return new ElaSequence(Ref);
			else
				throw new ElaCastException(ObjectType.Sequence, DataType);
		}


		public ElaFunction ToFunction()
		{
			if (DataType == ObjectType.Function)
				return (ElaFunction)Ref;
			else
				throw new ElaCastException(ObjectType.Function, DataType);
		}


		public ElaModule ToModule()
		{
			if (DataType == ObjectType.Module)
				return (ElaModule)Ref;
			else
				throw new ElaCastException(ObjectType.Module, DataType);
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