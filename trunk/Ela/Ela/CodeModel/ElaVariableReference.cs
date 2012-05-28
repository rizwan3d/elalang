using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariableReference : ElaExpression
	{
		#region Construction
		internal ElaVariableReference(Token tok) : base(tok, ElaNodeType.VariableReference)
		{
			
		}


		public ElaVariableReference() : this(null)
		{
			
		}
		#endregion


		#region Methods
		internal override string GetName()
		{
			return VariableName;
		}


		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			if (VariableName[0] != '$')
				sb.Append(VariableName);
		}
		#endregion


		#region Properties
		public string VariableName { get; set; }
		#endregion
	}
}