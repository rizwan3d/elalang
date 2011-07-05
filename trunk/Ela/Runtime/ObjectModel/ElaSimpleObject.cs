using System;

namespace Ela.Runtime.ObjectModel
{
	public abstract class ElaSimpleObject : ElaObject
	{
		#region Construction
		protected ElaSimpleObject() : base(ElaTypeCode.Object)
		{

		}
		#endregion
	}
}
