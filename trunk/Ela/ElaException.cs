using System;

namespace Ela
{
	public abstract class ElaException : Exception
	{
		#region Construction
		protected ElaException(string message, Exception ex)
			: base(message, ex)
		{

		}
		#endregion
	}
}
