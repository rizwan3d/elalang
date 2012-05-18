using System;
using System.Collections.Generic;
using System.Text;
using Ela.Debug;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaRecord : ElaObject, IEnumerable<ElaRecordField>
	{
		#region Construction
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.Record), (Int32)ElaTypeCode.Record, true, typeof(ElaRecord));

        private const string FIELDS = "fields";
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
        protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(Eq(left, right, ctx));
        }


        protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
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
                !(EqHelper.ListEquals(rt.values, lt.values, ctx)))
                return false;

            return true;
        }


		protected internal override ElaValue GetValue(ElaValue key, ExecutionContext ctx)
		{
			if (key.TypeId == ElaMachine.STR)
                return GetField(key.DirectGetString(), ctx);
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
				ctx.Fail(ElaRuntimeError.ImmutableStructure, index.Ref.ToString(), Show(new ElaValue(this), ShowInfo.Default, ctx));
		}


		private ElaValue GetField(string field, ExecutionContext ctx)
		{
			var index = GetOrdinal(field);

			if (index != -1 && index < values.Length)
				return values[index];

            ctx.IndexOutOfRange(new ElaValue(field), new ElaValue(this));
			return Default();
		}


        private void SetField(string field, ElaValue value, ExecutionContext ctx)
		{
			var res = SetValue(field, value);

			if (res == SetResult.Immutable)
				ctx.Fail(ElaRuntimeError.ImmutableStructure, field, Show(new ElaValue(this), ShowInfo.Default, ctx));
			else if (res == SetResult.OutOfRange)
                ctx.IndexOutOfRange(new ElaValue(field), new ElaValue(this));
		}


		protected internal override bool Has(string field, ExecutionContext ctx)
		{
			return GetOrdinal(field) != -1;
		}


        protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
			if (type.ReflectedTypeCode == ElaTypeCode.Record)
				return @this;
			else if (type.ReflectedTypeCode == ElaTypeCode.Tuple)
				return new ElaValue(new ElaTuple(values));

            ctx.ConversionFailed(new ElaValue(this), type.ReflectedTypeName);
            return Default();
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


        protected internal override ElaValue GetLength(ExecutionContext ctx)
        {
            return new ElaValue(keys.Length);
        }


        protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (left.TypeId == ElaMachine.REC && right.TypeId == ElaMachine.REC)
                return new ElaValue(Concat((ElaRecord)left.Ref, (ElaRecord)right.Ref));
            else
                return left.Force(ctx).Ref.Concatenate(left.Force(ctx), right.Force(ctx), ctx);
        }


        private ElaRecord Concat(ElaRecord left, ElaRecord right)
        {
            var list = new List<ElaRecordField>();
            list.AddRange(left);
            list.AddRange(right);
            return new ElaRecord(list.ToArray());
        }
        #endregion


        #region Methods
        public IEnumerator<ElaRecordField> GetEnumerator()
        {
            for (var i = 0; i < keys.Length; i++)
                yield return new ElaRecordField(keys[i], values[i], flags[i]);
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public bool HasField(string field)
        {
            return Array.IndexOf<String>(keys, field) != -1;
        }


        public override ElaPatterns GetSupportedPatterns()
        {
            return ElaPatterns.Record|ElaPatterns.Tuple;
        }


        public override ElaTypeInfo GetTypeInfo()
		{
            return CreateTypeInfo(new ElaRecordField(FIELDS, keys));
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
