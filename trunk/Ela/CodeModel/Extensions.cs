using System;
using System.Text;

namespace Ela.CodeModel
{
	internal static class Extensions
	{
		public static string AsString(this ElaOperator op)
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
				default:
					return String.Empty;
			}
		}


		public static string AsString(this ElaUnaryOperator op)
		{
			return op == ElaUnaryOperator.BitwiseNot ? "~~~" :
				op == ElaUnaryOperator.Negate ? "--" :
				String.Empty;
		}


		public static string ToStringAsComprehension(this ElaFor @for)
		{
			var sb = new StringBuilder();
			var sel = GetSelect(@for, sb);
			return sel.ToString() + " @ " + sb.ToString();
		}


		public static string ToStringAsGuard(this ElaExpression p)
		{
			return " | " + p;
		}


		private static ElaExpression GetSelect(ElaFor @for, StringBuilder sb)
		{
			sb.Append(@for.Pattern.ToString());
			sb.Append(" <- ");
			sb.Append(@for.Target.ToString());

			if (@for.Guard != null)
				sb.Append(@for.Guard.ToStringAsGuard());
			
			if (@for.Body.Type == ElaNodeType.For)
			{
				sb.Append(", ");
				return GetSelect((ElaFor)@for.Body, sb);
			}
			else
				return @for.Body;
		}


		public static string ToStringAsFunction(this ElaFunctionLiteral fun)
		{
			var sb = new StringBuilder();

			if (!String.IsNullOrEmpty(fun.Name))
				sb.Append(fun.Name + " ");

			var c = 0;

			foreach (var p in fun.Body.Entries)
			{
				if (c++ > 0)
					sb.Append(";\r\n");

				sb.Append(p.ToString());
			}

			return sb.ToString();
		}


		public static string ToStringAsWhere(this ElaBinding bind)
		{
			var sb = new StringBuilder();
			sb.AppendLine();
			sb.Append(" where ");
			sb.Append(bind.ToString(true));

			if (bind.And != null ||
				(bind.InitExpression.Type == ElaNodeType.FunctionLiteral &&
				((ElaFunctionLiteral)bind.InitExpression).Body.Entries.Count > 1))
				sb.Append(" end");

			return sb.ToString();
		}


		public static string ToStringAsFuncPattern(this ElaPattern pat)
		{
			if (pat == null)
				return String.Empty;
			else if (pat.Type == ElaNodeType.VariantPattern ||
				pat.Type == ElaNodeType.HeadTailPattern)
				return "(" + pat.ToString() + ")";
			else
				return pat.ToString();
		}


		public static bool IsSimpleExpression(this ElaExpression p)
		{
			return p.Type == ElaNodeType.VariableReference ||
				p.Type == ElaNodeType.Primitive ||
				p.Type == ElaNodeType.ListLiteral ||
				p.Type == ElaNodeType.RecordLiteral ||
				p.Type == ElaNodeType.TupleLiteral ||
				p.Type == ElaNodeType.Argument ||
				p.Type == ElaNodeType.BaseReference ||
				p.Type == ElaNodeType.Match ||
				p.Type == ElaNodeType.Try ||
				p.Type == ElaNodeType.LazyLiteral ||
				p.Type == ElaNodeType.BuiltinFunction ||
				p.Type == ElaNodeType.UnitLiteral;
		}


		public static bool IsHiddenVar(this ElaExpression p)
		{
			return p.Type == ElaNodeType.VariableReference &&
				((ElaVariableReference)p).VariableName[0] == '$';
		}


		public static string PutInBracesComplex(this ElaExpression p)
		{
			return p.IsSimpleExpression() ? p.ToString() : p.ToString().PutInBraces();
		}


		public static string PutInBracesComplex(this ElaPattern p)
		{
			var comp = p.Type == ElaNodeType.HeadTailPattern ||
				p.Type == ElaNodeType.VariantPattern;
			return !comp ? p.ToString() : p.ToString().PutInBraces();
		}



		public static string PutInBraces(this ElaExpression p)
		{
			return p.ToString().PutInBraces();
		}


		public static string PutInBraces(this string expStr)
		{
			return expStr[0] == '(' ? expStr : "(" + expStr + ")";
		}
	}
}
