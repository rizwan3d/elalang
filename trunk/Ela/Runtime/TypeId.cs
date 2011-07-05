using System;

namespace Ela.Runtime
{
	public sealed class TypeId
	{
		#region Construction
		internal readonly int Id;

		internal TypeId(int typeId)
		{
			Id = typeId;
		}
		#endregion
	}
}
