using System;
using Ela.CodeModel;
using Ela.Compilation;

namespace Ela
{
	internal static class Builtins
	{
		internal static void Kind(string func, out ElaBuiltinFunctionKind kind, out ElaOperator op)
		{
			op = ElaOperator.None;
			kind = ElaBuiltinFunctionKind.None;

			switch (func)
			{
				case "typeid": kind = ElaBuiltinFunctionKind.Typeid; break;
				case "not": kind = ElaBuiltinFunctionKind.Not; break;
				case "flip": kind = ElaBuiltinFunctionKind.Flip; break;
				case "force": kind = ElaBuiltinFunctionKind.Force; break;
				case "length": kind = ElaBuiltinFunctionKind.Length; break;
				case "type": kind = ElaBuiltinFunctionKind.Type; break;
				case "succ": kind = ElaBuiltinFunctionKind.Succ; break;
				case "pred": kind = ElaBuiltinFunctionKind.Pred; break;
				case "max": kind = ElaBuiltinFunctionKind.Max; break;
				case "min": kind = ElaBuiltinFunctionKind.Min; break;
				case "show": kind = ElaBuiltinFunctionKind.Show; break;
				case "showf": kind = ElaBuiltinFunctionKind.Showf; break;
				case "ref": kind = ElaBuiltinFunctionKind.Ref; break;
				case "nil": kind = ElaBuiltinFunctionKind.Nil; break;

				case "==": op = ElaOperator.Equals; break;
				case "<>": op = ElaOperator.NotEquals; break; 
				case ">=": op = ElaOperator.GreaterEqual; break; 
				case "<=": op = ElaOperator.LesserEqual; break; 
				case ">": op = ElaOperator.Greater; break; 
				case "<": op = ElaOperator.Lesser; break;
				case "+": op = ElaOperator.Add; break; 
				case "-": op = ElaOperator.Subtract; break; 
				case "*": op = ElaOperator.Multiply; break; 
				case "/": op = ElaOperator.Divide; break; 
				case "%": op = ElaOperator.Remainder; break; 
				case "**": op = ElaOperator.Power; break;
				case "--": op = ElaOperator.Negate; break; 
				case "&&&": op = ElaOperator.BitwiseAnd; break; 
				case "|||": op = ElaOperator.BitwiseOr; break; 
				case "^^^": op = ElaOperator.BitwiseOr; break; 
				case ">>>": op = ElaOperator.ShiftRight; break; 
				case "<<<": op = ElaOperator.ShiftLeft; break; 
				case "~~~": op = ElaOperator.BitwiseNot; break;
				case "++": op = ElaOperator.Concat; break; 
				case "::": op = ElaOperator.Cons; break;
				case ">>": op = ElaOperator.CompForward; break;
				case "<<": op = ElaOperator.CompBackward; break;
			}

			if (op != ElaOperator.None)
				kind = ElaBuiltinFunctionKind.Operator;
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
