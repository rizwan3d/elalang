using System;
using Ela.CodeModel;
using Ela.Compilation;

namespace Ela
{
	internal static class Builtins
	{
		internal static int Params(ElaBuiltinKind kind)
		{
			return kind >= ElaBuiltinKind.Showf ? 2 : 1;
		}


		internal static ElaBuiltinKind Kind(string func)
		{
			switch (func)
			{
				case "typeid": return ElaBuiltinKind.Typeid;
				case "not": return ElaBuiltinKind.Not;
				case "flip": return ElaBuiltinKind.Flip;
				case "force": return ElaBuiltinKind.Force;
				case "length": return ElaBuiltinKind.Length;
				case "type": return ElaBuiltinKind.Type;
				case "succ": return ElaBuiltinKind.Succ;
				case "pred": return ElaBuiltinKind.Pred;
				case "max": return ElaBuiltinKind.Max;
				case "min": return ElaBuiltinKind.Min;
				case "show": return ElaBuiltinKind.Show;
				case "showf": return ElaBuiltinKind.Showf;
				case "isref": return ElaBuiltinKind.IsRef;
				case "nil": return ElaBuiltinKind.Nil;

				case "equals": return ElaBuiltinKind.Equals;
				case "notequals": return ElaBuiltinKind.NotEquals;
				case "greaterequal": return ElaBuiltinKind.GreaterEqual;
				case "lesserequal": return ElaBuiltinKind.LesserEqual;
				case "greater": return ElaBuiltinKind.Greater;
				case "lesser": return ElaBuiltinKind.Lesser;
				case "add": return ElaBuiltinKind.Add;
				case "subtract": return ElaBuiltinKind.Subtract;
				case "multiply": return ElaBuiltinKind.Multiply;
				case "divide": return ElaBuiltinKind.Divide;
				case "remainder": return ElaBuiltinKind.Remainder;
				case "power": return ElaBuiltinKind.Power;
				case "negate": return ElaBuiltinKind.Negate;
				case "bitwiseand": return ElaBuiltinKind.BitwiseAnd;
				case "birwiseor": return ElaBuiltinKind.BitwiseOr;
				case "bitwisexor": return ElaBuiltinKind.BitwiseXor;
				case "shiftright": return ElaBuiltinKind.ShiftRight;
				case "shiftleft": return ElaBuiltinKind.ShiftLeft;
				case "bitwisenot": return ElaBuiltinKind.BitwiseNot;
				case "concat": return ElaBuiltinKind.Concat;
				case "cons": return ElaBuiltinKind.Cons;
				case "compforward": return ElaBuiltinKind.CompForward;
				case "compbackward": return ElaBuiltinKind.CompBackward;

				default: return ElaBuiltinKind.None;
			}
		}


		internal static ElaTraits Trait(string trait)
		{
			switch (trait)
			{
				case "Bit": return ElaTraits.Bit;
				case "Bool": return ElaTraits.Bool;
				case "Bound": return ElaTraits.Bound;
				case "Call": return ElaTraits.Call;
				case "Concat": return ElaTraits.Concat;
				case "Cons": return ElaTraits.Cons;
				case "Convert": return ElaTraits.Convert;
				case "Enum": return ElaTraits.Enum;
				case "Eq": return ElaTraits.Eq;
				case "FieldGet": return ElaTraits.FieldGet;
				case "FieldSet": return ElaTraits.FieldSet;
				case "Seq": return ElaTraits.Seq;
				case "Gen": return ElaTraits.Gen;
				case "Get": return ElaTraits.Get;
				case "Len": return ElaTraits.Len;
				case "Neg": return ElaTraits.Neg;
				case "Num": return ElaTraits.Num;
				case "Ord": return ElaTraits.Ord;
                case "Ix": return ElaTraits.Ix;
				case "Set": return ElaTraits.Set;
				case "Show": return ElaTraits.Show;
				case "Thunk": return ElaTraits.Thunk;
                case "Tag": return ElaTraits.Tag;
                case "Real": return ElaTraits.Real;
                case "Int": return ElaTraits.Int;
				default: return ElaTraits.None;
			}
		}
	}
}
