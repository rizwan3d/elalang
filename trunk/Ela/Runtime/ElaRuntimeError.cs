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

		// = 825,

		// = 826,

		// = 827,

		// = 828,

		// = 829,

		// = 830,

		// = 831,

		// = 832,

		// = 833,

		// = 834,

		// = 835,

		// = 836,

		// = 837,

		// = 838,

		// = 839,

		// = 840,

		// = 841,

		// = 842,

		// = 843,

		// = 844,

		// = 845,

		// = 846,

		// = 847,

		// = 848,

		// = 849,

		// = 850,

		// = 851,

		// = 852,

		// = 853,

		// = 854,

		// = 855,

		// = 856,

		// = 857,

		// = 858,

		// = 859,

		// = 860,

		// = 861,

		// = 862,

		// = 863,

		TraitGet = 900,

		TraitSet = 901,

		TraitFieldGet = 902,

		TraitFieldSet = 903,

		TraitBit = 904,

		TraitConcat = 905,

		TraitNum = 906,

		TraitOrd = 907,

		TraitEq = 908,

		TraitTag = 909,

		TraitEnum = 910,

		TraitNeg = 911,

		TraitBool = 912,

		TraitLen = 913,

		TraitCont = 914,

		TraitInsert = 915,

		TraitCons = 916,

		TraitIter = 917,

		TraitConvert = 918,

		TraitShow = 919,

		TraitBound = 920,

		TraitFold = 921,

		TraitCall = 922,

		TraitThunk = 923,

		TraitGen = 924,


		UserCode = 999
	}
}
