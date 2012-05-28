using System;

namespace Ela.Runtime
{
	public sealed class ElaMachineException : ElaException
	{
		#region Construction
		internal ElaMachineException(string message, Exception ex) : base(message, ex)
		{

		}


		internal ElaMachineException(string message) : base(message, null)
		{

		}
		#endregion
	}
}
