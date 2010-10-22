using System;
using Ela.Runtime;

namespace Ela.StandardLibrary
{
	internal static class ValueConverter
	{
		#region Methods
		internal static RuntimeValue ConvertToRuntimeValue(object value)
		{
			if (value is RuntimeValue)
				return (RuntimeValue)value;
			else
				return RuntimeValue.FromObject(value);
		}


		internal static T ConvertFromRuntimeValue<T>(RuntimeValue value)
		{
			try
			{
				if (typeof(T) == typeof(RuntimeValue))
					return (T)(Object)value;
				else
					return (T)value.ToObject();
			}
			catch (InvalidCastException)
			{
				throw new ElaParameterTypeException(value.DataType);
			}
		}
		#endregion
	}
}
