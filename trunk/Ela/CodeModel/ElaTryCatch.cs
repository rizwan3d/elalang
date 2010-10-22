using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaTryCatch : ElaExpression
	{
		#region Construction
		internal ElaTryCatch(Token tok) : base(tok, ElaNodeType.TryCatch)
		{

		}


		public ElaTryCatch() : base(ElaNodeType.TryCatch)
		{

		}
		#endregion


		#region Properties
		public ElaExpression Try { get; set; }

		public ElaExpression Catch { get; set; }

		public ElaVariableReference Variable { get; set; }
		#endregion
	}
}