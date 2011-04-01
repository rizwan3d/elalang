using System;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
    public sealed class GuidModule : ForeignModule
    {
        #region Construction
        private static readonly ElaGuid emptyGuid = new ElaGuid(Guid.Empty);

        public GuidModule()
        {

        }
        #endregion


        #region Nested Classes
        public sealed class ElaGuid : ElaObject
        {
            #region Construction
            internal ElaGuid(Guid value)
            {
                Value = value;
            }
            #endregion


            #region Methods
            public override ElaPatterns GetSupportedPatterns()
            {
                return ElaPatterns.None;
            }


            protected override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
            {
                var lg = left.As<ElaGuid>();
                var rg = right.As<ElaGuid>();

                if (lg != null && rg != null)
                    return new ElaValue(lg.Value == rg.Value);
                else if (lg != null)
                    return right.Equals(left, right, ctx);
                else
                {
                    ctx.InvalidLeftOperand(left, right, "equal");
                    return Default();
                }
            }


            protected override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
            {
                var lg = left.As<ElaGuid>();
                var rg = right.As<ElaGuid>();

                if (lg != null && rg != null)
                    return new ElaValue(lg.Value != rg.Value);
                else if (lg != null)
                    return right.NotEquals(left, right, ctx);
                else
                {
                    ctx.InvalidLeftOperand(left, right, "notequal");
                    return Default();
                }
            }


            protected override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
            {
                return !String.IsNullOrEmpty(info.Format) ? Value.ToString(info.Format) :
                    Value.ToString();
            }


            protected override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
            {
                if (type == ElaTypeCode.String)
                    return new ElaValue(Value.ToString());
                else
                {
                    ctx.ConversionFailed(new ElaValue(this), type);
                    return Default();
                }
            }
            #endregion


            #region Properties
            internal Guid Value { get; set; }
            #endregion
        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add("empty", emptyGuid);
            Add<ElaGuid>("guid", NewGuid);
            Add<String,ElaVariant>("parse", Parse);
        }


        public ElaGuid NewGuid()
        {
            return new ElaGuid(Guid.NewGuid());
        }


        public ElaVariant Parse(string str)
        {
            var g = Guid.Empty;

            try
            {
                g = new Guid(str);
                return ElaVariant.Some(g);
            }
            catch (Exception)
            {
                return ElaVariant.None();
            }
        }
        #endregion
    }
}
