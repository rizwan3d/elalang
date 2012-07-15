using System;
using System.Collections.Generic;
using System.Text;
using Ela.Linking;

namespace Ela.Runtime.ObjectModel
{
    public sealed class ElaUserType : ElaObject
    {
        private readonly string typeName;
        private readonly int tag;

        internal ElaUserType(string typeName, int typeCode, int tag, ElaValue value) : base((ElaTypeCode)typeCode)
        {
            this.typeName = typeName;
            this.tag = tag;

            if (value.TypeId != ElaMachine.UNI)
                Values = ((ElaTuple)value.Ref).Values;
        }

        protected internal override string GetTypeName()
        {
            return typeName;
        }

        internal override int GetTag(ExecutionContext ctx)
        {
            return tag;
        }

        internal override ElaValue Untag(CodeAssembly asm, ExecutionContext ctx, int index)
        {
            if (Values == null || index >= Values.Length)
            {
                ctx.Fail(new ElaError(ElaRuntimeError.InvalidTypeArgument,
                    asm.Constructors[tag], typeName, index + 1));
                return Default();
            }

            return Values[index];
        }

        internal ElaValue[] Values { get; private set; }
    }
}
