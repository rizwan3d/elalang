using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    public struct ExportVarData
    {
        public readonly ElaBuiltinKind Kind;
        public readonly CallConv CallConv;

        internal ExportVarData(ElaBuiltinKind kind, CallConv conv)
        {
            Kind = kind;
            CallConv = conv;
        }
    }
}
