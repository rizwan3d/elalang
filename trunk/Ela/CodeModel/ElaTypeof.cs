using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaTypeof : ElaExpression
	{
		#region Construction
		internal ElaTypeof(Token t) : base(t, ElaNodeType.Typeof)
		{

		}


		public ElaTypeof() : base(ElaNodeType.Typeof)
		{

		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }
		
		public override int Placeholders
		{
			get { return Expression.Placeholders; }
		}
		#endregion
	}
}