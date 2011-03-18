using System;
using System.Text;

namespace Ela.CodeModel
{
	internal static class Format
	{
		public static string OperatorAsString(ElaOperator op)
		{
			switch (op)
			{
				case ElaOperator.Add: return "+";
				case ElaOperator.Assign: return "<-";
				case ElaOperator.BitwiseAnd: return "&&&";
				case ElaOperator.BitwiseOr: return "|||";
				case ElaOperator.BitwiseXor: return "^^^";
				case ElaOperator.BooleanAnd: return "&&";
				case ElaOperator.BooleanOr: return "||";
				case ElaOperator.CompBackward: return "<<";
				case ElaOperator.CompForward: return ">>";
				case ElaOperator.Concat: return "++";
				case ElaOperator.ConsList: return "::";
				case ElaOperator.Divide: return "/";
				case ElaOperator.Equals: return "==";
				case ElaOperator.Greater: return ">";
				case ElaOperator.GreaterEqual: return ">=";
				case ElaOperator.Lesser: return "<";
				case ElaOperator.LesserEqual: return "<=";
				case ElaOperator.Modulus: return "%";
				case ElaOperator.Multiply: return "*";
				case ElaOperator.NotEquals: return "<>";
				case ElaOperator.Power: return "**";
				case ElaOperator.ShiftLeft: return "<<<";
				case ElaOperator.ShiftRight: return ">>>";
				case ElaOperator.Subtract: return "-";
				case ElaOperator.Swap: return "<->";
				case ElaOperator.Sequence: return "$";
                case ElaOperator.Negate: return "--";
                case ElaOperator.BitwiseNot: return "~~~";
				default:
					return String.Empty;
			}
		}


		public static string PatternToStringAsFuncPattern(ElaPattern pat)
		{
			if (pat == null)
				return String.Empty;
			else if (pat.Type == ElaNodeType.VariantPattern ||
				pat.Type == ElaNodeType.HeadTailPattern)
				return "(" + pat.ToString() + ")";
			else
				return pat.ToString();
		}


		public static bool IsSimpleExpression(ElaExpression p)
		{
			return 
				p == null ||
				p.Type == ElaNodeType.VariableReference ||
				p.Type == ElaNodeType.Primitive ||
				p.Type == ElaNodeType.ListLiteral ||
				p.Type == ElaNodeType.RecordLiteral ||
				p.Type == ElaNodeType.TupleLiteral ||
				p.Type == ElaNodeType.Argument ||
				p.Type == ElaNodeType.BaseReference ||
				p.Type == ElaNodeType.LazyLiteral ||
				p.Type == ElaNodeType.BuiltinFunction ||
				p.Type == ElaNodeType.UnitLiteral;
		}


		public static bool IsHiddenVar(ElaExpression p)
		{
			return p != null && p.Type == ElaNodeType.VariableReference &&
				((ElaVariableReference)p).VariableName[0] == '$';
		}


		public static string PutInBracesComplex(ElaExpression p)
		{
            return IsSimpleExpression(p) ? 
				(p != null ? p.ToString() : String.Empty) : 
				PutInBraces(p.ToString());
		}


		public static string PutInBracesComplex(ElaPattern p)
		{
			var comp = p.Type == ElaNodeType.HeadTailPattern ||
				p.Type == ElaNodeType.VariantPattern;
            return !comp ? p.ToString() : PutInBraces(p.ToString());
		}



		public static string PutInBraces(ElaExpression p)
		{
            return PutInBraces(p.ToString());
		}


		public static string PutInBraces(string expStr)
		{
			return expStr[0] == '(' ? expStr : "(" + expStr + ")";
		}
	}
}
