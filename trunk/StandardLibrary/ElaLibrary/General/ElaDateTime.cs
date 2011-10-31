using System;
using System.Globalization;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
    public sealed class ElaDateTime : ElaObject
    {
        #region Construction
        private const string TYPE_NAME = "dateTime";
		internal const string DEFAULT_FORMAT = "dd/MM/yyyy HH:mm:ss";
		internal static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-US");

        public ElaDateTime(DateTime dateTime) : this(dateTime.ToUniversalTime().Ticks)
        {
            
        }


        internal ElaDateTime(long ticks)
        {
            Ticks = ticks;
        }
        #endregion


        #region Nested Types
        private enum Comparison
        {
            Eq,
            Neq,
            Gt,
            Lt,
            GtEq,
            LtEq
        }
        #endregion


        #region Methods
        protected override string GetTypeName()
        {
            return TYPE_NAME;
        }


        protected override int Compare(ElaValue @this, ElaValue other)
        {
            var otherInt = default(Int64);

            if (other.TypeCode == ElaTypeCode.Integer)
                otherInt = other.AsInteger();
            else if (other.TypeCode == ElaTypeCode.Long)
                otherInt = other.As<ElaLong>().Value;
            else if (other.Is<ElaDateTime>())
                otherInt = other.As<ElaDateTime>().Ticks;
            else
                return -1;

            return (Int32)(Ticks - otherInt);
        }


        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }


        protected override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Compare(left, right, Comparison.Eq, ctx);   
        }


        protected override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Compare(left, right, Comparison.Neq, ctx);
        }


        protected override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Compare(left, right, Comparison.Gt, ctx);
        }


        protected override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Compare(left, right, Comparison.Lt, ctx);
        }


        protected override ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Compare(left, right, Comparison.GtEq, ctx);
        }


        protected override ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return Compare(left, right, Comparison.LtEq, ctx);
        }


        protected override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
        {
            var format = info.Format ?? DEFAULT_FORMAT;
            return new DateTime(Ticks).ToString(format, Culture.DateTimeFormat);
        }


        protected override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
        {
            if (type.ReflectedTypeCode == ElaTypeCode.Long)
                return new ElaValue(Ticks);

            ctx.ConversionFailed(@this, type.ReflectedTypeName);
            return Default();
        }


        protected override ElaValue GetMax(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDateTime(DateTime.MaxValue));
        }


		protected override ElaValue GetMin(ElaValue @this, ExecutionContext ctx)
        {
            return new ElaValue(new ElaDateTime(DateTime.MinValue));
        }


        private ElaValue Compare(ElaValue left, ElaValue right, Comparison comp, ExecutionContext ctx)
        {
            var ld = left.As<ElaDateTime>();
            var rd = right.As<ElaDateTime>();

            if (ld == null)
            {
                ctx.InvalidLeftOperand(left, right, Op(comp));
                return Default();
            }

            if (rd == null)
            {
                ctx.InvalidRightOperand(left, right, Op(comp));
                return Default();
            }

            switch (comp)
            {
                case Comparison.Eq: return new ElaValue(ld.Ticks == rd.Ticks);
                case Comparison.Neq: return new ElaValue(ld.Ticks != rd.Ticks);
                case Comparison.Gt: return new ElaValue(ld.Ticks > rd.Ticks);
                case Comparison.Lt: return new ElaValue(ld.Ticks < rd.Ticks);
                case Comparison.GtEq: return new ElaValue(ld.Ticks >= rd.Ticks);
                case Comparison.LtEq: return new ElaValue(ld.Ticks <= rd.Ticks);
                default: return Default();
            }
        }


        private string Op(Comparison comp)
        {
            switch (comp)
            {
                case Comparison.Eq: return "equal";
                case Comparison.Neq: return "notequal";
                case Comparison.Gt: return "greater";
                case Comparison.Lt: return "lesser";
                case Comparison.GtEq: return "greaterequal";
                case Comparison.LtEq: return "lesserequal";
                default: return String.Empty;
            }
        }
        #endregion


        #region Properties
        internal long Ticks { get; private set; }
        #endregion
    }
}
