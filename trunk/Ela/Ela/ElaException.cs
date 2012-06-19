using System;

namespace Ela
{
	public abstract class ElaException : Exception
	{
		protected ElaException(string message, Exception ex) : base(message, ex)
		{

		}
	}
}
