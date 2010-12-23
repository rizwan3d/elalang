using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaCastPattern : ElaPattern
	{
		#region Construction
		internal ElaCastPattern(Token tok) : base(tok, ElaNodeType.CastPattern)
		{

		}


		internal ElaCastPattern() : base(ElaNodeType.CastPattern)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return VariableName + "?" + TypeAffinity.GetShortForm();
		}
		#endregion


		#region Properties
		public ObjectType TypeAffinity { get; set; }

		public string VariableName { get; set; }
		#endregion
	}
}
