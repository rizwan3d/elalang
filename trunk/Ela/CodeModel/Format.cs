using System;
using System.Text;

namespace Ela.CodeModel
{
	internal struct Fmt
	{
		internal Fmt(int indent) : this(indent, FmtFlags.None)
		{
			
		}


		internal Fmt(int indent, FmtFlags flags)
		{
			Indent = indent;
			Flags = flags;
		}

		internal Fmt Add(FmtFlags flag)
		{
			return new Fmt(Indent, Flags | flag);
		}

		internal readonly int Indent;
		internal readonly FmtFlags Flags;
	}


	[Flags]
	internal enum FmtFlags
	{
		None = 0x00,
		NoParen = 0x001,
		Paren = 0x002
	}


	internal static class Format
	{
        private static readonly char[] ops = new char[] { '!', '%', '&', '*', '+', '-', '.', ':', '/', '<', '=', '>', '?', '@', '^', '|', '~' };
        
        public static bool IsSymbolic(string name)
        {
            return name.IndexOfAny(ops) != -1;
        }


		public static string OperatorAsString(ElaOperator op)
		{
			switch (op)
			{
				case ElaOperator.Assign: return "<-";
				case ElaOperator.BooleanAnd: return "&&";
				case ElaOperator.BooleanOr: return "||";
				case ElaOperator.Swap: return "<->";
				case ElaOperator.Sequence: return "$";
				default:
					return String.Empty;
			}
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
				p.Type == ElaNodeType.LazyLiteral ||
				p.Type == ElaNodeType.UnitLiteral;
		}


		public static bool IsHiddenVar(ElaExpression p)
		{
			return p != null && p.Type == ElaNodeType.VariableReference &&
				((ElaVariableReference)p).VariableName[0] == '$';
		}


		public static void PutInBraces(ElaExpression e, StringBuilder sb, Fmt fmt)
		{
			var simple = IsSimpleExpression(e);

			if (!simple)
			{
				sb.Append('(');
				e.ToString(sb, fmt.Add(FmtFlags.NoParen));
				sb.Append(')');
			}
			else
				e.ToString(sb, fmt);				
		}


		public static void PutInBraces(ElaPattern e, StringBuilder sb, Fmt fmt)
		{
			var complex = e.Type == ElaNodeType.HeadTailPattern ||
				e.Type == ElaNodeType.VariantPattern ||
				e.Type == ElaNodeType.AsPattern ||
				e.Type == ElaNodeType.IsPattern;

			if (complex)
			{
				sb.Append('(');
				e.ToString(sb, fmt.Add(FmtFlags.NoParen));
				sb.Append(')');
			}
			else
				e.ToString(sb, fmt);
		}
	}
}
