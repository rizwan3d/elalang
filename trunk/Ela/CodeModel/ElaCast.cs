using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaCast : ElaExpression
	{
		#region Construction
		internal ElaCast(Token t) : base(t, ElaNodeType.Cast)
		{

		}


		public ElaCast() : base(ElaNodeType.Cast)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
            var str = 
				(Format.IsSimpleExpression(Expression) ? Expression.ToString() : Format.PutInBraces(Expression)) + 
				":" + TypeCodeFormat.GetShortForm(CastAffinity);
			sb.Append(str);
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }

		public ElaTypeCode CastAffinity { get; set; }
		#endregion
	}
}
