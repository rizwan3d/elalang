using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaMatch : ElaExpression
	{
		#region Construction
		internal ElaMatch(Token tok, ElaNodeType type) : base(tok, type)
		{
			Entries = new List<ElaMatchEntry>();
		}


		internal ElaMatch(Token tok) : this(tok, ElaNodeType.Match)
		{
			
		}


		public ElaMatch() : this(null, ElaNodeType.Match)
		{
			
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("match ");
			sb.Append(Expression);
			sb.AppendLine(" with");
			var c = 0;

			foreach (var p in Entries)
			{
				if (c++ > 0)
					sb.Append(";\r\n");

				sb.Append(p);
			}

			return sb.ToString();
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		
		public List<ElaMatchEntry> Entries { get; private set; }
		#endregion
	}
}