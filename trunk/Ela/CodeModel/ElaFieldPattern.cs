using System;
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
		public override string ToString()
		{
			return Name + "=" + Value.ToString();
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