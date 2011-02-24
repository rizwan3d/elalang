using System;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
using System.Collections.Generic;

namespace Ela.Library.Collections
{
    public sealed class MapModule : ForeignModule
    {
        #region Construction
        public MapModule()
        {

        }
        #endregion


        #region Nested Classes
        public sealed class ElaMap : ElaObject
        {
            #region Construction
            private const string NOKEY = "NoKey";

            internal ElaMap() : base(ElaTraits.Eq|ElaTraits.Show|ElaTraits.Get|ElaTraits.Set|ElaTraits.Convert|ElaTraits.Len)
            {
                Map = new Dictionary<ElaValue,ElaValue>();
            }
            #endregion


            #region Methods
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


            protected override ElaValue Convert(ObjectType type, ExecutionContext ctx)
            {
                if (type == ObjectType.Record)
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
            internal Dictionary<ElaValue,ElaValue> Map { get; private set; }
            #endregion
        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<ElaMap>("emptyMap", CreateEmptyMap);
            Add<ElaRecord,ElaMap>("map", CreateMap);
            Add<ElaValue,ElaMap,Boolean>("contains", Contains);
            Add<ElaValue,ElaMap,ElaVariant>("get", Get);
            Add<ElaMap,ElaList>("keys", GetKeys);
            Add<ElaMap,ElaList>("values", GetValues);
        }


        public ElaMap CreateEmptyMap()
        {
            return new ElaMap();
        }


        public ElaMap CreateMap(ElaRecord rec)
        {
            var map = new ElaMap();

            foreach (var k in rec.GetKeys())
                map.Map.Add(new ElaValue(k), rec[k]);

            return map;
        }


        public bool Contains(ElaValue key, ElaMap map)
        {
            return map.Map.ContainsKey(key);
        }


        public ElaVariant Get(ElaValue key, ElaMap map)
        {
            var val = default(ElaValue);

            if (!map.Map.TryGetValue(key, out val))
                return ElaVariant.None();

            return ElaVariant.Some(val);
        }


        public ElaList GetKeys(ElaMap map)
        {
            return ElaList.FromEnumerable(map.Map.Keys);
        }


        public ElaList GetValues(ElaMap map)
        {
            return ElaList.FromEnumerable(map.Map.Values);
        }
        #endregion
    }
}