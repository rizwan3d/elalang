using System;
using System.Collections.Generic;

namespace Ela
{
	public abstract class TranslationResult
	{
		#region Construction
		protected TranslationResult(bool success, IEnumerable<ElaMessage> messages)
		{
			Success = success;
			Messages = messages;
		}
		#endregion


		#region Properties
		public bool Success { get; private set; }

		public IEnumerable<ElaMessage> Messages { get; private set; }
		#endregion
	}
}
