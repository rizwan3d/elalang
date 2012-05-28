using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    public struct ExportVarData
    {
        public readonly ElaBuiltinKind Kind;
        public readonly int ModuleHandle;
        public readonly int Address;

        internal ExportVarData(ElaBuiltinKind kind, int moduleHandle, int address)
        {
            Kind = kind;
            ModuleHandle = moduleHandle;
            Address = address;
        }
    }
}
