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
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			sb.Append('{');
			var c = 0;

			foreach (var f in Fields)
			{
				if (c++ > 0)
					sb.Append(',');

				f.ToString(sb, fmt);
			}

			sb.Append('}');
		}


		internal override bool CanFollow(ElaPattern pat)
		{
			if (pat.IsIrrefutable())
				return false;

			if (pat.Type == ElaNodeType.RecordPattern)
			{
				var rec = (ElaRecordPattern)pat;

				if (rec.Fields.Count != Fields.Count)
					return true;

				for (var i = 0; i < rec.Fields.Count; i++)
					if (Fields[i].CanFollow(rec.Fields[i]))
						return true;

				return false;
			}

			return true;
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