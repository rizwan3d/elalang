using System;
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


		#region Properties
		public ElaLiteralValue Value { get; set; }
		
		internal override ElaPatternAffinity Affinity
		{
			get 
			{
				switch (Value.LiteralType)
				{
					case ObjectType.Integer: 
					case ObjectType.Long: return ElaPatternAffinity.Integer;
					case ObjectType.Single: 
					case ObjectType.Double: return ElaPatternAffinity.Float;
					case ObjectType.Char: return ElaPatternAffinity.Char;
					case ObjectType.String: return ElaPatternAffinity.String;
					case ObjectType.Boolean: return ElaPatternAffinity.Boolean;
					default: return ElaPatternAffinity.Any;
				}
			}
		}
		#endregion
	}
}
