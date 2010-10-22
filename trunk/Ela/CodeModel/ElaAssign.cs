using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaAssign : ElaExpression
	{
		#region Construction
		internal ElaAssign(Token tok) : base(tok, ElaNodeType.Assign)
		{
			
		}


		public ElaAssign() : base(ElaNodeType.Assign)
		{
			
		}
		#endregion


		#region Properties
		public ElaExpression Left { get; set; }

		public ElaExpression Right { get; set; }

		public override int Placeholders { get { return Left.Placeholders + Right.Placeholders; } }
		#endregion
	}
}