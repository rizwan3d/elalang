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
			Path = new List<String>();
		}


		public ElaModuleInclude() : this(null)
		{
			
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("open ");

			for (var i = 0; i < Path.Count; i++)
			{
				if (i > 0)
					sb.Append('.');

				sb.Append(Path[i]);
			}

			sb.Append(Name);

			if (!String.IsNullOrEmpty(Alias) && Alias != Name)
				sb.Append(" as " + Alias);

			if (!String.IsNullOrEmpty(DllName))
				sb.Append("[" + DllName + "]");

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

		public List<String> Path { get; private set; }

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