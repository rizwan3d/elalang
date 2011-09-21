using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaString : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		public static readonly ElaString Empty = new ElaString(String.Empty);
		private string buffer;
		private int headIndex;
		private Guid g = Guid.NewGuid();
		
		public ElaString(string value) : this(value, 0)
		{
			
		}


		private ElaString(string value, int headIndex) : base(ElaTypeCode.String)
		{
			this.buffer = value ?? String.Empty;
			this.headIndex = headIndex;
		}
		#endregion


		#region Methods
		public override string GetTag()
        {
            return "String#";
        }


        public override bool Equals(ElaValue other)
        {
            return other.TypeCode == ElaTypeCode.String ? other.DirectGetString() == buffer :
                false;
        }


        public override int GetHashCode()
		{
			return buffer.GetHashCode();
		}


        internal protected override int Compare(ElaValue @this, ElaValue other)
		{
			return other.TypeCode == ElaTypeCode.String ? buffer.CompareTo(((ElaString)other.Ref).buffer) : -1;
		}
		

		public IEnumerator<ElaValue> GetEnumerator()
		{
			foreach (var c in GetValue())
				yield return new ElaValue(c);
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		internal string GetValue()
		{
			return headIndex == 0 ? buffer : buffer.Substring(headIndex);
		}


		public override string ToString()
		{
			return buffer;
		}
		#endregion


		#region Operations
        internal override int GetLength(ExecutionContext ctx)
        {
            return Length;
        }


		internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			if (type == ElaTypeCode.String)
				return new ElaValue(this);
			else if (type == ElaTypeCode.Char)
			{
				var val = GetValue();
				return val.Length > 0 ? new ElaValue(val[0]) : new ElaValue('\0');
			}
			else if (type == ElaTypeCode.Integer)
			{
				try
				{
					return new ElaValue(Int32.Parse(GetValue()));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(@this, type, ex.Message);
					return Default();
				}
			}
			else if (type == ElaTypeCode.Long)
			{
				try
				{
					return new ElaValue(Int64.Parse(GetValue()));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(@this, type, ex.Message);
					return Default();
				}
			}
			else if (type == ElaTypeCode.Single)
			{
				try
				{
					return new ElaValue(Single.Parse(GetValue(), Culture.NumberFormat));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(@this, type, ex.Message);
					return Default();
				}
			}
			else if (type == ElaTypeCode.Double)
			{
				try
				{
					return new ElaValue(Double.Parse(GetValue(), Culture.NumberFormat));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(@this, type, ex.Message);
					return Default();
				}
			}
			else if (type == ElaTypeCode.Boolean)
			{
				try
				{
					return new ElaValue(Boolean.Parse(GetValue()));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(@this, type, ex.Message);
					return Default();
				}
			}
			
			ctx.ConversionFailed(@this, type);
			return Default();
		}


		internal ElaValue Head()
		{
			return new ElaValue(buffer[headIndex]);
		}


		internal ElaValue Tail()
		{
			return new ElaValue(new ElaString(buffer, headIndex + 1));
		}


		internal bool IsNil()
		{
			return headIndex == buffer.Length;
		}
		#endregion


        #region Properties
        public int Length
        {
            get { return buffer.Length - headIndex; }
        }
        #endregion
    }
}
