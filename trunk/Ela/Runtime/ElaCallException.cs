using System;

namespace Ela.Runtime
{
	public class ElaCallException : ElaException
	{
		#region Construction
		public ElaCallException(string message) : base(message, null)
		{
			
		}
		#endregion


		#region Properties
		public virtual ElaRuntimeError Error
		{
			get { return ElaRuntimeError.CallFailed; }
		}
		#endregion
	}
}
