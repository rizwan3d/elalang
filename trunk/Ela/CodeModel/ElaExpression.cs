using System;
using Ela.Parsing;
using System.Text;

namespace Ela.CodeModel
{
	public abstract class ElaExpression
	{
		#region Construction
		protected ElaExpression(ElaNodeType type) : this(null, type)
		{

		}


		internal ElaExpression(Token tok, ElaNodeType type)
		{
			Type = type;

			if (tok != null)
			{
				Line = tok.line;
				Column = tok.col;
			}
		}
		#endregion


		#region Methods
		public void SetLinePragma(int line, int column)
		{
			Line = line;
			Column = column;
		}


		internal virtual string GetName()
		{
			return null;
		}


		public override string ToString()
		{
			var sb = new StringBuilder();
			ToString(sb);
			return sb.ToString();
		}


		internal abstract void ToString(StringBuilder sb);
		#endregion


		#region Properties
		public int Line { get; private set; }

		public int Column { get; private set; }
		
		public ElaExpressionFlags Flags { get; internal protected set; }
		
		public ElaNodeType Type { get; protected set; }
		#endregion
	}
}