using System;
using Ela.Linking;
using Ela.Runtime;

namespace Ela.Library.General
{
    public sealed class NumberModule : ForeignModule
    {
        public NumberModule()
        {

        }
        
        public override void Initialize()
        {
            Add<Single,Boolean>("infSingle", IsInfinitySingle);
            Add<Double,Boolean>("infDouble", IsInfinityDouble);
            Add<Single,Boolean>("posInfSingle", IsPositiveInfinitySingle);
            Add<Double,Boolean>("posInfDouble", IsPositiveInfinityDouble);
            Add<Single,Boolean>("negInfSingle", IsNegativeInfinitySingle);
            Add<Double,Boolean>("negInfDouble", IsNegativeInfinityDouble);
            Add<Single,Boolean>("nanSingle", IsNanSingle);
            Add<Double,Boolean>("nanDouble", IsNanDouble);
            Add<Double,Double>("floor", Floor);
            Add<Double,Double>("ceiling", Ceiling);
            Add<Double,Double>("round", Round);
            Add<Double,Int64>("truncate", Truncate);
        }

        public double Round(double x)
        {
            return Math.Round(x);
        }

        public long Truncate(double x)
        {
            return (Int64)Math.Truncate(x);
        }

        public double Floor(double val)
        {
            return Math.Floor(val);
        }

        public double Ceiling(double val)
        {
            return Math.Ceiling(val);
        }

        public bool IsInfinitySingle(float val)
        {
            return Single.IsInfinity(val);
        }

        public bool IsInfinityDouble(double val)
        {
            return Double.IsInfinity(val);
        }

        public bool IsNegativeInfinitySingle(float val)
        {
            return Single.IsNegativeInfinity(val);
        }

        public bool IsNegativeInfinityDouble(double val)
        {
            return Double.IsNegativeInfinity(val);
        }

        public bool IsPositiveInfinitySingle(float val)
        {
            return Single.IsPositiveInfinity(val);
        }

        public bool IsPositiveInfinityDouble(double val)
        {
            return Double.IsPositiveInfinity(val);
        }
        
        public bool IsNanSingle(float val)
        {
            return Single.IsNaN(val);
        }

        public bool IsNanDouble(double val)
        {
            return Double.IsNaN(val);
        }
    }
}
