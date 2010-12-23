using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaString : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Show | ElaTraits.Eq | ElaTraits.Get | ElaTraits.Len | ElaTraits.Convert | ElaTraits.Concat | ElaTraits.Fold;
		private string buffer;
		private int headIndex;
		
		public ElaString(string value) : this(value, 0)
		{
			
		}


		private ElaString(string value, int headIndex) : base(ObjectType.String, TRAITS)
		{
			this.buffer = value ?? String.Empty;
			this.headIndex = headIndex;
		}
		#endregion


		#region Traits
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.Type == right.Type && left.AsString() == right.AsString());
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.Type != right.Type || left.AsString() != right.AsString());
		}


		protected internal override ElaValue GetLength(ExecutionContext ctx)
		{
			return new ElaValue(buffer.Length - headIndex);
		}


		protected internal override ElaValue GetValue(ElaValue key, ExecutionContext ctx)
		{
			if (key.Type != ElaMachine.INT)
			{
				ctx.InvalidIndexType(key.DataType);
				return base.GetValue(key, ctx);
			}

			var val = GetValue();
			
			if (key.I4 >= val.Length || key.I4 < 0)
			{
				ctx.IndexOutOfRange(key, new ElaValue(this));
				return base.GetValue(key, ctx);
			}

			return new ElaValue(val[key.I4]);
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Type == ElaMachine.STR)
			{
				if (right.Type == ElaMachine.STR)
					return new ElaValue(new ElaString(left.AsString() + right.AsString()));
				else
					return right.Ref.Concatenate(left, right, ctx);
			}
			else
			{
				ctx.InvalidLeftOperand(left, right, ElaTraits.Concat);
				return Default();
			}
		}


		protected internal override ElaValue Convert(ObjectType type, ExecutionContext ctx)
		{
			if (type == ObjectType.String)
				return new ElaValue(this);
			else if (type == ObjectType.Char)
			{
				var val = GetValue();
				return val.Length > 0 ? new ElaValue(val[0]) : new ElaValue('\0');
			}
			else if (type == ObjectType.Integer)
			{
				try
				{
					return new ElaValue(Int32.Parse(GetValue()));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(new ElaValue(this), type, ex.Message);
					return base.Convert(type, ctx);
				}
			}
			else if (type == ObjectType.Long)
			{
				try
				{
					return new ElaValue(Int64.Parse(GetValue()));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(new ElaValue(this), type, ex.Message);
					return base.Convert(type, ctx);
				}
			}
			else if (type == ObjectType.Single)
			{
				try
				{
					return new ElaValue(float.Parse(GetValue(), Culture.NumberFormat));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(new ElaValue(this), type, ex.Message);
					return base.Convert(type, ctx);
				}
			}
			else if (type == ObjectType.Double)
			{
				try
				{
					return new ElaValue(double.Parse(GetValue(), Culture.NumberFormat));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(new ElaValue(this), type, ex.Message);
					return base.Convert(type, ctx);
				}
			}
			else if (type == ObjectType.Boolean)
			{
				try
				{
					return new ElaValue(bool.Parse(GetValue()));
				}
				catch (Exception ex)
				{
					ctx.ConversionFailed(new ElaValue(this), type, ex.Message);
					return base.Convert(type, ctx);
				}
			}
			else
			{
				ctx.ConversionFailed(new ElaValue(this), type);
				return base.Convert(type, ctx);
			}
		}


		protected internal override string Show(ExecutionContext ctx, ShowInfo info)
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
		#endregion


		#region Methods
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
	}
}
