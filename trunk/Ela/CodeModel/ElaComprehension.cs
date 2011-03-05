using System;
using System.Collections.Generic;
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
				Initial.Type == ElaNodeType.ListLiteral ? "[{0}]" :
				Initial.ToString() + " @@ " + "[{0}]";

            return String.Format(format, Format.ForToStringAsComprehension(Generator));
		}
		#endregion


		#region Properties
		public ElaFor Generator { get; set; }

		public ElaExpression Initial { get; set; }

		public bool Lazy { get; set; }
		#endregion
	}
}
