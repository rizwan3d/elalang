using System;

namespace Ela.CodeModel
{
	public struct ElaLiteralValue
	{
		#region Construction
		private Conv data;
		private string str;

		public ElaLiteralValue(int val)
		{
			data = new Conv();
			data.I4_1 = val;
			str = null;
			LiteralType = ObjectType.Integer;
		}


		public ElaLiteralValue(float val)
		{
			data = new Conv();
			data.R4 = val;
			str = null;
			LiteralType = ObjectType.Single;
		}


		public ElaLiteralValue(string val)
		{
			data = new Conv();
			str = val;
			LiteralType = ObjectType.String;
		}


		public ElaLiteralValue(bool val)
		{
			data = new Conv();
			data.I4_1 = val ? 1 : 0;
			str = null;
			LiteralType = ObjectType.Boolean;
		}


		public ElaLiteralValue(char val)
		{
			data = new Conv();
			data.I4_1 = (Int32)val;
			str = null;
			LiteralType = ObjectType.Char;	
		}


		public ElaLiteralValue(long val)
		{
			data = new Conv();
			data.I8 = val;
			str = null;
			LiteralType = ObjectType.Long;	
		}


		public ElaLiteralValue(double val)
		{
			data = new Conv();
			data.R8 = val;
			str = null;
			LiteralType = ObjectType.Double;	
		}


		public ElaLiteralValue(ObjectType type)
		{
			data = new Conv();
			str = null;
			LiteralType = type;			
		}
		#endregion


		#region Methods
		public int AsInteger()
		{
			return data.I4_1;
		}


		public float AsReal()
		{
			return data.R4;
		}
		
		
		public long AsLong()
		{
			return data.I8;
		}


		public double AsDouble()
		{
			return data.R8;
		}


		public bool AsBoolean()
		{
			return data.I4_1 == 1;
		}


		public char AsChar()
		{
			return (Char)data.I4_1;
		}


		public string AsString()
		{
			return str;
		}


		internal Conv GetData()
		{
			return data;
		}


		public ElaLiteralValue MakeNegative()
		{
			if (LiteralType == ObjectType.Integer)
				return new ElaLiteralValue(-AsInteger());
			else if (LiteralType == ObjectType.Long)
				return new ElaLiteralValue(-AsLong());
			else if (LiteralType == ObjectType.Single)
				return new ElaLiteralValue(-AsReal());
			else if (LiteralType == ObjectType.Double)
				return new ElaLiteralValue(-AsDouble());
			else
				throw new NotSupportedException();
		}


		public override string ToString()
		{
			switch (LiteralType)
			{
				case ObjectType.Integer: return AsInteger().ToString(Culture.NumberFormat);
				case ObjectType.Long: return AsLong().ToString(Culture.NumberFormat) + "L";
				case ObjectType.Boolean: return AsBoolean().ToString().ToLower();
				case ObjectType.Char: return "'" + AsChar().ToString() + "'";
				case ObjectType.Single: return AsReal().ToString(Culture.NumberFormat);
				case ObjectType.Double: return AsDouble().ToString(Culture.NumberFormat) + "D";
				case ObjectType.String: return "\"" + AsString() + "\"";
				default: return String.Empty;
			}
		}
		#endregion


		#region Fields
		public readonly ObjectType LiteralType;
		#endregion
	}
}