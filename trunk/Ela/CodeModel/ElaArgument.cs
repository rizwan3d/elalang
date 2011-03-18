using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaArgument : ElaExpression
	{
		#region Construction
		internal ElaArgument(Token tok) : base(tok, ElaNodeType.Argument)
		{
			
		}
		

		public ElaArgument() : base(ElaNodeType.Argument)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('$');
			sb.Append(ArgumentName);
		}
		#endregion


		#region Properties
		public string ArgumentName { get; set; }
		#endregion
	}
}