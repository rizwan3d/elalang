using System;
using System.Collections.Generic;
using System.Text;
using Ela.Runtime;
using Ela.CodeModel;

namespace Ela.Linking
{
	internal sealed class TernaryOverload
	{
		internal TernaryOverload(DispatchTernaryFun fun, TypeId arg1, TypeId arg2, ElaTernaryFunction kind)
		{
			Function = fun;
			Arg1 = arg1;
			Arg2 = arg2;
			Kind = kind;
		}

		internal readonly DispatchTernaryFun Function;
		internal readonly TypeId Arg1;
		internal readonly TypeId Arg2;
		internal readonly ElaTernaryFunction Kind;
	}


	internal sealed class BinaryOverload
	{
		internal BinaryOverload(DispatchBinaryFun fun, TypeId arg1, TypeId arg2, ElaBinaryFunction kind)
		{
			Function = fun;
			Arg1 = arg1;
			Arg2 = arg2;
			Kind = kind;
		}

		internal readonly DispatchBinaryFun Function;
		internal readonly TypeId Arg1;
		internal readonly TypeId Arg2;
		internal readonly ElaBinaryFunction Kind;
	}


	internal sealed class UnaryOverload
	{
		internal UnaryOverload(DispatchUnaryFun fun, TypeId arg, ElaUnaryFunction kind)
		{
			Function = fun;
			Arg = arg;
			Kind = kind;
		}

		internal readonly DispatchUnaryFun Function;
		internal readonly TypeId Arg;
		internal readonly ElaUnaryFunction Kind;
	}


	public enum ElaBinaryFunction
	{
		Add,
		Subtract,
		Multiply,
		Divide,
		Remainder,
		Power,
		BitwiseAnd,
		BitwiseOr,
		BitwiseXor,
		ShiftLeft,
		ShiftRight,
		Equal,
		NotEqual,
		Greater,
		GreaterEqual,
		Lesser,
		LesserEqual,
		Concat,
		Cons,
		GetValue
	}


	public enum ElaUnaryFunction
	{
		Negate,
		BitwiseNot,
		Clone,
		Succ,
		Pred,
		Min,
		Max,
		Length,
		Nil,
		Head,
		Tail,
		IsNil,
	}


	public enum ElaTernaryFunction
	{
		SetValue
	}
}
