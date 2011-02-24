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
                val.DataType == ObjectType.Double ? Double.IsInfinity((Double)val.AsObject()) :
                val.DataType == ObjectType.Single ? Single.IsInfinity((Single)val.AsObject()) :
                false;
        }


        public bool IsNan(ElaValue val)
        {
            return
                val.DataType == ObjectType.Double ? Double.IsNaN((Double)val.AsObject()) :
                val.DataType == ObjectType.Single ? Single.IsNaN((Single)val.AsObject()) :
                false;
        }


        public bool IsNegativeInfinity(ElaValue val)
        {
            return
                val.DataType == ObjectType.Double ? Double.IsNegativeInfinity((Double)val.AsObject()) :
                val.DataType == ObjectType.Single ? Single.IsNegativeInfinity((Single)val.AsObject()) :
                false;
        }


        public bool IsPositiveInfinity(ElaValue val)
        {
            return
                val.DataType == ObjectType.Double ? Double.IsPositiveInfinity((Double)val.AsObject()) :
                val.DataType == ObjectType.Single ? Single.IsPositiveInfinity((Single)val.AsObject()) :
                false;
        }
        #endregion
    }
}
