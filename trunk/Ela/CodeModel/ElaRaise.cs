﻿using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaRaise : ElaExpression
	{
		#region Construction
		internal ElaRaise(Token tok) : base(tok, ElaNodeType.Raise)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaRaise() : this(null)
		{
			
		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			if (ErrorCode == "Failure")
			{
				sb.Append("fail ");
				Expression.ToString(sb);					
			}
			else
			{
				sb.Append("raise ");
				sb.Append(ErrorCode);
			
				if (Expression != null)
				{
					sb.Append('(');
					Expression.ToString(sb);
					sb.Append(')');
				}
			}
		}
		#endregion


		#region Properties
		public string ErrorCode { get; set; }

		public ElaExpression Expression { get; set; }	
		#endregion
	}
}