using System;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class MutableMapModule : ForeignModule
    {
        #region Construction
        public MutableMapModule()
        {

        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<ElaMutableMap>("empty", CreateEmptyMap);
            Add<ElaRecord,ElaMutableMap>("map", CreateMap);
            Add<ElaValue,ElaMutableMap,Boolean>("contains", Contains);
            Add<ElaValue,ElaMutableMap,ElaValue>("get", Get);
            Add<ElaValue,ElaValue,ElaMutableMap,ElaUnit>("set", Set);
            Add<ElaMutableMap,ElaList>("keys", GetKeys);
            Add<ElaMutableMap,ElaList>("values", GetValues);
            Add<ElaMutableMap,ElaRecord>("toRecord", m => m.ConvertToRecord());

            Add<ElaValue,ElaValue,Boolean>("mapEqual", (l,r) => l.ReferenceEquals(r));
            Add<ElaMutableMap,String>("toString", m => m.ToString());
            Add<ElaMutableMap,Int32>("mapLength", m => m.Count);
        }


        public ElaMutableMap CreateEmptyMap()
        {
            return new ElaMutableMap();
        }


        public ElaMutableMap CreateMap(ElaRecord rec)
        {
            var map = new ElaMutableMap();

            foreach (var k in rec.GetKeys())
                map.Map.Add(new ElaValue(k), rec[k]);

            return map;
        }


        public bool Contains(ElaValue key, ElaMutableMap map)
        {
            return map.Map.ContainsKey(key);
        }


        public ElaValue Get(ElaValue key, ElaMutableMap map)
        {
            return map.GetValue(key);
        }


        public ElaUnit Set(ElaValue key, ElaValue value, ElaMutableMap map)
        {
            map.SetValue(key, value);
            return ElaUnit.Instance;
        }


        public ElaList GetKeys(ElaMutableMap map)
        {
            return ElaList.FromEnumerable(map.Map.Keys);
        }


        public ElaList GetValues(ElaMutableMap map)
        {
            return ElaList.FromEnumerable(map.Map.Values);
        }
        #endregion
    }
}