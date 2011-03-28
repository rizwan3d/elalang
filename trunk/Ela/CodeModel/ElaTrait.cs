using System;
using System.Collections.Generic;
using Ela.Parsing;
using System.Text;

namespace Ela.CodeModel
{
    public sealed class ElaTrait : ElaExpression
    {
        #region Construction
        internal ElaTrait(Token tok) : base(tok, ElaNodeType.Trait)
        {
            Functions = new List<String>();
        }


        public ElaTrait() : this(null)
        {

        }
        #endregion


        #region Methods
        internal override void ToString(StringBuilder sb, Fmt fmt)
        {
            sb.Append("trait ");

            foreach (var s in Functions)
            {
                sb.Append(s);
                sb.Append(' ');
            }
        }
        #endregion


        #region Properties
        public List<String> Functions { get; private set; }
        
        public string Name { get; set; }
        #endregion
    }
}