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
            Add<ElaValue,Boolean>("inf", IsInfinity);
            Add<ElaValue,Boolean>("nan", IsNan);
            Add<ElaValue,Boolean>("posInf", IsPositiveInfinity);
            Add<ElaValue,Boolean>("negInf", IsNegativeInfinity);
            Add("maxInt", new ElaValue(Int32.MaxValue));
            Add("minInt", new ElaValue(Int32.MinValue));
            Add("maxLong", new ElaValue(Int64.MaxValue));
            Add("minLong", new ElaValue(Int64.MinValue));
            Add("maxSingle", new ElaValue(Single.MaxValue));
            Add("minSingle", new ElaValue(Single.MinValue));
            Add("maxDouble", new ElaValue(Double.MaxValue));
            Add("minDouble", new ElaValue(Double.MinValue));
        }

        public bool IsInfinity(ElaValue val)
        {
            return
                val.TypeCode == ElaTypeCode.Double ? Double.IsInfinity((Double)val.AsObject()) :
                val.TypeCode == ElaTypeCode.Single ? Single.IsInfinity((Single)val.AsObject()) :
                false;
        }
        
        public bool IsNan(ElaValue val)
        {
            return
                val.TypeCode == ElaTypeCode.Double ? Double.IsNaN((Double)val.AsObject()) :
                val.TypeCode == ElaTypeCode.Single ? Single.IsNaN((Single)val.AsObject()) :
                false;
        }
        
        public bool IsNegativeInfinity(ElaValue val)
        {
            return
                val.TypeCode == ElaTypeCode.Double ? Double.IsNegativeInfinity((Double)val.AsObject()) :
                val.TypeCode == ElaTypeCode.Single ? Single.IsNegativeInfinity((Single)val.AsObject()) :
                false;
        }
        
        public bool IsPositiveInfinity(ElaValue val)
        {
            return
                val.TypeCode == ElaTypeCode.Double ? Double.IsPositiveInfinity((Double)val.AsObject()) :
                val.TypeCode == ElaTypeCode.Single ? Single.IsPositiveInfinity((Single)val.AsObject()) :
                false;
        }
    }
}
