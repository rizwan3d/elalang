using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaIsPattern : ElaPattern
	{
		#region Construction
		internal ElaIsPattern(Token tok) : base(tok, ElaNodeType.IsPattern)
		{

		}


		internal ElaIsPattern() : base(ElaNodeType.IsPattern)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('?');
			sb.Append(Traits != ElaTraits.None ? FormatTraitList() : TypeCodeFormat.GetShortForm(TypeAffinity));
		}


		private string FormatTraitList()
		{
			var sb = new StringBuilder();
			sb.Append('(');
			sb.Append(Traits.ToString().Replace('|', ','));
			sb.Append(')');
			return sb.ToString();
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

			return (@is.TypeAffinity != TypeAffinity || @is.TypeAffinity == default(ElaTypeCode)) 
                && (@is.Traits == default(ElaTraits) || @is.Traits != Traits);
		}
		#endregion


		#region Properties
		public ElaTypeCode TypeAffinity { get; set; }

		public ElaTraits Traits { get; set; }
		#endregion
	}
}
