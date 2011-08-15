using System;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
    public sealed class GuidModule : ForeignModule
    {
        #region Construction
		private TypeId guidTypeId;
        public GuidModule()
        {

        }
        #endregion


		#region Nested Classes
		public sealed class ElaGuid : ElaObject
		{
			#region Construction
            private const string TAG = "Guid#";

            public ElaGuid(Guid value, ElaMachine vm) : this(value, vm.GetTypeId(TAG))
            {

            }


			internal ElaGuid(Guid value, TypeId typeId) : base(typeId)
			{
                Value = value;
			}
			#endregion


			#region Methods
			public override string GetTag()
			{
				return "Guid#";
			}


			public override int GetHashCode()
			{
				return Value.GetHashCode();
			}


			protected override int Compare(ElaValue @this, ElaValue other)
			{
				var g1 = @this.As<ElaGuid>();
				var g2 = other.As<ElaGuid>();
				return g1 != null && g2 != null ? g1.Value.CompareTo(g2.Value) : -1;
			}


			public override string ToString()
			{
				return Value.ToString();
			}
			#endregion


			#region Properties
			public Guid Value { get; internal set; }
			#endregion
		}  
		#endregion


		#region Methods
		public override void RegisterTypes(TypeRegistrator registrator)
		{
			guidTypeId = registrator.ObtainTypeId("Guid#");
		}


		public override void Initialize()
        {
            Add<ElaGuid>("guid", NewGuid);
            Add<String,ElaVariant>("parse", Parse);
			Add<ElaGuid,ElaGuid,Boolean>("guidEqual", (l,r) => l.Value == r.Value);
			Add<ElaGuid,String>("guidToString", v => v.Value.ToString());
        }


        public ElaGuid NewGuid()
        {
            return new ElaGuid(Guid.NewGuid(), guidTypeId);
        }


        public ElaVariant Parse(string str)
        {
            var g = Guid.Empty;

            try
            {
                g = new Guid(str);
                return ElaVariant.Some(new ElaValue(new ElaGuid(g, guidTypeId)));
            }
            catch (ElaTypeException)
            {
                throw;
            }
            catch (Exception)
            {
                return ElaVariant.None();
            }
        }
		#endregion
    }
}
