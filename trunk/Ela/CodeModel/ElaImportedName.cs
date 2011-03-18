using System;
using Ela.Parsing;
using System.Text;

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


		#region Methods
		internal override void ToString(StringBuilder sb)
		{
			sb.Append(ExternalName == LocalName ? ExternalName : (ExternalName + " = " + LocalName));
		}
		#endregion


		#region Properties
		public string ExternalName { get; set; }

		public string LocalName { get; set; }
		#endregion
	}
}