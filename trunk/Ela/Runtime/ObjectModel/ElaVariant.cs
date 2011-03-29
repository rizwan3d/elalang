using System;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public class ElaVariant : ElaObject
	{
		#region Construction
        private const string SOME = "Some";
        private const string NONE = "None";
		private const string LEFT = "Left";
		private const string RIGHT = "Right";
        private const string TAG = "tag";
		private static readonly ElaVariant noneVariant = new ElaVariant(NONE, new ElaValue(ElaUnit.Instance));

        public ElaVariant(string tag) : this(tag, new ElaValue(ElaUnit.Instance))
        {

        }


		public ElaVariant(string tag, ElaValue value) : base(ElaTypeCode.Variant, ElaTraits.Tag)
		{
			Tag = tag;
			Value = value;
		}
		#endregion


		#region Traits
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.Ref == right.Ref);
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.Ref != right.Ref);
		}


		protected internal override string GetTag(ExecutionContext ctx)
		{
			return Tag;
		}


        protected internal override ElaValue Untag(ExecutionContext ctx)
        {
            return Value;
        }


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return "`" + Tag + (Value.Ref != ElaUnit.Instance ? " " + Value.Ref.Show(Value, info, ctx) : String.Empty);
		}


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			if (type == ElaTypeCode.Variant)
				return new ElaValue(this);

			ctx.ConversionFailed(new ElaValue(this), type);
			return Default();
		}
		#endregion


        #region Methods
        public override ElaTypeInfo GetTypeInfo()
        {
            var info = base.GetTypeInfo();
            info.AddField(TAG, Tag);
            return info;
        }


        public static ElaVariant Some(ElaValue value)
        {
            return new ElaVariant(SOME, value);
        }


        public static ElaVariant Some<T>(T value)
        {
            return new ElaVariant(SOME, ElaValue.FromObject(value));
        }


        public static ElaVariant None()
        {
            return noneVariant;
        }


		public static ElaVariant Left(ElaValue value)
		{
			return new ElaVariant(LEFT, value);
		}


		public static ElaVariant Left<T>(T value)
		{
			return new ElaVariant(LEFT, ElaValue.FromObject(value));
		}


		public static ElaVariant Right(ElaValue value)
		{
			return new ElaVariant(RIGHT, value);
		}


		public static ElaVariant Right<T>(T value)
		{
			return new ElaVariant(RIGHT, ElaValue.FromObject(value));
		}
        #endregion


        #region Properties
        public string Tag { get; protected set; }

		public ElaValue Value { get; protected set; }
		#endregion
	}
}