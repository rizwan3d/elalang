using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaEquation : ElaExpression, IEnumerable<ElaEquation>
	{
        internal ElaEquation(Token tok, ElaNodeType type) : base(tok, type)
		{
			
		}


		internal ElaEquation(Token tok) : base(tok, ElaNodeType.Equation)
		{
			
		}
        
		public ElaEquation() : this(null)
		{
			
		}

        internal override bool Safe()
        {
            return Right == null ? Left.Safe() : Right.Safe();
        }
		
        internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			ToString(sb, fmt, "let");
		}
        
		internal void ToString(StringBuilder sb, Fmt fmt, string keyword)
		{
            if (fmt.Indent > 0)
                sb.Append(' ', fmt.Indent);

            var ln = sb.Length;

            if (!Left.Parens)
                Left.ToString(sb, fmt);
            else
            {
                sb.Append('(');
                Left.ToString(sb, fmt);
                sb.Append(')');
            }

            ln = sb.Length - ln;

            if (Right != null)
            {
                sb.Append(" = ");
                Right.ToString(sb, fmt);
            }
		}

        internal bool IsFunction()
        {
            return !Left.Parens && Left.Type == ElaNodeType.Juxtaposition;
        }

        internal string GetFunctionName()
        {
            return ((ElaJuxtaposition)Left).Target.GetName();
        }

        internal string GetLeftName()
        {
            return IsFunction() ? GetFunctionName() : Left.GetName();
        }

        internal int GetArgumentNumber()
        {
            return IsFunction() ? ((ElaJuxtaposition)Left).Parameters.Count : 0;
        }

        public ElaVariableFlags VariableFlags { get; set; }

        public string AssociatedType { get; set; }

        public ElaExpression Left { get; set; }

        public ElaExpression Right { get; set; }

        public ElaEquation Next { get; set; }

        public IEnumerator<ElaEquation> GetEnumerator()
        {
            var b = this;

            while (b != null)
            {
                yield return b;
                b = b.Next;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}