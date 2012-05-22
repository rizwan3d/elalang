using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaIndexer : ElaExpression
	{
		#region Construction
		internal ElaIndexer(Token tok) : base(tok, ElaNodeType.Indexer)
		{
			
		}


		public ElaIndexer() : this(null)
		{
			
		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			Format.PutInBraces(TargetObject, sb, fmt);
			sb.Append('.');
			sb.Append('[');
			Index.ToString(sb, fmt);
			sb.Append(']');
		}
		#endregion


		#region Properties
		public ElaExpression Index { get; set; }

		public ElaExpression TargetObject { get; set; }
		#endregion
	}
}