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

			if (pat.Type == ElaNodeType.IsPattern)
			{
				var @is = (ElaIsPattern)pat;
				return @is.TypeAffinity != TypeAffinity && @is.Traits != Traits;
			}

			return false;
		}
		#endregion


		#region Properties
		public ElaTypeCode TypeAffinity { get; set; }

		public ElaTraits Traits { get; set; }
		#endregion
	}
}
