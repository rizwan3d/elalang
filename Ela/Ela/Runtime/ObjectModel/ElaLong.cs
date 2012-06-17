using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLong : ElaObject
	{
		public ElaLong(long value) : base(ElaTypeCode.Long)
		{
			Value = value;
		}
		
        internal override long AsLong()
        {
            return Value;
        }
        
        public override string ToString(string format, IFormatProvider provider)
        {
            return Value.ToString(format, provider);
        }

		public long Value { get; private set; }
	}
}