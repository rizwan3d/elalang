using System;

namespace Ela
{
	public class ElaFatalException : ElaException
	{
		#region Construction
		internal ElaFatalException(string message) : base(message, null)
		{

		}


		internal ElaFatalException(string message, Exception ex) : base(message, ex)
		{

		}
		#endregion
	}
}
