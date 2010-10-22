using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaFunctionCall : ElaExpression
	{
		#region Construction
		internal ElaFunctionCall(Token tok) : base(tok, ElaNodeType.FunctionCall)
		{
			Parameters = new List<ElaExpression>();
		}


		public ElaFunctionCall() : this(null)
		{
			
		}
		#endregion


		#region Properties
		public ElaExpression Target { get; set; }

		public List<ElaExpression> Parameters { get; private set; }

		public bool ConvertParametersToTuple { get; set; }

		public override int Placeholders 
		{ 
			get 
			{
				var len = Parameters.Count;

				if (len == 0)
					return Target.Placeholders;
				else
				{
					var cumul = 0;

					for (var i = 0; i < len; i++)
					{
						var p = Parameters[i];
						cumul += p.Placeholders;
					}

					return cumul;
				}
			} 
		}
		#endregion
	}

}