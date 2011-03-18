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