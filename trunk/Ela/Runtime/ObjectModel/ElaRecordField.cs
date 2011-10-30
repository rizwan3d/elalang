using System;

namespace Ela.Runtime.ObjectModel
{
	public struct ElaRecordField
	{
        public ElaRecordField(string field, ElaValue value)
        {
            Field = field;
            Value = value;
            Mutable = false;
        }

        public ElaRecordField(string field, object value)
        {
            Field = field;
            Value = ElaValue.FromObject(value);
            Mutable = false;
        }
        
        public ElaRecordField(string field, ElaValue value, bool mutable)
		{
			Field = field;
			Value = value;
			Mutable = mutable;
		}

		public ElaRecordField(string field, object value, bool mutable)
		{
			Field = field;
			Value = ElaValue.FromObject(value);
			Mutable = mutable;
		}

		public readonly string Field;
		public readonly ElaValue Value;
		public readonly bool Mutable;
	}
}
