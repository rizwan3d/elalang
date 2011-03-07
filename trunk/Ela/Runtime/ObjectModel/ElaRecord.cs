using System;
using System.Collections.Generic;
using System.Text;
using Ela.Debug;

namespace Ela.Runtime.ObjectModel
{
	public class ElaRecord : ElaObject
	{
		#region Construction
        private const string FIELDS = "fields";
        private const ElaTraits TRAITS = ElaTraits.Eq | ElaTraits.Get | ElaTraits.Set | ElaTraits.Len | ElaTraits.FieldGet | ElaTraits.FieldSet | ElaTraits.Convert | ElaTraits.Show | ElaTraits.Ix;
		private string[] keys;
        private ElaValue[] values;
		private bool[] flags;
        private int cons;

		private enum SetResult
		{
			None,
			Done,
			OutOfRange,
			Immutable
		}

		public ElaRecord(params ElaRecordField[] fields) : base(ElaTypeCode.Record, TRAITS)
		{
			keys = new string[fields.Length];
            values = new ElaValue[fields.Length];
			flags = new bool[fields.Length];

			for (var i = 0; i < fields.Length; i++)
				AddField(i, fields[i].Field, fields[i].Mutable, fields[i].Value);
		}


		internal ElaRecord(int size) : base(ElaTypeCode.Record, TRAITS)
		{
			keys = new string[size];
			flags = new bool[size];
            values = new ElaValue[size];
		}
		#endregion


		#region Traits
        protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Eq(left, right, ctx));
        }


        protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(!Eq(left, right, ctx));
        }


        private bool Eq(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (left.Ref == right.Ref)
                return true;

            var lt = left.Ref as ElaRecord;
            var rt = right.Ref as ElaRecord;

            if (lt == null || rt == null || rt.Length != lt.Length || 
                !(EqHelper.ListEquals(rt.keys, lt.keys)) ||
                !(EqHelper.ListEquals(rt.values, lt.values)))
                return false;

            return true;
        }


		protected internal override ElaValue GetValue(ElaValue key, ExecutionContext ctx)
		{
			key = key.Id(ctx);

			if (key.TypeId == ElaMachine.STR)
				return GetField(key.AsString(), ctx);
			else if (key.TypeId == ElaMachine.INT)
			{
				if (key.I4 != -1 && key.I4 < values.Length)
					return values[key.I4];
				else
				{
					ctx.IndexOutOfRange(key, new ElaValue(this));
					return base.GetValue(key, ctx);
				}
			}

			ctx.InvalidIndexType(key);
			return Default();
		}


		protected internal override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
		{
			index = index.Id(ctx);
			var res = SetResult.None;

			if (index.TypeId == ElaMachine.STR)
				res = SetValue(index.Ref.ToString(), value);
			else if (index.TypeId == ElaMachine.INT)
				res = SetValue(index.I4, value);
			else
				ctx.InvalidIndexType(index);

			if (res == SetResult.OutOfRange)
				ctx.IndexOutOfRange(index, new ElaValue(this));
			else if (res == SetResult.Immutable)
				ctx.Fail(ElaRuntimeError.FieldImmutable, index.Ref.ToString(), Show(ctx, ShowInfo.Default));
		}


		protected internal override ElaValue GetField(string field, ExecutionContext ctx)
		{
			var index = GetOrdinal(field);

			if (index != -1 && index < values.Length)
				return values[index];

			ctx.UnknownField(field, new ElaValue(this));
			return Default();
		}


		protected internal override void SetField(string field, ElaValue value, ExecutionContext ctx)
		{
			var res = SetValue(field, value);

			if (res == SetResult.Immutable)
				ctx.Fail(ElaRuntimeError.FieldImmutable, field, Show(ctx, ShowInfo.Default));
			else if (res == SetResult.OutOfRange)
				ctx.UnknownField(field, new ElaValue(this));
		}


		protected internal override bool HasField(string field, ExecutionContext ctx)
		{
			return GetOrdinal(field) != -1;
		}


		protected internal override ElaValue Convert(ElaTypeCode type, ExecutionContext ctx)
		{
			if (type == ElaTypeCode.Record)
				return new ElaValue(this);
			else if (type == ElaTypeCode.Tuple)
				return new ElaValue(new ElaTuple(values));
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

				if (flags[c - 1])
					sb.Append("!");

				sb.AppendFormat("{0}={1}", k, this[k].Ref.Show(this[k], ctx, info));
			}

			sb.Append('}');
			return sb.ToString();
		}


        protected internal override ElaValue GetLength(ExecutionContext ctx)
        {
            return new ElaValue(keys.Length);
        }
		#endregion


		#region Methods
		public override ElaTypeInfo GetTypeInfo()
		{
            var info = base.GetTypeInfo();
            info.AddField(FIELDS, keys);
            return info;
		}


		public IEnumerable<String> GetKeys()
		{
			return keys;
		}


		internal void AddField(string key, ElaValue value)
		{
			AddField(key, false, value);
		}


		internal void AddField(string key, bool mutable, ElaValue value)
		{
            keys[cons] = key;
            flags[cons] = mutable;
			values[cons] = value;
            cons++;
		}


		internal void AddField(int index, string key, bool mutable, ElaValue value)
		{
			keys[index] = key;
			values[index] = value;
			flags[index] = mutable;
		}


		private int GetOrdinal(string key)
		{
			for (var i = 0; i < keys.Length; i++)
				if (keys[i] == key)
					return i;

			return -1;
		}


		private SetResult SetValue(string key, ElaValue value)
		{
			var idx = GetOrdinal(key);
			return SetValue(idx, value);
		}


		private SetResult SetValue(int index, ElaValue value)
		{
			if (index > -1 && index < values.Length)
			{
				if (!flags[index])
					return SetResult.Immutable;

				values[index] = value;
				return SetResult.Done;
			}
			else
				return SetResult.OutOfRange;
		}
		#endregion


		#region Properties
        public int Length
        {
            get { return keys.Length; }
        }


		public ElaValue this[string key]
		{
			get
			{
				var index = GetOrdinal(key);

				if (index > -1 && index < values.Length)
					return values[index];
				else
					throw new IndexOutOfRangeException();
			}
			set
			{
				var res = SetValue(key, value);

				if (res == SetResult.OutOfRange)
					throw new IndexOutOfRangeException();
				else if (res == SetResult.Immutable)
					throw new InvalidOperationException();
			}
		}


		public ElaValue this[int index]
		{
			get
			{
				if (index > -1 && index < values.Length)
					return values[index];
				else
					throw new IndexOutOfRangeException();
			}
			set
			{
				var res = SetValue(index, value);

				if (res == SetResult.OutOfRange)
					throw new IndexOutOfRangeException();
				else if (res == SetResult.Immutable)
					throw new InvalidOperationException();
			}
		}
		#endregion
	}
}
