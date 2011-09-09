using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaPrimitive : ElaExpression
	{
		#region Construction
		internal ElaPrimitive(Token tok) : base(tok, ElaNodeType.Primitive)
		{
			
		}


		public ElaPrimitive() : base(ElaNodeType.Primitive)
		{
			
		}
		#endregion
		

		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append(Value);
		}
		#endregion


		#region Properties
		public ElaLiteralValue Value { get; set; }
		#endregion
	}
}