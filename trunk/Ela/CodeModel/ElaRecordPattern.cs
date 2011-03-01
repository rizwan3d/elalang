using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaRecordPattern : ElaPattern
	{
		#region Construction
		internal ElaRecordPattern(Token tok) : base(tok, ElaNodeType.RecordPattern)
		{
			Fields = new List<ElaFieldPattern>();
		}


		public ElaRecordPattern() : this(null)
		{

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
		public List<ElaFieldPattern> Fields { get; private set; }
		
		internal override ElaPatternAffinity Affinity
		{
			get { return ElaPatternAffinity.Sequence|ElaPatternAffinity.Any; }
		}
		#endregion
	}
}