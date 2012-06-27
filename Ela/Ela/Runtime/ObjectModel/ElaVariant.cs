using System;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public class ElaVariant : ElaObject
	{
		private const string SOME = "Some";
        private const string NONE = "None";
		private const string LEFT = "Left";
		private const string RIGHT = "Right";
        private const string TAG = "type";
        private static readonly ElaVariant noneVariant = new ElaVariant(NONE, new ElaValue(ElaUnit.Instance));

        public ElaVariant(string tag) : this(tag, new ElaValue(ElaUnit.Instance))
        {

        }

		public ElaVariant(string tag, ElaValue value) : base(ElaTypeCode.Variant)
		{
			Tag = tag;
			Value = value;
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

        internal override string GetTag(ExecutionContext ctx)
		{
			return Tag;
		}

		internal override ElaValue Untag(ExecutionContext ctx)
        {
            return Value;
        }

        public override string ToString(string format, IFormatProvider provider)
        {
            if (Value.Ref is ElaUnit)
                return Tag;
            else
                return Tag + " " + Value.ToString(format, provider);
		}

        public string Tag { get; protected set; }

		public ElaValue Value { get; protected set; }
	}
}