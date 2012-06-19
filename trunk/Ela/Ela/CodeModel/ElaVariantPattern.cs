﻿using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariantPattern : ElaPattern
	{
		internal ElaVariantPattern(Token tok) : base(tok, ElaNodeType.VariantPattern)
		{

		}
        
		public ElaVariantPattern() : base(ElaNodeType.VariantPattern)
		{

		}
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append(Tag);

			if (Pattern != null)
			{
				sb.Append(' ');
				Format.PutInBraces(Pattern, sb, fmt);
			}
		}
        
		internal override bool CanFollow(ElaPattern pat)
		{
			if (pat.IsIrrefutable())
				return false;

			if (pat.Type == ElaNodeType.VariantPattern)
			{
				var var = (ElaVariantPattern)pat;

				if (var.Tag != Tag)
					return true;
				else if (var.Pattern == null)
					return false;
				else if (Pattern != null)
					return Pattern.CanFollow(var.Pattern);
			}

			return true;
		}
		
		public string Tag { get; set; }

		public ElaPattern Pattern { get; set; }
	}
}
