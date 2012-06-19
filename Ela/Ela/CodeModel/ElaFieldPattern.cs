using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFieldPattern : ElaPattern
	{
		internal ElaFieldPattern(Token tok) : base(tok, ElaNodeType.FieldPattern)
		{

		}
        
		public ElaFieldPattern() : base(ElaNodeType.FieldPattern)
		{

		}
		
		internal override void ToString(StringBuilder sb, Fmt fmt)		
		{
			if (Value.GetName() == Name)
				sb.Append(Name);
			else
			{
				sb.Append(Name);
				sb.Append('=');
				Value.ToString(sb, fmt);
			}
		}
        
		internal override bool CanFollow(ElaPattern pat)
		{
			if (pat.IsIrrefutable())
				return false;

			if (pat.Type == ElaNodeType.FieldPattern)
			{
				var fld = (ElaFieldPattern)pat;
				return Name != fld.Name || Value.CanFollow(fld.Value);
			}

			return true;
		}
		
		public string Name { get; set; }

		public ElaPattern Value { get; set; }
	}
}