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
		public override string ToString()
		{
			var sb = new StringBuilder();
			var c = 0;

			foreach (var f in Patterns)
			{
				if (c++ > 0)
					sb.Append(" ");

				if (f.Type == ElaNodeType.VariantPattern || f.Type == ElaNodeType.HeadTailPattern)
                    sb.Append(Format.PutInBraces(f.ToString()));
				else
					sb.Append(f.ToString());
			}

			return sb.ToString();
		}
		#endregion
	}
}
