using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaPatternGroup : ElaTuplePattern
	{
		internal ElaPatternGroup(Token t) : base(t, ElaNodeType.PatternGroup)
		{

		}
        
		public ElaPatternGroup() : base(null, ElaNodeType.PatternGroup)
		{

		}
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			var c = 0;

			foreach (var f in Patterns)
			{
				if (c++ > 0)
					sb.Append(' ');

				Format.PutInBraces(f, sb, fmt);
			}
		}
	}
}
