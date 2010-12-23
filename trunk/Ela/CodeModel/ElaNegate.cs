using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaNegate : ElaExpression
	{
		#region Construction
		internal ElaNegate(Token tok) : base(tok, ElaNodeType.Negate)
		{
			
		}


		public ElaNegate() : this(null)
		{

		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		#endregion
	}
}