using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariablePattern : ElaPattern
	{
		#region Construction
		internal ElaVariablePattern(Token tok) : base(tok, ElaNodeType.VariablePattern)
		{

		}


		public ElaVariablePattern() : base(ElaNodeType.VariablePattern)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return Name[0] == '$' ? String.Empty : Name;
		}
		#endregion


		#region Properties
		public string Name { get; set; }
		#endregion
	}
}
