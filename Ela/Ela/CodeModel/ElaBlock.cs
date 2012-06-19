using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBlock : ElaExpression
	{
		internal ElaBlock(Token tok) : base(tok, ElaNodeType.Block)
		{
			
		}
        
		public ElaBlock() : base(ElaNodeType.Block)
		{
			
		}
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			foreach (var e in Expressions)
			{
				if (e.Type == ElaNodeType.Binding)
					sb.AppendLine();

				sb.AppendLine(e.ToString());
			}
		}
		
		private List<ElaExpression> _expressions;
		public List<ElaExpression> Expressions
		{
			get
			{
				if (_expressions == null)
					_expressions = new List<ElaExpression>();

				return _expressions;
			}
		}
		
		public ElaExpression LastExpression
		{
			get
			{
				if (_expressions != null && _expressions.Count > 0)
				{
					var last = _expressions[_expressions.Count - 1];
					return last.Type == ElaNodeType.Block ? ((ElaBlock)last).LastExpression : last;
				}
				else
					return null;
			}
		}
	}
}