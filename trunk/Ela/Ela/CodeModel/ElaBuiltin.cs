using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaBuiltin : ElaExpression
	{
		#region Construction
		internal ElaBuiltin(Token tok) : base(tok, ElaNodeType.Builtin)
		{

		}


		public ElaBuiltin() : base(ElaNodeType.Builtin)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append("__internal ");
			sb.Append(Kind.ToString().ToLower());
		}
		#endregion


		#region Properties
		public ElaBuiltinKind Kind { get; set; }
		#endregion
	}
}