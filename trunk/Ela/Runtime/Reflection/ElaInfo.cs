using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public abstract class ElaInfo : ElaObject
	{
		#region Construction
		protected ElaInfo() : base(ObjectType.Object)
		{
			
		}
		#endregion
	}
}
