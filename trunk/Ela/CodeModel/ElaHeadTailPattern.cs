using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaHeadTailPattern : ElaTuplePattern
	{
		#region Construction
		internal ElaHeadTailPattern(Token tok) : base(tok, ElaNodeType.HeadTailPattern)
		{
			
		}


		public ElaHeadTailPattern() : this(null)
		{
			
		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			var c = 0;

			foreach (var f in Patterns)
			{
				if (c++ > 0)
					sb.Append("::");

				if (f.Type == ElaNodeType.HeadTailPattern)
                    sb.Append(Format.PutInBraces(f));
				else
					sb.Append(f.ToString());
			}
		}
		#endregion


		#region Properties
		internal override ElaPatternAffinity Affinity
		{
			get { return ElaPatternAffinity.Fold|ElaPatternAffinity.Any; }
		}
		#endregion
	}
}
