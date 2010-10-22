using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLong : ElaObject
	{
		#region Construction
		public ElaLong(long value) : base(ObjectType.Long)
		{
			Value = value;
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return Value.ToString();
		}
		#endregion


		#region Operators
		public static implicit operator ElaLong(long val)
		{
			return new ElaLong(val);
		}


		public static implicit operator long(ElaLong val)
		{
			return val.Value;
		}
		#endregion


		#region Properties
		public long Value { get; private set; }
		#endregion
	}
}