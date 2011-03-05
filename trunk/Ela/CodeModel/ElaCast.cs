using System;
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
		public override string ToString()
		{
            return (Format.IsSimpleExpression(Expression) ? Expression.ToString() : Format.PutInBraces(Expression)) +
				":" + TypeCodeFormat.GetShortForm(CastAffinity);
		}
		#endregion


		#region Properties
		public ElaExpression Expression { get; set; }

		public ElaTypeCode CastAffinity { get; set; }
		#endregion
	}
}
