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

            return String.Format(format, Generator.ToString());
		}
		#endregion


		#region Properties
		public ElaGenerator Generator { get; set; }

		public ElaExpression Initial { get; set; }

		public bool Lazy { get; set; }
		
		public ElaPattern Pattern { get; set; }

		public ElaExpression Guard { get; set; }

		public ElaExpression Target { get; set; }

		public ElaExpression Body { get; set; }

		public ElaExpression InitExpression { get; set; }
		#endregion
	}
}
