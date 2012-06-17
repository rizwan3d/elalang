using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Classes
{
    internal sealed class LazyInstance : Class
    {
        internal override ElaValue Construct(ElaValue instance, ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(new ElaLazyList((ElaLazy)instance.Ref, value));
        }

        internal override ElaValue Nil(ElaValue value, ExecutionContext ctx)
        {
            return new ElaValue(ElaList.Empty);
        }
    }
}
