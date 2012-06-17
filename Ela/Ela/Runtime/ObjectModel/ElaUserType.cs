using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
    public sealed class ElaUserType : ElaObject
    {
        private readonly string typeName;

        internal ElaUserType(string typeName, int typeCode, ElaValue value) : base((ElaTypeCode)typeCode)
        {
            this.typeName = typeName;
            Value = value;
        }

        protected internal override string GetTypeName()
        {
            return typeName;
        }

        internal override string GetTag(ExecutionContext ctx)
        {
            return Value.Ref.GetTag(ctx);
        }

        internal override ElaValue Untag(ExecutionContext ctx)
        {
            return Value.Ref.Untag(ctx);
        }

        internal ElaValue Value { get; private set; }
    }
}
