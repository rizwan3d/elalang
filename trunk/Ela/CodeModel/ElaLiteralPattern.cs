using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaLiteralPattern : ElaPattern
	{
		#region Construction
		internal ElaLiteralPattern(Token tok) : base(tok, ElaNodeType.LiteralPattern)
		{

		}


		internal ElaLiteralPattern() : base(ElaNodeType.LiteralPattern)
		{

		}
		#endregion
		

		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			sb.Append(Value.ToString());
		}
		#endregion


		#region Properties
		public ElaLiteralValue Value { get; set; }
		
		internal override ElaPatternAffinity Affinity
		{
			get 
			{
				switch (Value.LiteralType)
				{
					case ElaTypeCode.Integer: 
					case ElaTypeCode.Long: return ElaPatternAffinity.Integer;
					case ElaTypeCode.Single: 
					case ElaTypeCode.Double: return ElaPatternAffinity.Real;
					case ElaTypeCode.Char: return ElaPatternAffinity.Char;
					case ElaTypeCode.String: return ElaPatternAffinity.String|ElaPatternAffinity.Sequence|ElaPatternAffinity.Fold;
					case ElaTypeCode.Boolean: return ElaPatternAffinity.Boolean;
					default: return ElaPatternAffinity.Any;
				}
			}
		}
		#endregion
	}
}
