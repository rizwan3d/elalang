﻿using System;
using Ela.CodeModel;

namespace Ela
{
	internal static class Builtins
	{
		internal static ElaBuiltinFunctionKind Kind(string func)
		{
			switch (func)
			{
				case "cout": return ElaBuiltinFunctionKind.Cout;
				case "typeid": return ElaBuiltinFunctionKind.Typeid;
				case "not": return ElaBuiltinFunctionKind.Not;
				case "flip": return ElaBuiltinFunctionKind.Flip;
				case "force": return ElaBuiltinFunctionKind.Force;
				case "length": return ElaBuiltinFunctionKind.Length;
				
				case "type": return ElaBuiltinFunctionKind.Type;
				case "succ": return ElaBuiltinFunctionKind.Succ;
				case "pred": return ElaBuiltinFunctionKind.Pred;
				case "max": return ElaBuiltinFunctionKind.Max;
				case "min": return ElaBuiltinFunctionKind.Min;
				
				case "show": return ElaBuiltinFunctionKind.Show;
				case "showf": return ElaBuiltinFunctionKind.Showf;
				default: return ElaBuiltinFunctionKind.None;
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
				case "Fold": return ElaTraits.Fold;
				case "Gen": return ElaTraits.Gen;
				case "Get": return ElaTraits.Get;
				case "Len": return ElaTraits.Len;
				case "Neg": return ElaTraits.Neg;
				case "Num": return ElaTraits.Num;
				case "Ord": return ElaTraits.Ord;
				case "Set": return ElaTraits.Set;
				case "Show": return ElaTraits.Show;
				case "Thunk": return ElaTraits.Thunk;
				default: return ElaTraits.None;
			}
		}
	}
}