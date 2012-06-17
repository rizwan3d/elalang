using System;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaBoolean : ElaObject
	{
		internal static readonly ElaBoolean Instance = new ElaBoolean();
        
		private ElaBoolean() : base(ElaTypeCode.Boolean)
		{

		}
	}
}