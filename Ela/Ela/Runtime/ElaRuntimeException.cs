using System;

namespace Ela.Runtime
{
	internal class ElaRuntimeException : ElaException
	{
		internal ElaRuntimeException(ElaRuntimeError error, params object[] arguments)
		{
            Error = error;
            Arguments = arguments;
		}

        internal ElaRuntimeError Error { get; set; }

        internal object[] Arguments { get; set; }
	}
}
