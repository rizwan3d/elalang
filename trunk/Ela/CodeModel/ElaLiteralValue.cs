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
			LiteralType = ElaTypeCode.Integer;
		}


		public ElaLiteralValue(float val)
		{
			data = new Conv();
			data.R4 = val;
			str = null;
			LiteralType = ElaTypeCode.Single;
		}


		public ElaLiteralValue(string val)
		{
			data = new Conv();
			str = val;
			LiteralType = ElaTypeCode.String;
		}


		public ElaLiteralValue(bool val)
		{
			data = new Conv();
			data.I4_1 = val ? 1 : 0;
			str = null;
			LiteralType = ElaTypeCode.Boolean;
		}


		public ElaLiteralValue(char val)
		{
			data = new Conv();
			data.I4_1 = (Int32)val;
			str = null;
			LiteralType = ElaTypeCode.Char;	
		}


		public ElaLiteralValue(long val)
		{
			data = new Conv();
			data.I8 = val;
			str = null;
			LiteralType = ElaTypeCode.Long;	
		}


		public ElaLiteralValue(double val)
		{
			data = new Conv();
			data.R8 = val;
			str = null;
			LiteralType = ElaTypeCode.Double;	
		}


		public ElaLiteralValue(ElaTypeCode type)
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
			if (LiteralType == ElaTypeCode.Integer)
				return new ElaLiteralValue(-AsInteger());
			else if (LiteralType == ElaTypeCode.Long)
				return new ElaLiteralValue(-AsLong());
			else if (LiteralType == ElaTypeCode.Single)
				return new ElaLiteralValue(-AsReal());
			else if (LiteralType == ElaTypeCode.Double)
				return new ElaLiteralValue(-AsDouble());
			else
				throw new NotSupportedException();
		}


		public override string ToString()
		{
			switch (LiteralType)
			{
				case ElaTypeCode.Integer: return AsInteger().ToString(Culture.NumberFormat);
				case ElaTypeCode.Long: return AsLong().ToString(Culture.NumberFormat) + "L";
				case ElaTypeCode.Boolean: return AsBoolean().ToString().ToLower();
				case ElaTypeCode.Char: return "'" + AsChar().ToString() + "'";
				case ElaTypeCode.Single: return AsReal().ToString(Culture.NumberFormat);
				case ElaTypeCode.Double: return AsDouble().ToString(Culture.NumberFormat) + "D";
				case ElaTypeCode.String: return "\"" + AsString() + "\"";
				default: return String.Empty;
			}
		}
		#endregion


		#region Fields
		public readonly ElaTypeCode LiteralType;
		#endregion
	}
}