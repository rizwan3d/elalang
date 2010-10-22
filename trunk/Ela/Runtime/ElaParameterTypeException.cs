using System;

namespace Ela.Runtime
{
	public sealed class ElaParameterTypeException : ElaCallException
	{
		#region Construction
		private ElaRuntimeError error;

		public ElaParameterTypeException(ObjectType invalidType) :
			base(Strings.GetError(ElaRuntimeError.UnknownParameterType, invalidType.GetShortForm()))
		{
			error = ElaRuntimeError.UnknownParameterType;
		}


		public ElaParameterTypeException(ObjectType expectedType, ObjectType invalidType) :
			base(Strings.GetError(ElaRuntimeError.InvalidParameterType,
				expectedType.GetShortForm(), invalidType.GetShortForm()))
		{
			error = ElaRuntimeError.InvalidParameterType;
		}
		#endregion


		#region Properties
		public override ElaRuntimeError Error
		{
			get { return error; }
		}
		#endregion
	}
}
