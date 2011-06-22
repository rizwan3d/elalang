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
        public override ElaPatterns GetSupportedPatterns()
        {
            return ElaPatterns.Tuple|ElaPatterns.HeadTail;
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
		#endregion


		#region Operations
		protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return new ElaValue(left.TypeId == right.TypeId && left.DirectGetString() == right.DirectGetString());
		}


		protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return new ElaValue(left.TypeId != right.TypeId || left.DirectGetString() != right.DirectGetString());
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.STR)
                return right.TypeId == ElaMachine.STR ? new ElaValue(left.DirectGetString().CompareTo(right.DirectGetString()) > 0) :
					right.Ref.Greater(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "greater");
			return Default();
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.STR)
                return right.TypeId == ElaMachine.STR ? new ElaValue(left.DirectGetString().CompareTo(right.DirectGetString()) < 0) :
					right.Ref.Lesser(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "lesser");
			return Default();
		}


		protected internal override ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.STR)
				return right.TypeId == ElaMachine.STR ? new ElaValue(left.DirectGetString().CompareTo(right.DirectGetString()) >= 0) :
					right.Ref.GreaterEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "greaterequal");
			return Default();
		}


		protected internal override ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId == ElaMachine.STR)
				return right.TypeId == ElaMachine.STR ? new ElaValue(left.DirectGetString().CompareTo(right.DirectGetString()) <= 0) :
					right.Ref.LesserEqual(left, right, ctx);
			
			ctx.InvalidLeftOperand(left, right, "lesserequal");
			return Default();
		}


		protected internal override ElaValue GetLength(ExecutionContext ctx)
		{
			return new ElaValue(buffer.Length - headIndex);
		}


		protected internal override ElaValue GetValue(ElaValue key, ExecutionContext ctx)
		{
			if (key.TypeId != ElaMachine.INT)
			{
				ctx.InvalidIndexType(key);
				return Default();
			}

			var val = GetValue();
			
			if (key.I4 >= val.Length || key.I4 < 0)
			{
				ctx.IndexOutOfRange(key, new ElaValue(this));
				return Default();
			}

			return new ElaValue(val[key.I4]);
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            //if (left.TypeId == ElaMachine.STR && right.TypeId == ElaMachine.STR)
            //    return new ElaValue(new ElaString(left.DirectGetString() + right.DirectGetString()));
            //else if (left.TypeId == ElaMachine.STR)
            //    return right.Ref.Concatenate(left, right, ctx);

            if (left.TypeId == ElaMachine.STR)
                return new ElaValue(left.DirectGetString() + right.Show(ShowInfo.Default, ctx));
            else
                return new ElaValue(left.Show(ShowInfo.Default, ctx) + right.DirectGetString());
			
			//ctx.InvalidLeftOperand(left, right, "concat");
			//return Default();
		}


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
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


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			var val = GetValue();
			val = info.StringLength > 0 && val.Length > info.StringLength ? 
				val.Substring(0, info.StringLength) + "..." : val;
			return val;
		}


		protected internal override ElaValue Head(ExecutionContext ctx)
		{
			return new ElaValue(buffer[headIndex]);
		}


		protected internal override ElaValue Tail(ExecutionContext ctx)
		{
			return new ElaValue(new ElaString(buffer, headIndex + 1));
		}


		protected internal override bool IsNil(ExecutionContext ctx)
		{
			return headIndex == buffer.Length;
		}


		protected internal override ElaValue Cons(ElaObject next, ElaValue value, ExecutionContext ctx)
		{
			var nextStr = next as ElaString;

			if (nextStr == null)
			{
				ctx.InvalidType(GetTypeName(), new ElaValue(next));
				return Default();
			}

			if (value.TypeCode != ElaTypeCode.String && value.TypeCode != ElaTypeCode.Char)
			{
				ctx.InvalidType(GetTypeName(), value);
				return Default();
			}

			return new ElaValue(new ElaString(value.ToString() + nextStr));
		}


		protected internal override ElaValue Nil(ExecutionContext ctx)
		{
			return new ElaValue(ElaString.Empty);
		}
		#endregion
	}
}
