using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    public struct ExportVarData
    {
        public readonly ElaVariableFlags Flags;
        public readonly ElaBuiltinKind Kind;
        public readonly int ModuleHandle;
        public readonly int Address;

        internal ExportVarData(ElaBuiltinKind kind, ElaVariableFlags flags, int moduleHandle, int address)
        {
            Kind = kind;
            ModuleHandle = moduleHandle;
            Address = address;
            Flags = flags;
        }
    }
}
