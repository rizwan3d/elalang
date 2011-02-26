using System;
using Ela.Linking;
using Ela.Runtime;

namespace Ela.Library.General
{
    public sealed class RealModule : ForeignModule
    {
        #region Construction
        public RealModule()
        {

        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<ElaValue,Boolean>("inf", IsInfinity);
            Add<ElaValue,Boolean>("nan", IsNan);
            Add<ElaValue,Boolean>("posInf", IsPositiveInfinity);
            Add<ElaValue,Boolean>("negInf", IsNegativeInfinity);
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
        #endregion
    }
}
