using System;
using System.Collections.Generic;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class ElaMutableMap : ElaObject
    {
        #region Construction
        private const string NOKEY = "NoKey";
		private const string TYPENAME = "mutableMap";
		
        public ElaMutableMap() : base(ElaTraits.Eq | ElaTraits.Show | ElaTraits.Get | ElaTraits.Set | ElaTraits.Convert | ElaTraits.Len)
        {
            Map = new Dictionary<ElaValue,ElaValue>();
        }
        #endregion


        #region Methods
		protected override string GetTypeName()
		{
			return TYPENAME;
		}


		public void Add(ElaValue key, ElaValue value)
        {
            Map.Add(key, value);
        }


        public bool Remove(ElaValue key)
        {
            return Map.Remove(key);
        }


        public bool Contains(ElaValue key)
        {
            return Map.ContainsKey(key);
        }


        public void Clear()
        {
            Map.Clear();
        }


        protected override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.ReferenceEquals(right));
        }


        protected override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(!left.ReferenceEquals(right));
        }


        protected override string Show(ExecutionContext ctx, ShowInfo info)
        {
            return new ElaValue(ConvertToRecord()).Show(ctx, info);
        }


        protected override ElaValue Convert(ElaTypeCode type, ExecutionContext ctx)
        {
            if (type == ElaTypeCode.Record)
                return new ElaValue(ConvertToRecord());

            ctx.ConversionFailed(new ElaValue(this), type);
            return Default();
        }


        protected override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
        {
            var val = default(ElaValue);

            if (!Map.TryGetValue(index, out val))
            {
                ctx.Fail(NOKEY, String.Format("An element with a key '{0}' doesn't exist.", index));
                return Default();
            }

            return val;
        }


        protected override void SetValue(ElaValue index, ElaValue value, ExecutionContext ctx)
        {
            if (Map.ContainsKey(index))
                Map.Add(index, value);
            else
                Map[index] = value;
        }


        protected override ElaValue GetLength(ExecutionContext ctx)
        {
            return new ElaValue(Map.Count);
        }


        private ElaRecord ConvertToRecord()
        {
            var fields = new ElaRecordField[Map.Count];
            var c = 0;

            foreach (var kv in Map)
                fields[c++] = new ElaRecordField(kv.Key.ToString(), kv.Value, true);

            return new ElaRecord(fields);
        }
        #endregion


        #region Properties
        internal Dictionary<ElaValue, ElaValue> Map { get; private set; }

        public int Count
        {
            get { return Map.Count; }
        }
        #endregion
    }
}
