using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaVariablePattern : ElaPattern
	{
		#region Construction
		internal ElaVariablePattern(Token tok) : base(tok, ElaNodeType.VariablePattern)
		{

		}


		public ElaVariablePattern() : base(ElaNodeType.VariablePattern)
		{

		}
		#endregion


		#region Methods
        internal override string GetName()
        {
            return Name;
        }


		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
            if (Name[0] != '$')
            {
                if (Format.IsSymbolic(Name))
                    sb.AppendFormat("({0})", Name);
                else
                    sb.Append(Name);
            }
		}


		internal override bool IsIrrefutable()
		{
			return true;
		}


		internal override bool CanFollow(ElaPattern pat)
		{
			return !pat.IsIrrefutable();
		}
		#endregion


		#region Properties
		public string Name { get; set; }
		#endregion
	}
}
