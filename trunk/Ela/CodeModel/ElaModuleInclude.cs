using System;
using System.Collections.Generic;
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