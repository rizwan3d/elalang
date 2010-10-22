using System;
using System.Collections.Generic;

namespace Ela.CodeModel
{
	public sealed class ElaCodeUnit
	{
		#region Construction
		public ElaCodeUnit()
		{
			Expressions = new List<ElaExpression>();
		}
		#endregion


		#region Properties
		public List<ElaExpression> Expressions { get; private set; }
		#endregion
	}
}
