using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    internal sealed class InlineFun
    {
        internal InlineFun(ElaFunctionLiteral fun, Scope scope)
        {
            Literal = fun;
            Scope = scope;
        }

        internal readonly ElaFunctionLiteral Literal;
        internal readonly Scope Scope;
    }
}
