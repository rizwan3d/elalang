using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaPatternGroup : ElaTuplePattern
	{
		#region Construction
		internal ElaPatternGroup(Token t) : base(t, ElaNodeType.PatternGroup)
		{

		}


		public ElaPatternGroup() : base(null, ElaNodeType.PatternGroup)
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
					sb.Append(' ');

				if (f.Type == ElaNodeType.VariantPattern || f.Type == ElaNodeType.HeadTailPattern)
					sb.Append(Format.PutInBraces(f));
				else
					f.ToString(sb);
			}
		}
		#endregion
	}
}
