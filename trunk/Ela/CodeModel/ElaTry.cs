using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaTry : ElaMatch
	{
		#region Construction
		internal ElaTry(Token tok) : base(tok, ElaNodeType.Try)
		{

		}


		public ElaTry() : base(null, ElaNodeType.Try)
		{

		}
		#endregion
		
		
		#region Methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("try ");
			sb.Append(Expression);
			sb.AppendLine(" with");
			var c = 0;

			foreach (var p in Entries)
			{
				if (c++ > 0)
					sb.Append(";\r\n");

				sb.Append(p);
			}

			sb.Append(" end");
			return sb.ToString();
		}
		#endregion
	}
}