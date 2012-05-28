using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaImportedVariable : ElaExpression
	{
		#region Construction
		internal ElaImportedVariable(Token tok) : base(tok, ElaNodeType.ImportedVariable)
		{
			
		}


		public ElaImportedVariable() : this(null)
		{

		}
		#endregion


		#region Methods
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			if (Private)
				sb.Append("private ");

			if (Name == LocalName)
				sb.Append(Name);
			else
				sb.AppendFormat("{0}={1}", LocalName, Name);
		}
		#endregion


		#region Properties
		public string Name { get; set; }

		public string LocalName { get; set; }

		public bool Private { get; set; }
		#endregion
	}
}