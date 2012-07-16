using System;

namespace Ela.Runtime
{
	public class ElaRuntimeException : ElaException
	{
		public ElaRuntimeException(string message) : base(message, null)
		{

		}
	}
}
