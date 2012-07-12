using System;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
	public sealed class CoreModule : ForeignModule
	{
		public CoreModule()
		{

		}
		
        public override void Initialize()
		{
			
			Add<Int32,Int32,Int32,Int32>("rnd", Rnd);
            Add<String,ElaValue,ElaVariant>("createVariant", CreateVariant);
            Add<ElaValue,Boolean>("evaled", IsEvaled);
        }

        public bool IsEvaled(ElaValue obj)
        {
            return !obj.Is<ElaLazy>() || obj.As<ElaLazy>().Evaled;
        }

        public ElaVariant CreateVariant(string tag, ElaValue val)
        {
            return new ElaVariant(tag, val);
        }
                
        public int Rnd(int seed, int min, int max)
        {
			var rnd = new Random(seed);
            return rnd.Next(min, max);
        }
	}
}
