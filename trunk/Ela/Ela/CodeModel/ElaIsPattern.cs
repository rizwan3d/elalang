using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaIsPattern : ElaPattern
	{
		internal ElaIsPattern(Token tok) : base(tok, ElaNodeType.IsPattern)
		{

		}
        
		internal ElaIsPattern() : base(ElaNodeType.IsPattern)
		{

		}
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('?');
			sb.Append(TypeName);
		}
        
		internal override bool CanFollow(ElaPattern pat)
		{
			if (pat.IsIrrefutable())
				return false;

            var @is = default(ElaIsPattern);

            if (pat.Type == ElaNodeType.IsPattern)
                @is = (ElaIsPattern)pat;
            else if (pat.Type == ElaNodeType.AsPattern)
            {
                var @as = (ElaAsPattern)pat;

                if (@as.Pattern.Type == ElaNodeType.IsPattern)
                    @is = (ElaIsPattern)@as.Pattern;
                else
                    return true;
            }
            else if (pat.Type == ElaNodeType.VariantPattern)
            {
                var var = (ElaVariantPattern)pat;

                if (var.Pattern != null && var.Pattern.Type == ElaNodeType.IsPattern)
                    @is = (ElaIsPattern)var.Pattern;
                else
                    return true;
            }
            else
                return true;

			return @is.TypeName != TypeName;
		}

		public string TypeName { get; set; }

        public string TypePrefix { get; set; }

        private List<TraitInfo> _traits;
        public List<TraitInfo> Traits
        {
            get { return _traits ?? (_traits = new List<TraitInfo>()); }
        }
	}
}
