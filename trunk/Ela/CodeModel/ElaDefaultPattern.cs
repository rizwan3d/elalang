using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaDefaultPattern : ElaPattern
	{
		#region Construction
		internal ElaDefaultPattern(Token tok) : base(tok, ElaNodeType.DefaultPattern)
		{

		}


		public ElaDefaultPattern() : base(ElaNodeType.DefaultPattern)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			sb.Append('_');
		}
		#endregion
	}
}
