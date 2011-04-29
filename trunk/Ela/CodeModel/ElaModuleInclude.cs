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
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append("open ");

			for (var i = 0; i < Path.Count; i++)
			{
				if (i > 0)
					sb.Append('.');

				sb.Append(Path[i]);
			}

			sb.Append(Name);

			if (!String.IsNullOrEmpty(Alias) && Alias != Name)
			{
				sb.Append('@');
				sb.Append(Alias);
			}

			if (!String.IsNullOrEmpty(DllName))
			{
				sb.Append('#');
				sb.Append(DllName);
			}
		}
		#endregion


		#region Properties
		public string Name { get; set; }

		public string Alias { get; set; }

		public string DllName { get; set; }

        public bool RequireQuailified { get; set; }

		public List<String> Path { get; private set; }		
		#endregion
	}
}