using System;
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
		public override string ToString()
		{
			return Value.ToString();
		}
		#endregion


		#region Properties
		public ElaLiteralValue Value { get; set; }
		#endregion
	}
}