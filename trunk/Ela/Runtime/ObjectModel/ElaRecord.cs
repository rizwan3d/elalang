using System;
using System.Collections.Generic;
using System.Text;
using Ela.Debug;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaRecord : ElaObject
	{
		#region Construction
        private const string FIELDS = "fields";
        internal string[] keys;
        internal ElaValue[] values;
		internal bool[] flags;
        private int cons;

		private enum SetResult
		{
			None,
			Done,
			OutOfRange,
			Immutable
		}

		public ElaRecord(params ElaRecordField[] fields) : base(ElaTypeCode.Record)
		{
			keys = new string[fields.Length];
            values = new ElaValue[fields.Length];
			flags = new bool[fields.Length];

			for (var i = 0; i < fields.Length; i++)
				AddField(i, fields[i].Field, fields[i].Mutable, fields[i].Value);
		}


		internal ElaRecord(int size) : base(ElaTypeCode.Record)
		{
			keys = new string[size];
			flags = new bool[size];
            values = new ElaValue[size];
		}
		#endregion


		#region Operations
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
				ctx.Fail(ElaRuntimeError.FieldImmutable, field, Show(new ElaValue(this), ShowInfo.Default, ctx));
			else if (res == SetResult.OutOfRange)
				ctx.UnknownField(field, new ElaValue(this));
		}


		protected internal override bool HasField(string field, ExecutionContext ctx)
		{
			return GetOrdinal(field) != -1;
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
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

				sb.AppendFormat("{0}={1}", k, this[k].Ref.Show(this[k], info, ctx));
			}

			sb.Append('}');
			return sb.ToString();
		}


        internal ElaValue Clone()
        {
            var rec = new ElaRecord(values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                var v = values[i];
                rec.AddField(i, keys[i], flags[i], v);
            }

            return new ElaValue(rec);
        }
        #endregion


        #region Methods
        internal override ElaValue Convert(ElaValue @this, ElaTypeCode type)
        {
            if (type == ElaTypeCode.Tuple)
                return new ElaValue(new ElaTuple(values));

            return base.Convert(@this, type);
        }


        internal override string GetTag()
        {
            return "Record#";
        }


        internal override int GetLength(ExecutionContext ctx)
        {
            return Length;
        }


        public override ElaPatterns GetSupportedPatterns()
        {
            return ElaPatterns.Record|ElaPatterns.Tuple;
        }


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


		internal void AddField(string key, bool mutable, ElaValue value)
		{
            AddField(cons, key, mutable, value);
            cons++;
		}


		internal void AddField(int index, string key, bool mutable, ElaValue value)
		 {
			keys[index] = key;
			values[index] = value;
			flags[index] = mutable;
		}


		internal int GetOrdinal(string key)
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
