using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class ModuleInstance : Class
    {
        internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.MOD)
            {
                NoOverloadBinary(TCF.MOD, right, "equal", ctx);
                return false;
            }

            return ((ElaModule)left.Ref).Handle == ((ElaModule)right.Ref).Handle;
        }

        internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (right.TypeId != ElaMachine.MOD)
            {
                NoOverloadBinary(TCF.MOD, right, "notequal", ctx);
                return false;
            }

            return ((ElaModule)left.Ref).Handle != ((ElaModule)right.Ref).Handle;
        }

        internal override ElaValue GetLength(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(((ElaModule)value.Ref).GetVariableCount(ctx));
        }

        internal override ElaValue GetValue(ElaValue value, ElaValue index, ExecutionContext ctx)
        {
            return ((ElaModule)value.Ref).GetValue(index, ctx);
        }
    }
}
