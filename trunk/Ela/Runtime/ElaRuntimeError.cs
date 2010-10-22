using System;

namespace Ela.Runtime
{
	public enum ElaRuntimeError
	{
		None = 0,

		Unknown = 800,

		CallFailed = 801,

		ConversionFailed = 802,

		DivideByZero = 803,

		ExternalCallFailed = 804,

		IndexOutOfRange = 805,

		InvalidBinaryOperation = 806,

		InvalidUnaryOperation = 807,

		InvalidType = 808,

		MatchFailed = 809,

		NotFunction = 810,

		OperationNotSupported = 811,

		PrivateVariable = 812,

		TooManyParams = 813,

		TooFewParams = 814,

		UnableCallAsFunction = 815,

		UnableConvert = 816,

		UndefinedArgument = 817,

		UndefinedVariable = 818,

		UnknownField = 819,

		UnknownPervasive = 820,

		InvalidParameterType = 821,

		UnknownParameterType = 822,

		CallWithNoParams = 823
	}
}
