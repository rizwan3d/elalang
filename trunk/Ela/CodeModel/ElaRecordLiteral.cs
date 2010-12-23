using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaRecordLiteral : ElaExpression
	{
		#region Construction
		internal ElaRecordLiteral(Token tok) : this(tok, ElaNodeType.RecordLiteral)
		{
			
		}


		internal ElaRecordLiteral(Token tok, ElaNodeType type) : base(tok, type)
		{
			Fields = new List<ElaFieldDeclaration>();
		}


		public ElaRecordLiteral() : this(ElaNodeType.RecordLiteral)
		{
			
		}


		protected ElaRecordLiteral(ElaNodeType type) : base(type)
		{
			Fields = new List<ElaFieldDeclaration>();
		}
		#endregion
		

		#region Methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append('{');
			var c = 0;

			foreach (var f in Fields)
			{
				if (c++ > 0)
					sb.Append(", ");

				sb.Append(f.ToString());
			}

			sb.Append('}');
			return sb.ToString();
		}
		#endregion

		
		#region Properties
		public List<ElaFieldDeclaration> Fields { get; private set; }
		#endregion
	}
}