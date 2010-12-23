using System;
using Ela.CodeModel;

namespace Ela
{
	internal static class BuiltinFunc
	{
		internal static ElaBuiltinFunctionKind Kind(string func)
		{
			switch (func)
			{
				case "cout": return ElaBuiltinFunctionKind.Cout;
				case "typeid": return ElaBuiltinFunctionKind.Typeid;
				case "ignore": return ElaBuiltinFunctionKind.Ignore;
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
	}
}
