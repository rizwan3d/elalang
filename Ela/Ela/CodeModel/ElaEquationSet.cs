using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public sealed class ElaEquationSet : ElaExpression
	{
		public ElaEquationSet() : base(null, ElaNodeType.EquationSet)
		{
            Equations = new List<ElaEquation>();
		}

        internal override bool Safe()
        {
            foreach (var e in Equations)
                if (!e.Safe())
                    return false;

            return true;
        }
		
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
            foreach (var e in Equations)
            {
                e.ToString(sb, fmt);
                sb.AppendLine();
            }
		}
		
		public List<ElaEquation> Equations { get; private set; }
	}
}