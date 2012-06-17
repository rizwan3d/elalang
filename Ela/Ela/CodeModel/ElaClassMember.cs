using System;
using System.Collections.Generic;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
    public sealed class ElaClassMember : ElaExpression
    {
        #region Construction
        internal ElaClassMember(Token tok) : base(tok, ElaNodeType.TypeClass)
        {
            
        }


        public ElaClassMember() : this(null)
        {

        }
        #endregion


        #region Methods
        internal override void ToString(StringBuilder sb, Fmt fmt)
        {
            sb.Append(Name);
            sb.Append("::");

            for (var i = 0; i < Arguments; i++)
            {
                if (i > 0)
                    sb.Append("->");

                if ((Mask & (1 << i)) == (1 << i))
                    sb.Append("a");
                else
                    sb.Append("_");
            }
        }
        #endregion


        #region Properties
        public string Name { get; set; }

        public int Arguments { get; set; }

        public int Mask { get; set; }
        #endregion
    }
}