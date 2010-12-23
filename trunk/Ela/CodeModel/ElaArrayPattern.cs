using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaArrayPattern : ElaTuplePattern
	{
		#region Construction
		internal ElaArrayPattern(Token tok) : base(tok, ElaNodeType.ArrayLiteral)
		{
			
		}


		public ElaArrayPattern() : this(null)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("[|");

			var c = 0;

			foreach (var p in base.Patterns)
			{
				if (c++ > 0)
					sb.Append(", ");

				sb.Append(p.ToString());
			}
			
			sb.Append("|]");
			return sb.ToString();
		}
		#endregion


		#region Properties
		internal override ElaPatternAffinity Affinity
		{
			get { return ElaPatternAffinity.Array; }
		}
		#endregion
	}
}