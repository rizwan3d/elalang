using System;

namespace Ela.Runtime.ObjectModel
{
	public struct ElaRecordField
	{
		public ElaRecordField(string field, RuntimeValue value)
		{
			Field = field;
			Value = value;
		}

		public ElaRecordField(string field, object value)
		{
			Field = field;
			Value = RuntimeValue.FromObject(value);
		}

		internal readonly string Field;
		internal readonly RuntimeValue Value;
	}
}
