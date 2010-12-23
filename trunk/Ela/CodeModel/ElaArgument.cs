using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaArgument : ElaExpression
	{
		#region Construction
		internal ElaArgument(Token tok) : base(tok, ElaNodeType.Argument)
		{
			
		}
		

		public ElaArgument() : base(ElaNodeType.Argument)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return "$" + ArgumentName;
		}
		#endregion


		#region Properties
		public string ArgumentName { get; set; }
		#endregion
	}
}