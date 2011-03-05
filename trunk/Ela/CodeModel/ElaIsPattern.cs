﻿using System;
using Ela.Parsing;
using System.Text;

namespace Ela.CodeModel
{
	public sealed class ElaIsPattern : ElaPattern
	{
		#region Construction
		internal ElaIsPattern(Token tok) : base(tok, ElaNodeType.IsPattern)
		{

		}


		internal ElaIsPattern() : base(ElaNodeType.IsPattern)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return Traits != ElaTraits.None ? FormatTraitList() : TypeCodeFormat.GetShortForm(TypeAffinity);
		}


		private string FormatTraitList()
		{
			var sb = new StringBuilder();
			sb.Append('<');
			sb.Append(Traits.ToString().Replace('|', ','));
			sb.Append('>');
			return sb.ToString();
		}
		#endregion


		#region Properties
		public ElaTypeCode TypeAffinity { get; set; }

		public ElaTraits Traits { get; set; }
		#endregion
	}
}
