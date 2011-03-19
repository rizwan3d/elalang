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

		TuplesLength = 804,

		IndexOutOfRange = 805,

		ExpectedFunction = 806,

		LeftOperand = 807,

		InvalidType = 808,

		MatchFailed = 809,

		RightOperand = 810,

		OperationNotSupported = 811,

		PrivateVariable = 812,

		TooManyParams = 813,

		TooFewParams = 814,

		InvalidFormat = 815,

		FieldImmutable = 816,

		UndefinedArgument = 817,

		UndefinedVariable = 818,

		UnknownField = 819,

		UnknownPervasive = 820,

		InvalidParameterType = 821,

		UnknownParameterType = 822,

		CallWithNoParams = 823,

		InvalidIndexType = 824,

		RefEqualsNotSupported = 825,

		TraitGet = 826,

		TraitSet = 827,

		TraitFieldGet = 828,

		TraitFieldSet = 829,

		TraitBit = 830,

		TraitConcat = 831,

		TraitNum = 832,

		TraitOrd = 833,

		TraitEq = 834,

		TraitTag = 835,

		TraitEnum = 836,

		TraitNeg = 837,

		TraitBool = 838,

		TraitLen = 839,

		TraitCont = 840,

		TraitInsert = 841,

		TraitCons = 842,

		TraitIter = 843,

		TraitConvert = 844,

		TraitShow = 845,

		TraitBound = 846,

		TraitFold = 847,

		TraitCall = 848,

		TraitThunk = 849,

		TraitGen = 850,


		UserCode = 999
	}
}
