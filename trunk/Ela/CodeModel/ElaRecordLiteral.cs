using System;
using System.Collections.Generic;
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

		
		#region Properties
		public List<ElaFieldDeclaration> Fields { get; private set; }
		#endregion
	}
}