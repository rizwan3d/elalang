using System;
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
