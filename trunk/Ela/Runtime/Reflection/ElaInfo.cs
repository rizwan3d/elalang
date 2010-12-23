using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public abstract class ElaInfo : ElaRecord
	{
		#region Construction
		protected ElaInfo(int size) : base(size)
		{
			
		}
		#endregion
	}
}