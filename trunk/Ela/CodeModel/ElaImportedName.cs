using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaImportedName : ElaExpression
	{
		#region Construction
		internal ElaImportedName(Token tok) : base(tok, ElaNodeType.ImportedName)
		{
			
		}


		public ElaImportedName() : base(ElaNodeType.ImportedName)
		{
			
		}
		#endregion


		#region Properties
		public string ExternalName { get; set; }

		public string LocalName { get; set; }
		#endregion
	}
}