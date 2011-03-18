using System;
using System.Text;
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
		internal override void ToString(StringBuilder sb)
		{
			sb.Append(Name[0] == '$' ? String.Empty : Name);
		}
		#endregion


		#region Properties
		public string Name { get; set; }
		#endregion
	}
}
