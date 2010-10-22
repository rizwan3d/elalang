using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaIs : ElaExpression
	{
		#region Construction
		internal ElaIs(Token t) : base(t, ElaNodeType.Is)
		{

		}


		public ElaIs() : base(ElaNodeType.Is)
		{

		}
		#endregion


		#region Properties
		public ElaPattern Pattern { get; set; }

		public ElaExpression Expression { get; set; }

		public override int Placeholders
		{
			get { return Expression.Placeholders; }
		}
		#endregion
	}
}