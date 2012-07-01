using System;
using Ela.CodeModel;
using Ela.Parsing;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class RecordInstance : Class
    {
        internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            left = left.Ref.Force(left, ctx);
            right = right.Ref.Force(right, ctx);

            if (right.TypeId != ElaMachine.REC)
            {
                ctx.InvalidOperand(left, right, "concatenate");
                return Default();
            }

            return new ElaValue(ElaRecord.Concat((ElaRecord)left.Ref, (ElaRecord)right.Ref));
        }

        internal override ElaValue GetLength(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(((ElaRecord)value.Ref).Length);
        }

        internal override ElaValue GetValue(ElaValue value, ElaValue index, ExecutionContext ctx)
        {
            return ((ElaRecord)value.Ref).GetValue(index, ctx);
        }
    }
}
