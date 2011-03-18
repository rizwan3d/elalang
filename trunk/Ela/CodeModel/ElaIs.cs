using System;
using System.Text;
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
		internal override void ToString(StringBuilder sb)
		{
			var str = Expression + (Pattern.Type == ElaNodeType.IsPattern ? String.Empty :  " is ") + Format.PutInBracesComplex(Pattern);
			sb.Append(str);
		}
		#endregion


		#region Properties
		public ElaPattern Pattern { get; set; }

		public ElaExpression Expression { get; set; }
		#endregion
	}
}