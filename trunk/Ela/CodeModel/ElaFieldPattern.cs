using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFieldPattern : ElaPattern
	{
		#region Construction
		internal ElaFieldPattern(Token tok) : base(tok, ElaNodeType.FieldPattern)
		{

		}


		public ElaFieldPattern() : base(ElaNodeType.FieldPattern)
		{

		}
		#endregion


		#region Methods
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
		#endregion


		#region Properties
		public string Name { get; set; }

		public ElaPattern Value { get; set; }

		internal override ElaPatternAffinity Affinity
		{
			get { return ElaPatternAffinity.Any; }
		}
		#endregion
	}
}