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
			Add<Double,Double>("exp", Exp);
			Add<Double,Double>("cos", Cos);
			Add<Double,Double>("sin", Sin);
			Add<Double,Double>("acos", Acos);
			Add<Double,Double>("asin", Asin);
			Add<Double,Double>("tan", Tan);
			Add<Double,Double>("atan", Atan);
			Add<Double,Double>("log", Log);
			Add<Int32,Int32,Int32,Int32>("rnd", Rnd);
			Add("pi", Math.PI);
			Add("e", Math.E);
            Add<String,ElaValue,ElaVariant>("createVariant", CreateVariant);
            Add<ElaValue,Boolean>("evaled", IsEvaled);
        }

        public bool IsEvaled(ElaValue obj)
        {
            return obj.Is<ElaLazy>();
        }

        public ElaVariant CreateVariant(string tag, ElaValue val)
        {
            return new ElaVariant(tag, val);
        }

		public double Exp(double val)
		{
			return Math.Exp(val);
		}
        
		public double Cos(double val)
		{
			return Math.Cos(val);
		}
        
		public double Sin(double val)
		{
			return Math.Sin(val);
		}
        
        public double Acos(double val)
        {
            return Math.Acos(val);
        }
        
        public double Asin(double val)
        {
            return Math.Asin(val);
        }
        
		public double Tan(double val)
		{
			return Math.Tan(val);
		}
              
		public double Atan(double val)
		{
			return Math.Atan(val);
		}
        
		public double Log(double x)
		{
			return Math.Log(x);
		}
        
        public int Rnd(int seed, int min, int max)
        {
			var rnd = new Random(seed);
            return rnd.Next(min, max);
        }
	}
}
