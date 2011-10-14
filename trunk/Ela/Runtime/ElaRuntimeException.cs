using System;

namespace Ela.Runtime
{
	public class ElaRuntimeException : ElaException
	{
		#region Construction
		private const string DEFAULT = "Failure";

		public ElaRuntimeException(string message) : this(DEFAULT, message)
		{

		}


		public ElaRuntimeException(string category, string message) : base(message, null)
		{
			Category = category;
		}
		#endregion


		#region Properties
		public string Category { get; private set; }
		#endregion
	}
}
