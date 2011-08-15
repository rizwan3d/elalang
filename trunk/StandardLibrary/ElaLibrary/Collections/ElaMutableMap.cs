using System;
using System.Collections.Generic;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class ElaMutableMap : ElaObject
    {
        #region Construction
        private const string TAG = "MutableMap#";
        private const string NOKEY = "NoKey";


        public ElaMutableMap(ElaMachine vm) : this(vm.GetTypeId(TAG))
        {

        }

		
        internal ElaMutableMap(TypeId typeId) : base(typeId)
        {
            Map = new Dictionary<ElaValue,ElaValue>();
        }
        #endregion


        #region Methods
        public override string GetTag()
		{
			return TAG;
        }


        public override string ToString()
        {
            return new ElaValue(ConvertToRecord()).ToString();
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


        public ElaValue GetValue(ElaValue index)
        {
            var val = default(ElaValue);

            if (!Map.TryGetValue(index, out val))
                throw new ElaRuntimeException(ElaRuntimeError.IndexOutOfRange, index);
            
            return val;
        }


        public void SetValue(ElaValue index, ElaValue value)
        {
            if (!Map.ContainsKey(index))
                Map.Add(index, value);
            else
                Map[index] = value;
        }


        public ElaRecord ConvertToRecord()
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
