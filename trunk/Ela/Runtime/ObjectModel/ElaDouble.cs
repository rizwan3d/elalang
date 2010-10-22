using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaDouble : ElaObject
	{
		#region Construction
		public ElaDouble(double value) : base(ObjectType.Double)
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
		public static implicit operator ElaDouble(double val)
		{
			return new ElaDouble(val);
		}


		public static implicit operator double(ElaDouble val)
		{
			return val.Value; 
		}
		#endregion


		#region Properties
		public double Value { get; private set; }
		#endregion
	}
}