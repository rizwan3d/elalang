using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    public struct ExportVarData
    {
        public readonly ElaBuiltinKind Kind;
        public readonly CallConv CallConv;
        public readonly int ModuleHandle;
        public readonly int Address;

        internal ExportVarData(ElaBuiltinKind kind, CallConv conv, int moduleHandle, int address)
        {
            Kind = kind;
            CallConv = conv;
            ModuleHandle = moduleHandle;
            Address = address;
        }
    }
}
