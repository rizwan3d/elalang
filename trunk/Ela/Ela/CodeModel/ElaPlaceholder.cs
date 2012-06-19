﻿using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaPlaceholder : ElaExpression
	{
		internal ElaPlaceholder(Token tok) : base(tok, ElaNodeType.Placeholder)
		{
			
		}

		public ElaPlaceholder() : this(null)
		{
			
		}

        internal override bool Safe()
        {
            return true;
        }

		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('_');
		}
	}
}