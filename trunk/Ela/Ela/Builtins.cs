using System;
using Ela.CodeModel;
using Ela.Compilation;

namespace Ela
{
    internal static class Builtins
    {
        internal static ElaBuiltinKind Kind(string func)
        {
            switch (func)
            {
                case "fpipe": return ElaBuiltinKind.ForwardPipe;
                case "bpipe": return ElaBuiltinKind.BackwardPipe;
                case "not": return ElaBuiltinKind.Not;
                case "flip": return ElaBuiltinKind.Flip;
                case "force": return ElaBuiltinKind.Force;
                case "length": return ElaBuiltinKind.Length;
                case "typeinfo": return ElaBuiltinKind.Type;
                case "succ": return ElaBuiltinKind.Succ;
                case "pred": return ElaBuiltinKind.Pred;
                case "nil": return ElaBuiltinKind.Nil;
                case "head": return ElaBuiltinKind.Head;
                case "tail": return ElaBuiltinKind.Tail;
                case "isnil": return ElaBuiltinKind.IsNil;
                case "gettag": return ElaBuiltinKind.Gettag;
                case "untag": return ElaBuiltinKind.Untag;
                
                case "showf": return ElaBuiltinKind.Showf;
                case "readf": return ElaBuiltinKind.Readf;

                case "recfield": return ElaBuiltinKind.RecField;
                case "cast": return ElaBuiltinKind.Cast;
                case "equal": return ElaBuiltinKind.Equal;
                case "notequal": return ElaBuiltinKind.NotEqual;
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
                case "bitwiseor": return ElaBuiltinKind.BitwiseOr;
                case "bitwisexor": return ElaBuiltinKind.BitwiseXor;
                case "shiftright": return ElaBuiltinKind.ShiftRight;
                case "shiftleft": return ElaBuiltinKind.ShiftLeft;
                case "bitwisenot": return ElaBuiltinKind.BitwiseNot;
                case "concat": return ElaBuiltinKind.Concat;
                case "cons": return ElaBuiltinKind.Cons;
                case "get": return ElaBuiltinKind.Get;

                default: return ElaBuiltinKind.None;
            }
        }
    }
}
