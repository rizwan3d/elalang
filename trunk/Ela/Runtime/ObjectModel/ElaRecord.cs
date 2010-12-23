using System;
using System.Collections.Generic;
using System.Text;
using Ela.Debug;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public class ElaRecord : ElaTuple
	{
		#region Construction
		private const ElaTraits TRAITS = ElaTraits.Ord | ElaTraits.Enum | ElaTraits.Eq | ElaTraits.Get | ElaTraits.Set | ElaTraits.Len | ElaTraits.Seq | ElaTraits.FieldGet | ElaTraits.FieldSet | ElaTraits.Convert | ElaTraits.Show;
		private string[] keys;

		public ElaRecord(params ElaRecordField[] fields)
			: base(fields.Length, ObjectType.Record, TRAITS)
		{
			keys = new string[fields.Length];

			for (var i = 0; i < fields.Length; i++)
				AddField(i, fields[i].Field, fields[i].Value);
		}


		internal ElaRecord(int size)
			: base(size, ObjectType.Record, TRAITS)
		{
			keys = new string[size];
		}
		#endregion


		#region Traits
		protected internal override ElaValue GetValue(ElaValue key, ExecutionContext ctx)
		{
			key = key.Id(ctx);

			if (key.Type == ElaMachine.STR)
				return GetField(key.AsString(), ctx);
			else if (key.Type == ElaMachine.INT)
			{
				if (key.I4 != -1 && key.I4 < Values.Length)
					return Values[key.I4];
				else
				{
					ctx.IndexOutOfRange(key, new ElaValue(this));
					return base.GetValue(key, ctx);
				}
			}

			ctx.InvalidIndexType(key.DataType);
			return Default();
		}


		protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
		{
			index = index.Id(ctx);

			if (index.Type == ElaMachine.STR)
			{
				if (!SetValue(index.Ref.ToString(), value))
					ctx.IndexOutOfRange(index, new ElaValue(this));

				return;
			}
			else if (index.Type == ElaMachine.INT)
			{
				if (!SetValue(index.I4, value))
					ctx.IndexOutOfRange(index, new ElaValue(this));

				return;
			}

			ctx.InvalidIndexType(index.DataType);
		}


		protected internal override ElaValue GetField(string field, ExecutionContext ctx)
		{
			var index = GetOrdinal(field);

			if (index != -1 && index < Values.Length)
				return Values[index];

			ctx.UnknownField(field, new ElaValue(this));
			return Default();
		}


		protected internal override void SetField(string field, ElaValue value, ExecutionContext ctx)
		{
			if (!SetValue(field, value))
				ctx.UnknownField(field, new ElaValue(this));
		}


		protected internal override bool HasField(string field, ExecutionContext ctx)
		{
			return GetOrdinal(field) != -1;
		}


		protected internal override ElaValue Convert(ObjectType type, ExecutionContext ctx)
		{
			if (type == ObjectType.Record)
				return new ElaValue(this);
			else if (type == ObjectType.Tuple)
				return new ElaValue(new ElaTuple(Values));
			else
			{
				ctx.ConversionFailed(new ElaValue(this), type);
				return base.Convert(type, ctx);
			}
		}


		protected internal override string Show(ExecutionContext ctx, ShowInfo info)
		{
			var sb = new StringBuilder();
			sb.Append('{');

			var c = 0;

			foreach (var k in GetKeys())
			{
				if (info.SequenceLength > 0 && c > info.SequenceLength)
				{
					sb.Append("...");
					break;
				}

				if (c++ > 0)
					sb.Append(",");

				sb.AppendFormat("{0}={1}", k, this[k].Ref.Show(this[k], ctx, info));
			}

			sb.Append('}');
			return sb.ToString();
		}
		#endregion


		#region Methods
		public override ElaTypeInfo GetTypeInfo()
		{
			return new ElaRecordInfo(this);
		}


		public IEnumerable<String> GetKeys()
		{
			return keys;
		}


		internal void AddField(string key, ElaValue value)
		{
			keys[base.Length] = key;
			base.InternalSetValue(value);
		}


		internal void AddField(int index, string key, ElaValue value)
		{
			keys[index] = key;
			Values[index] = value;
		}


		private int GetOrdinal(string key)
		{
			for (var i = 0; i < keys.Length; i++)
				if (keys[i] == key)
					return i;

			return -1;
		}


		private bool SetValue(string key, ElaValue value)
		{
			var idx = GetOrdinal(key);
			return SetValue(idx, value);
		}


		private bool SetValue(int index, ElaValue value)
		{
			if (index > -1 && index < Values.Length)
			{
				Values[index] = value;
				return true;
			}
			else
				return false;
		}
		#endregion


		#region Properties
		public ElaValue this[string key]
		{
			get
			{
				var index = GetOrdinal(key);

				if (index > -1 && index < Values.Length)
					return Values[index];
				else
					throw new IndexOutOfRangeException();
			}
			set
			{
				if (!SetValue(key, value))
					throw new IndexOutOfRangeException();
			}
		}


		public new ElaValue this[int index]
		{
			get
			{
				if (index > -1 && index < Values.Length)
					return Values[index];
				else
					throw new IndexOutOfRangeException();
			}
			set
			{
				if (!SetValue(index, value))
					throw new IndexOutOfRangeException();
			}
		}
		#endregion
	}
}
