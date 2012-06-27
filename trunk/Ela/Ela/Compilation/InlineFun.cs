using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    internal sealed class InlineFun
    {
        internal InlineFun(ElaExpression fun, Scope scope)
        {
            Literal = fun;
            Scope = scope;
        }

        internal readonly ElaExpression Literal;
        internal readonly Scope Scope;
    }
}
