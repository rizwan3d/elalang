using System;
using System.Collections.Generic;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaSeqPattern : ElaPattern
	{
		#region Construction
		internal ElaSeqPattern(Token tok) : base(tok, ElaNodeType.SeqPattern)
		{
			
		}


		internal ElaSeqPattern(Token tok, ElaNodeType type) : base(tok, type)
		{
			
		}


		internal ElaSeqPattern(Token tok, List<ElaPattern> patterns, ElaNodeType type) : base(tok, type)
		{
			_patterns = patterns;
		}


		public ElaSeqPattern() : this(null)
		{
			
		}
		#endregion


		#region Methods
		internal bool IsRecord()
		{
			var isRec = true;

			if (_patterns != null)
			{
				foreach (var p in _patterns)
					if (p.Type != ElaNodeType.FieldPattern)
						isRec = false;
			}
			else
				isRec = false;

			return isRec;
		}
		#endregion


		#region Properties
		private List<ElaPattern> _patterns;
		public List<ElaPattern> Patterns
		{
			get
			{
				if (_patterns == null)
					_patterns = new List<ElaPattern>();

				return _patterns;
			}
		}


		public bool HasChildren
		{
			get { return _patterns != null; }
		}


		internal override ElaPatternAffinity Affinity
		{
			get { return ElaPatternAffinity.Sequence; }
		}
		#endregion
	}
}