﻿using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFor : ElaExpression
	{
		#region Construction
		internal ElaFor(Token tok) : base(tok, ElaNodeType.For)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaFor() : this(null)
		{
			
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("for ");

            sb.Append(Format.PutInBracesComplex(Pattern));

			if (Guard != null)
                sb.Append(Format.ExpressionToStringAsGuard(Guard));

			if (InitExpression != null)
			{
				sb.Append(" = ");
				sb.Append(InitExpression.ToString());
			}

			if (ForType == ElaForType.ForTo)
				sb.Append(" to ");
			else if (ForType == ElaForType.ForDownto)
				sb.Append(" downto ");
			else
				sb.Append(" in ");

			sb.Append(Target.ToString());
			sb.Append(" do ");
			sb.Append(Body.ToString());
			return sb.ToString();
		}
		#endregion


		#region Properties
		public ElaPattern Pattern { get; set; }

		public ElaExpression Guard { get; set; }

		public ElaExpression Target { get; set; }

		public ElaExpression Body { get; set; }

		public ElaExpression InitExpression { get; set; }

		public ElaForType ForType { get; set; }
		#endregion
	}
}