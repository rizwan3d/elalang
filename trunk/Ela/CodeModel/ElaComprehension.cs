using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaComprehension : ElaExpression
	{
		#region Construction
		internal ElaComprehension(Token tok) : base(tok, ElaNodeType.Comprehension)
		{

		}


		public ElaComprehension() : base(null, ElaNodeType.Comprehension)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			var format = Lazy ? "[& {0}]" :
				Initial.Type == ElaNodeType.ArrayLiteral ? "[|{0}|]" : "[{0}]";

			return String.Format(format, Generator.ToStringAsComprehension());
		}
		#endregion


		#region Properties
		public ElaFor Generator { get; set; }

		public ElaExpression Initial { get; set; }

		public bool Lazy { get; set; }
		#endregion
	}
}
