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


		#region Methods
		public override string ToString()
		{
			return Expression + (Pattern.Type == ElaNodeType.IsPattern ? String.Empty :  " ? ") + 
				Pattern.PutInBracesComplex();
		}
		#endregion


		#region Properties
		public ElaPattern Pattern { get; set; }

		public ElaExpression Expression { get; set; }
		#endregion
	}
}