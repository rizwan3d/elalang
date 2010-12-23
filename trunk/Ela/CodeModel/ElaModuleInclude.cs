using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaModuleInclude : ElaExpression
	{
		#region Construction
		internal ElaModuleInclude(Token tok) : base(tok, ElaNodeType.ModuleInclude)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}


		public ElaModuleInclude() : base(ElaNodeType.ModuleInclude)
		{
			Flags = ElaExpressionFlags.ReturnsUnit;
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("open ");
			sb.Append(Name);

			if (!String.IsNullOrEmpty(Alias) && Alias != Name)
				sb.Append(" as " + Alias);

			if (!String.IsNullOrEmpty(DllName))
				sb.Append("[" + DllName + "]");

			if (!String.IsNullOrEmpty(Folder))
				sb.Append(" at \"" + Folder + "\"");

			if (HasImports)
			{
				sb.Append(" with ");
				var c = 0;

				foreach (var i in _imports)
				{
					if (c++ > 0)
						sb.Append(',');

					sb.Append(i);
				}
			}

			sb.AppendLine();
			return sb.ToString();
		}
		#endregion


		#region Properties
		public string Name { get; set; }

		public string Alias { get; set; }

		public string DllName { get; set; }

		public string Folder { get; set; }

		private List<ElaImportedName> _imports;
		public List<ElaImportedName> Imports
		{
			get
			{
				if (_imports == null)
					_imports = new List<ElaImportedName>();

				return _imports;
			}
		}

		public bool HasImports { get { return _imports != null; } }
		#endregion
	}
}