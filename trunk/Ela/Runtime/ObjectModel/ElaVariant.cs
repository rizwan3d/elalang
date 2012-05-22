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
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.Variant), (Int32)ElaTypeCode.Variant, true, typeof(ElaVariant));
		private static readonly ElaVariant noneVariant = new ElaVariant(NONE, new ElaValue(ElaUnit.Instance));

        public ElaVariant(string tag) : this(tag, new ElaValue(ElaUnit.Instance))
        {

        }


		public ElaVariant(string tag, ElaValue value) : base(ElaTypeCode.Variant)
		{
			Tag = tag;
			Value = value;
		}
		#endregion


		#region Methods
		public override ElaPatterns GetSupportedPatterns()
		{
			return ElaPatterns.Variant;
		}


		public override int GetHashCode()
		{
			return Tag.GetHashCode();
		}


		protected internal override int Compare(ElaValue @this, ElaValue other)
		{
			return @this.TypeId == other.TypeId ? ((ElaVariant)@this.Ref).Tag.CompareTo(((ElaVariant)other.Ref).Tag) : -1;
		}


		public override ElaTypeInfo GetTypeInfo()
		{
			return CreateTypeInfo(new ElaRecordField(TAG, Tag));
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


		#region Operations
        protected internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId != right.TypeId || left.TypeId != ElaMachine.VAR)
				return false;

			var v1 = (ElaVariant)left.Ref;
			var v2 = (ElaVariant)right.Ref;

			if (v1.Tag != v2.Tag)
				return false;
			
			return v1.Value.Equal(v1.Value, v2.Value, ctx);
		}


		protected internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.TypeId != right.TypeId || left.TypeId != ElaMachine.VAR)
				return true;

			var v1 = (ElaVariant)left.Ref;
			var v2 = (ElaVariant)right.Ref;

			if (v1.Tag != v2.Tag)
				return true;

			return v1.Value.NotEqual(v1.Value, v2.Value, ctx);
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
			return Tag + (Value.Ref != ElaUnit.Instance ? " " + Value.Ref.Show(Value, info, ctx) : String.Empty);
		}


        protected internal override ElaValue Convert(ElaValue @this, ElaTypeInfo type, ExecutionContext ctx)
		{
            if (type.ReflectedTypeCode == ElaTypeCode.Variant)
                return @this;
            else if (type.ReflectedTypeCode == ElaTypeCode.String)
                return new ElaValue(Tag + (Value.Ref != ElaUnit.Instance ? " " + Value.Convert(type, ctx) : String.Empty));

            ctx.ConversionFailed(@this, type.ReflectedTypeName);
			return Default();
		}


		private ElaValue InvalidOperand(ElaValue left, ElaValue right, string op, ExecutionContext ctx)
		{
			if (left.Ref == null)
				ctx.InvalidRightOperand(left, right, op);
			else
				ctx.InvalidRightOperand(left, right, op);

			return Default();
		}
		#endregion


        #region Properties
        public string Tag { get; protected set; }

		public ElaValue Value { get; protected set; }
		#endregion
	}
}