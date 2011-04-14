using System;
using System.Text;
using Ela.Parsing;

namespace Ela.CodeModel
{
    public sealed class ElaAttribute : ElaExpression
    {
        #region Construction
        internal ElaAttribute(Token tok) : base(tok, ElaNodeType.Attribute)
        {

        }


        public ElaAttribute() : base(ElaNodeType.Attribute)
        {

        }
        #endregion


        #region Methods
        internal override void ToString(StringBuilder sb, Fmt fmt)
        {
            sb.Append('@');
            sb.Append(Name);

            if (Local)
                sb.Append(":let");

            sb.Append(' ');
            sb.Append(Value);
        }
        #endregion


        #region Properties
        public string Name { get; set; }

        public string Value { get; set; }

        public bool Local { get; set; }
        #endregion
    }
}