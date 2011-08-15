using System;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
	public sealed class MapModule : ForeignModule
	{
		#region Construction
        private TypeId mapTypeId;

		public MapModule()
		{

		}
		#endregion


		#region Methods
		public override void Initialize()
		{
			Add<ElaRecord,ElaMap>("map", CreateMap);
			Add<ElaValue,ElaValue,ElaMap,ElaMap>("add", Add);
			Add<ElaValue,ElaMap,ElaMap>("remove", Remove);
			Add<ElaValue,ElaMap,Boolean>("contains", Contains);
			Add<ElaValue,ElaMap,ElaValue>("get", (i,m) => m.GetValue(i));
            Add<ElaMap,ElaRecord>("toRecord", m => m.ConvertToRecord());
	
            Add<ElaMap,String>("toString", m => m.ToString());
            Add<ElaMap,Int32>("mapLength", m => m.Length);
       	}


        public override void RegisterTypes(TypeRegistrator registrator)
        {
            mapTypeId = registrator.ObtainTypeId("Map#");
        }


		public ElaMap CreateMap(ElaRecord rec)
		{
			var map = new ElaMap(AvlTree.Empty, mapTypeId);

			foreach (var k in rec.GetKeys())
				map = new ElaMap(map.Tree.Add(new ElaValue(k), rec[k]), mapTypeId);

			return map;
		}


		public ElaMap Add(ElaValue key, ElaValue value, ElaMap map)
		{
			return map.Add(key, value);
		}


		public ElaMap Remove(ElaValue key, ElaMap map)
		{
			return map.Remove(key);
		}


		public bool Contains(ElaValue key, ElaMap map)
		{
			return map.Contains(key);
		}
		#endregion
	}
}