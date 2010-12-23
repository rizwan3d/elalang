using System;
using System.Collections.Generic;
using Ela.Parsing;
using System.Text;

namespace Ela.CodeModel
{
	public sealed class ElaBlock : ElaExpression
	{
		#region Construction
		internal ElaBlock(Token tok) : base(tok, ElaNodeType.Block)
		{
			
		}


		public ElaBlock() : base(ElaNodeType.Block)
		{
			
		}
		#endregion
				

		#region Methods
		public override string ToString()
		{
			var sb = new StringBuilder();

			foreach (var e in Expressions)
				sb.Append(e.ToString());

			return sb.ToString();
		}
		#endregion


		#region Properties
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


		public bool StartScope { get; set; }


		public bool IsEmpty
		{
			get { return _expressions == null; }
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
		#endregion
	}
}