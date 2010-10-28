using System;

namespace Ela.Runtime
{
	public sealed class ElaCastException : ElaCallException
	{
		#region Construction
		public ElaCastException(ObjectType expectedType, ObjectType invalidType) : base(String.Empty)
		{
			ExpectedType = expectedType;
			InvalidType = invalidType;
		}
		#endregion


		#region Properties
		public ObjectType InvalidType { get; set; }

		public ObjectType ExpectedType { get; set; }
		#endregion
	}
}
