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


		#region Properties
		public ElaExpression Expression { get; set; }

		public ObjectType CastAffinity { get; set; }
		
		public override int Placeholders { get { return Expression.Placeholders; } }
		#endregion
	}
}
