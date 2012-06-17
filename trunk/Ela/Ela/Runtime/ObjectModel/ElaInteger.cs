using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaInteger : ElaObject
	{
		internal static readonly ElaInteger Instance = new ElaInteger();
		
		private ElaInteger() : base(ElaTypeCode.Integer)
		{
			
		}
	}
}