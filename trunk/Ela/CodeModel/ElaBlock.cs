using System;
using System.Collections.Generic;
using Ela.Parsing;

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


		public bool IsEmpty
		{
			get { return _expressions == null; }
		}
				

		public ElaExpression LastExpression
		{
			get
			{
				return _expressions != null && _expressions.Count > 0 ?
					_expressions[_expressions.Count - 1] : null;
			}
		}
		#endregion
	}
}