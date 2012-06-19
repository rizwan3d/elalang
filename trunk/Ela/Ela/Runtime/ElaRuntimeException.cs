using System;

namespace Ela.Runtime
{
	public class ElaRuntimeException : ElaException
	{
		private const string DEFAULT = "Failure";

		public ElaRuntimeException(string message) : this(DEFAULT, message)
		{

		}
        
		public ElaRuntimeException(string category, string message) : base(message, null)
		{
			Category = category;
		}
		
        public string Category { get; private set; }
	}
}
